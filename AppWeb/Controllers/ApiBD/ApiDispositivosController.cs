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
                    d.claveproducto,
                    d.fechaadquisicion,
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
                    claveproducto = row["claveproducto"],
                    fechaadquisicion = row["fechaadquisicion"] == DBNull.Value ? null : Convert.ToDateTime(row["fechaadquisicion"]).ToString("yyyy-MM-dd"),
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
                    d.claveproducto,
                    d.fechaadquisicion,
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
                    claveproducto = row["claveproducto"],
                    fechaadquisicion = row["fechaadquisicion"] == DBNull.Value ? null : Convert.ToDateTime(row["fechaadquisicion"]).ToString("yyyy-MM-dd"),
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
                string claveproducto = datos.claveproducto;
                string fechaadquisicion = datos.fechaadquisicion;
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
                    { "claveproducto", claveproducto },
                    { "fechaadquisicion", fechaadquisicion },
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
        [Route("Modificar")]
        public HttpResponseMessage Modificar([FromBody] dynamic datos)
        {
            // Edición deshabilitada
            return Request.CreateResponse(HttpStatusCode.Forbidden, "No se permite modificar un dispositivo una vez creado.");
        }

        [HttpDelete]
        [Route("Eliminar")]
        public HttpResponseMessage Eliminar(int id)
        {
            try
            {
                var db = new DataBaseHelper();
                // Verificar si hay préstamos activos (fechaRegreso es NULL)
                var dt = db.SelectTable($@"
                    SELECT 1 FROM PRESTAMOS 
                    WHERE id_prestamo IN (
                        SELECT id_prestamo FROM PRESTAMOS_DISPOSITIVOS WHERE id_dispositivo = {id}
                    ) AND fechaRegreso IS NULL
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
