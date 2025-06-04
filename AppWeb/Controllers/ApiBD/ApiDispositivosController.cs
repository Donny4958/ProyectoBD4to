using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Dispositivos")]
    public class ApiDispositivosController : ApiController
    {
        [HttpGet]
        [Route("ObtenerTodos")]
        public HttpResponseMessage ObtenerTodos()
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable(@"
                SELECT 
                    d.id_dispositivo,
                    d.rfid,
                    d.nombre,
                    d.descripcion,
                    d.foto,
                    d.clave_producto,
                    d.fecha_adquisicion,
                    d.costo,
                    d.donado,
                    d.proveedor,
                    d.id_laboratorio,
                    l.nombre AS laboratorio_nombre,
                    l.descripcion AS laboratorio_descripcion
                FROM DISPOSITIVOS d
                LEFT JOIN LABORATORIOS l ON d.id_laboratorio = l.id_laboratorio
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_dispositivo = row["id_dispositivo"],
                    rfid = row["rfid"],
                    nombre = row["nombre"],
                    descripcion = row["descripcion"],
                    foto = row["foto"] == DBNull.Value ? null : row["foto"].ToString(),
                    clave_producto = row["clave_producto"],
                    fecha_adquisicion = row["fecha_adquisicion"] == DBNull.Value ? null : Convert.ToDateTime(row["fecha_adquisicion"]).ToString("yyyy-MM-dd"),
                    costo = row["costo"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["costo"]),
                    donado = row["donado"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["donado"]),
                    proveedor = row["proveedor"],
                    id_laboratorio = row["id_laboratorio"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["id_laboratorio"]),
                    laboratorio = new
                    {
                        nombre = row["laboratorio_nombre"] == DBNull.Value ? null : row["laboratorio_nombre"].ToString(),
                        descripcion = row["laboratorio_descripcion"] == DBNull.Value ? null : row["laboratorio_descripcion"].ToString()
                    }
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }

        [HttpGet]
        [Route("ObtenerPorId")]
        public HttpResponseMessage ObtenerPorId(int id)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($@"
                SELECT 
                    d.id_dispositivo,
                    d.rfid,
                    d.nombre,
                    d.descripcion,
                    d.foto,
                    d.clave_producto,
                    d.fecha_adquisicion,
                    d.costo,
                    d.donado,
                    d.proveedor,
                    d.id_laboratorio,
                    l.nombre AS laboratorio_nombre,
                    l.descripcion AS laboratorio_descripcion
                FROM DISPOSITIVOS d
                LEFT JOIN LABORATORIOS l ON d.id_laboratorio = l.id_laboratorio
                WHERE d.id_dispositivo = {id}
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var dispositivo = new
                {
                    id_dispositivo = row["id_dispositivo"],
                    rfid = row["rfid"],
                    nombre = row["nombre"],
                    descripcion = row["descripcion"],
                    foto = row["foto"] == DBNull.Value ? null : row["foto"].ToString(),
                    clave_producto = row["clave_producto"],
                    fecha_adquisicion = row["fecha_adquisicion"] == DBNull.Value ? null : Convert.ToDateTime(row["fecha_adquisicion"]).ToString("yyyy-MM-dd"),
                    costo = row["costo"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["costo"]),
                    donado = row["donado"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(row["donado"]),
                    proveedor = row["proveedor"],
                    id_laboratorio = row["id_laboratorio"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["id_laboratorio"]),
                    laboratorio = new
                    {
                        nombre = row["laboratorio_nombre"] == DBNull.Value ? null : row["laboratorio_nombre"].ToString(),
                        descripcion = row["laboratorio_descripcion"] == DBNull.Value ? null : row["laboratorio_descripcion"].ToString()
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, dispositivo);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Dispositivo no encontrado");
        }

        [HttpPost]
        [Route("Crear")]
        public HttpResponseMessage Crear([FromBody] dynamic datos)
        {
            try
            {
                string rfid = datos.rfid;
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;
                string foto = datos.foto;
                string clave_producto = datos.clave_producto;
                string fecha_adquisicion = datos.fecha_adquisicion;
                decimal? costo = datos.costo != null ? (decimal)datos.costo : (decimal?)null;
                bool? donado = datos.donado != null ? (bool)datos.donado : (bool?)null;
                string proveedor = datos.proveedor;
                int? id_laboratorio = datos.id_laboratorio != null ? (int)datos.id_laboratorio : (int?)null;

                if (string.IsNullOrEmpty(rfid) || string.IsNullOrEmpty(nombre))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos obligatorios faltantes");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "rfid", rfid },
                    { "nombre", nombre },
                    { "descripcion", descripcion },
                    { "foto", foto },
                    { "clave_producto", clave_producto },
                    { "fecha_adquisicion", fecha_adquisicion },
                    { "costo", costo },
                    { "donado", donado.HasValue ? (donado.Value ? 1 : 0) : (object)DBNull.Value },
                    { "proveedor", proveedor },
                    { "id_laboratorio", id_laboratorio }
                };
                bool insertado = db.InsertRow("DISPOSITIVOS", data);
                if (insertado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Dispositivo creado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al crear dispositivo");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [Route("ModificarParcial")]
        public HttpResponseMessage ModificarParcial([FromBody] dynamic datos)
        {
            try
            {
                int id_dispositivo = datos.id_dispositivo;
                var db = new DataBaseHelper();

                var campos = new Dictionary<string, object>();

                // Solo agrega los campos que vienen en el body
                if (datos.descripcion != null)
                    campos["descripcion"] = (string)datos.descripcion;
                if (datos.foto != null)
                    campos["foto"] = (string)datos.foto;
                if (datos.costo != null)
                    campos["costo"] = (decimal)datos.costo;

                if (campos.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No se enviaron campos para modificar.");

                bool actualizado = db.UpdateRow("DISPOSITIVOS", campos, $"id_dispositivo={id_dispositivo}");
                if (actualizado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Componente modificado correctamente.");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el componente.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpDelete]
        [Route("Eliminar")]
        public HttpResponseMessage Eliminar(int id)
        {
            try
            {
                var db = new DataBaseHelper();
                // Verificar si hay préstamos activos (fecha_regreso es NULL)
                var dt = db.SelectTable($@"
                    SELECT 1 FROM PRESTAMOS 
                    WHERE id_dispositivo = {id} AND fecha_regreso IS NULL
                ");
                if (dt.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No se puede eliminar el dispositivo porque tiene préstamos activos.");
                }

                bool eliminado = db.DeleteRow("DISPOSITIVOS", $"id_dispositivo={id}");
                if (eliminado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Dispositivo eliminado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el dispositivo");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
