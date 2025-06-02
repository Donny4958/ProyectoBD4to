using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Equipos")]
    public class EquiposController : ApiController
    {
        // Obtener todos los equipos con laboratorio y responsable
        [HttpGet]
        [Route("ObtenerTodos")]
        public HttpResponseMessage ObtenerTodos()
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable(@"
                SELECT 
                    E.ID, 
                    E.nombre, 
                    E.Laboratorio, 
                    L.Nombre AS NombreLaboratorio, 
                    E.Responsable, 
                    U.Nombre AS NombreResponsable
                FROM Equipos E
                LEFT JOIN Laboratorios L ON E.Laboratorio = L.ID
                LEFT JOIN Usuarios U ON E.Responsable = U.ID
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id = row["ID"].ToString(),
                    nombre = row["nombre"].ToString(),
                    laboratorioId = row["Laboratorio"]?.ToString(),
                    laboratorio = row["NombreLaboratorio"]?.ToString(),
                    responsableId = row["Responsable"]?.ToString(),
                    responsable = row["NombreResponsable"]?.ToString()
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }

        // Obtener un equipo por ID
        [HttpGet]
        [Route("ObtenerPorId")]
        public HttpResponseMessage ObtenerPorId(int id)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($@"
                SELECT 
                    E.ID, 
                    E.nombre, 
                    E.Laboratorio, 
                    L.Nombre AS NombreLaboratorio, 
                    E.Responsable, 
                    U.Nombre AS NombreResponsable,
                    GROUP_CONCAT(P.Nombre SEPARATOR ', ') AS Proyectos
                FROM Equipos E
                LEFT JOIN Laboratorios L ON E.Laboratorio = L.ID
                LEFT JOIN Proyectos P ON P.Equipo = E.ID
                LEFT JOIN Usuarios U ON E.Responsable = U.ID
                WHERE E.ID = {id}
                GROUP BY E.ID, E.nombre, E.Laboratorio, L.Nombre, E.Responsable, U.Nombre
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var equipo = new
                {
                    id = row["ID"].ToString(),
                    nombre = row["nombre"].ToString(),
                    laboratorioId = row["Laboratorio"]?.ToString(),
                    laboratorio = row["NombreLaboratorio"]?.ToString(),
                    responsableId = row["Responsable"]?.ToString(),
                    responsable = row["NombreResponsable"]?.ToString(),
                    Proyectos = row["Proyectos"]?.ToString()
                };
                return Request.CreateResponse(HttpStatusCode.OK, equipo);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Equipo no encontrado");
        }

        // Crear equipo
        [HttpPost]
        [Route("Crear")]
        public HttpResponseMessage Crear([FromBody] dynamic datos)
        {
            try
            {
                string nombre = datos.nombre;
                int? laboratorio = datos.laboratorioId != null ? (int?)datos.laboratorioId : null;
                int responsable = datos.responsableId;

                if (string.IsNullOrEmpty(nombre) || responsable <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "nombre", nombre },
                    { "Responsable", responsable }
                };
                if (laboratorio.HasValue)
                    data.Add("Laboratorio", laboratorio.Value);

                bool insertado = db.InsertRow("Equipos", data);
                if (insertado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Equipo creado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al crear equipo");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Modificar equipo
        [HttpPut]
        [Route("Modificar")]
        public HttpResponseMessage Modificar([FromBody] dynamic datos)
        {
            try
            {
                int id = datos.id;
                string nombre = datos.nombre;
                int? laboratorio = datos.laboratorioId != null ? (int?)datos.laboratorioId : null;
                int responsable = datos.responsableId;

                if (id <= 0 || string.IsNullOrEmpty(nombre) || responsable <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "nombre", nombre },
                    { "Responsable", responsable }
                };
                if (laboratorio.HasValue)
                    data.Add("Laboratorio", laboratorio.Value);

                bool actualizado = db.UpdateRow("Equipos", data, $"ID={id}");
                if (actualizado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Equipo modificado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el equipo o no se modificó");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Eliminar equipo
        [HttpDelete]
        [Route("Eliminar")]
        public HttpResponseMessage Eliminar(int id)
        {
            try
            {
                var db = new DataBaseHelper();
                bool eliminado = db.DeleteRow("Equipos", $"ID={id}");
                if (eliminado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Equipo eliminado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el equipo");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Obtener proyectos de un equipo
        [HttpGet]
        [Route("ObtenerProyectosPorEquipo")]
        public HttpResponseMessage ObtenerProyectosPorEquipo(int idEquipo)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($@"
                SELECT 
                    P.ID, 
                    P.Nombre, 
                    P.Descripcion
                FROM Proyectos P
                WHERE P.Equipo = {idEquipo}
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id = row["ID"].ToString(),
                    nombre = row["Nombre"].ToString(),
                    descripcion = row["Descripcion"].ToString()
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }
    }
}
