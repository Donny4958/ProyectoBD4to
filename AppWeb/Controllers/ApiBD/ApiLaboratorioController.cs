using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Laboratorios")]
    public class LabsController : ApiController
    {
        [HttpGet]
        [Route("ObtenerLaboratorios")]
        public HttpResponseMessage ObtenerLaboratorios()
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable(@"
                SELECT 
                    L.id_laboratorio AS Id, 
                    L.nombre AS Nombre, 
                    L.descripcion, 
                    U.nombre AS Responsable,
                    U.id_usuario ResponsableId
                FROM LABORATORIOS L
                LEFT JOIN USUARIOS U ON L.id_responsable = U.id_usuario
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    Id = row["Id"].ToString(),
                    nombre = row["Nombre"].ToString(),
                    descripcion = row["descripcion"].ToString(),
                    Responsable = row["Responsable"]?.ToString()
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }

        [HttpGet]
        [Route("ObtenerLaboratorioPorId")]
        public HttpResponseMessage ObtenerLaboratorioPorId(int id)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($@"
                SELECT id_laboratorio, nombre, descripcion, id_responsable 
                FROM LABORATORIOS 
                WHERE id_laboratorio={id}
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var laboratorio = new
                {
                    id_laboratorio = row["id_laboratorio"].ToString(),
                    nombre = row["nombre"].ToString(),
                    descripcion = row["descripcion"].ToString(),
                    id_responsable = row["id_responsable"]?.ToString()
                };
                return Request.CreateResponse(HttpStatusCode.OK, laboratorio);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Laboratorio no encontrado");
        }

        [HttpPost]
        [Route("CrearLaboratorio")]
        public HttpResponseMessage CrearLaboratorio([FromBody] dynamic datos)
        {
            try
            {
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;
                string id_responsable = datos.id_responsable;

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(id_responsable))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "nombre", nombre },
                    { "descripcion", descripcion },
                    { "id_responsable", id_responsable }
                };
                bool insertado = db.InsertRow("LABORATORIOS", data);
                if (insertado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Laboratorio creado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al crear laboratorio");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [Route("ModificarLaboratorio")]
        public HttpResponseMessage ModificarLaboratorio([FromBody] dynamic datos)
        {
            try
            {
                int id = datos.id_laboratorio;
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;
                string id_responsable = datos.id_responsable;

                if (id <= 0 || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(id_responsable))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "nombre", nombre },
                    { "descripcion", descripcion },
                    { "id_responsable", id_responsable }
                };
                bool actualizado = db.UpdateRow("LABORATORIOS", data, $"id_laboratorio={id}");
                if (actualizado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Laboratorio modificado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el laboratorio o no se modificó");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Route("EliminarLaboratorio")]
        public HttpResponseMessage EliminarLaboratorio(int id)
        {
            try
            {
                var db = new DataBaseHelper();
                bool eliminado = db.DeleteRow("LABORATORIOS", $"id_laboratorio={id}");
                if (eliminado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Laboratorio eliminado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el laboratorio");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("ObtenerProfesoresOEmpleados")]
        public HttpResponseMessage ObtenerProfesoresOEmpleados()
        {
            try
            {
                var db = new DataBaseHelper();
                var dt = db.SelectTable("SELECT id_usuario ID, nombre Nombre, rol Rol FROM USUARIOS WHERE rol IN ('profesor', 'empleado');");
                var lista = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new
                    {
                        ID = row["ID"].ToString(),
                        Rol = row["Rol"].ToString(),
                        Nombre = row["Nombre"].ToString(),
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, lista);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("ObtenerPorLaboratorio")]
        public HttpResponseMessage ObtenerPorLaboratorio(int idLaboratorio)
        {
            try
            {
                var db = new DataBaseHelper();
                var dt = db.SelectTable($@"
                    SELECT 
                        P.id_proyecto, 
                        P.nombrep AS Nombre, 
                        P.descripcion AS Descripcion, 
                        U.nombre AS Responsable
                    FROM PROYECTOS P
                    LEFT JOIN USUARIOS U ON P.id_responsable = U.id_usuario
                    WHERE P.id_laboratorio = {idLaboratorio}
                ");
                var lista = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new
                    {
                        id_proyecto = row["id_proyecto"].ToString(),
                        Nombre = row["Nombre"].ToString(),
                        Descripcion = row["Descripcion"].ToString(),
                        Responsable = row["Responsable"]?.ToString()
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, lista);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
