using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Proyectos")]
    public class ProyectosController : ApiController
    {
        [HttpGet]
        [Route("ObtenerTodos")]
        public HttpResponseMessage ObtenerTodos()
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable(@"
                SELECT 
                    P.id_proyecto, 
                    P.nombrep, 
                    P.descripcion, 
                    P.id_responsable,
                    U.nom_usuario AS nombre_responsable
                FROM PROYECTOS P
                LEFT JOIN USUARIO U ON P.id_responsable = U.id_usuario
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id = row["id_proyecto"],
                    nombre = row["nombrep"],
                    descripcion = row["descripcion"],
                    id_responsable = row["id_responsable"],
                    responsable = row["nombre_responsable"]
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
                    P.id_proyecto, 
                    P.nombrep, 
                    P.descripcion, 
                    P.id_responsable,
                    U.nom_usuario AS nombre_responsable
                FROM PROYECTOS P
                LEFT JOIN USUARIO U ON P.id_responsable = U.id_usuario
                WHERE P.id_proyecto = {id}
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var proyecto = new
                {
                    id = row["id_proyecto"],
                    nombre = row["nombrep"],
                    descripcion = row["descripcion"],
                    id_responsable = row["id_responsable"],
                    responsable = row["nombre_responsable"]
                };
                return Request.CreateResponse(HttpStatusCode.OK, proyecto);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Proyecto no encontrado");
        }

        [HttpPost]
        [Route("Crear")]
        public HttpResponseMessage Crear([FromBody] dynamic datos)
        {
            try
            {
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;
                int id_responsable = datos.id_responsable;

                if (string.IsNullOrEmpty(nombre) || id_responsable <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "nombrep", nombre },
                    { "descripcion", descripcion },
                    { "id_responsable", id_responsable }
                };
                bool insertado = db.InsertRow("PROYECTOS", data);
                if (insertado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Proyecto creado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al crear proyecto");
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
            try
            {
                int id = datos.id;
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;
                int id_responsable = datos.id_responsable;

                if (id <= 0 || string.IsNullOrEmpty(nombre) || id_responsable <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "nombrep", nombre },
                    { "descripcion", descripcion },
                    { "id_responsable", id_responsable }
                };
                bool actualizado = db.UpdateRow("PROYECTOS", data, $"id_proyecto={id}");
                if (actualizado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Proyecto modificado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el proyecto o no se modificó");
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
                bool eliminado = db.DeleteRow("PROYECTOS", $"id_proyecto={id}");
                if (eliminado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Proyecto eliminado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el proyecto");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("UsuariosPorProyecto")]
        public HttpResponseMessage UsuariosPorProyecto(int idProyecto)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($@"
                SELECT U.id_usuario, U.nom_usuario, U.correo, U.rol
                FROM PROYECTO_USUARIO PU
                INNER JOIN USUARIO U ON PU.id_usuario = U.id_usuario
                WHERE PU.id_proyecto = {idProyecto}
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_usuario = row["id_usuario"],
                    nombre = row["nom_usuario"],
                    correo = row["correo"],
                    rol = row["rol"]
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }

        [HttpPost]
        [Route("AgregarUsuarioAProyecto")]
        public HttpResponseMessage AgregarUsuarioAProyecto([FromBody] dynamic datos)
        {
            try
            {
                int id_usuario = datos.id_usuario;
                int id_proyecto = datos.id_proyecto;

                if (id_usuario <= 0 || id_proyecto <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "id_usuario", id_usuario },
                    { "id_proyecto", id_proyecto }
                };
                bool insertado = db.InsertRow("PROYECTO_USUARIO", data);
                if (insertado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Usuario agregado al proyecto correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al agregar usuario al proyecto");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
