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
                    P.ID, 
                    P.Nombre, 
                    P.Descripcion, 
                    L.Nombre AS NombreLaboratorio, 
                    U.Nombre AS NombreResponsable,
                    E.Nombre AS NombreEquipo
                FROM Proyectos P
                LEFT JOIN Laboratorios L ON P.Laboratorio = L.ID
                LEFT JOIN Usuarios U ON P.Responsable = U.ID
                LEFT JOIN Equipos E ON P.Equipo = E.ID
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id = row["ID"].ToString(),
                    nombre = row["Nombre"].ToString(),
                    descripcion = row["Descripcion"].ToString(),
                    laboratorio = row["NombreLaboratorio"].ToString(),
                    responsable = row["NombreResponsable"].ToString(),
                    equipo = row["NombreEquipo"]?.ToString()
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
                    P.ID, 
                    P.Nombre, 
                    P.Descripcion, 
                    P.Laboratorio, 
                    L.Nombre AS NombreLaboratorio, 
                    P.Responsable, 
                    U.Nombre AS NombreResponsable,
                    P.Equipo,
                    E.Nombre AS NombreEquipo
                FROM Proyectos P
                LEFT JOIN Laboratorios L ON P.Laboratorio = L.ID
                LEFT JOIN Usuarios U ON P.Responsable = U.ID
                LEFT JOIN Equipos E ON P.Equipo = E.ID
                WHERE P.ID = {id}
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var proyecto = new
                {
                    id = row["ID"].ToString(),
                    nombre = row["Nombre"].ToString(),
                    descripcion = row["Descripcion"].ToString(),
                    laboratorioId = row["Laboratorio"].ToString(),
                    laboratorio = row["NombreLaboratorio"].ToString(),
                    responsableId = row["Responsable"].ToString(),
                    responsable = row["NombreResponsable"].ToString(),
                    equipoId = row["Equipo"].ToString(),
                    equipo = row["NombreEquipo"]?.ToString()
                };
                return Request.CreateResponse(HttpStatusCode.OK, proyecto);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Proyecto no encontrado");
        }

        [HttpGet]
        [Route("ObtenerPorLaboratorio")]
        public HttpResponseMessage ObtenerPorLaboratorio(int idLaboratorio)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($@"
                SELECT 
                    P.ID, 
                    P.Nombre, 
                    P.Descripcion, 
                    P.Laboratorio, 
                    L.Nombre AS NombreLaboratorio, 
                    P.Responsable, 
                    U.Nombre AS NombreResponsable,
                    P.Equipo,
                    E.Nombre AS NombreEquipo
                FROM Proyectos P
                LEFT JOIN Laboratorios L ON P.Laboratorio = L.ID
                LEFT JOIN Usuarios U ON P.Responsable = U.ID
                LEFT JOIN Equipos E ON P.Equipo = E.ID
                WHERE P.Laboratorio = {idLaboratorio}
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id = row["ID"].ToString(),
                    nombre = row["Nombre"].ToString(),
                    descripcion = row["Descripcion"].ToString(),
                    laboratorioId = row["Laboratorio"].ToString(),
                    laboratorio = row["NombreLaboratorio"].ToString(),
                    responsableId = row["Responsable"].ToString(),
                    responsable = row["NombreResponsable"].ToString(),
                    equipoId = row["Equipo"].ToString(),
                    equipo = row["NombreEquipo"]?.ToString()
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }

        [HttpPost]
        [Route("Crear")]
        public HttpResponseMessage Crear([FromBody] dynamic datos)
        {
            try
            {
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;
                int laboratorio = datos.laboratorioId;
                int responsable = datos.responsableId;
                int equipo = datos.equipoId;

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(descripcion) || laboratorio <= 0 || responsable <= 0 || equipo <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "Nombre", nombre },
                    { "Descripcion", descripcion },
                    { "Laboratorio", laboratorio },
                    { "Responsable", responsable },
                    { "Equipo", equipo }
                };
                bool insertado = db.InsertRow("Proyectos", data);
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
                int laboratorio = datos.laboratorioId;
                int responsable = datos.responsableId;
                int equipo = datos.equipoId;

                if (id <= 0 || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(descripcion) || laboratorio < 0 || responsable < 0 || equipo < 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "Nombre", nombre },
                    { "Descripcion", descripcion },
                    { "Laboratorio", laboratorio },
                    { "Responsable", responsable },
                    { "Equipo", equipo }
                };
                bool actualizado = db.UpdateRow("Proyectos", data, $"ID={id}");
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
                bool eliminado = db.DeleteRow("Proyectos", $"ID={id}");
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
    }
}
