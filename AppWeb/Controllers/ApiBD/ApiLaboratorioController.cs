using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;

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
            var dt = db.SelectTable("SELECT Laboratorios.ID,Laboratorios.Nombre,Laboratorios.Descripcion, Usuarios.Nombre Res FROM Laboratorios INNER JOIN Usuarios ON Laboratorios.ResponsableID = Usuarios.ID;");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    Id = row["ID"].ToString(),
                    nombre = row["Nombre"].ToString(),
                    descripcion = row["Descripcion"].ToString(),
                    Responsable = row["Res"].ToString()
                });
            }
            return Request.CreateResponse(HttpStatusCode.OK, lista);
        }

        [HttpGet]
        [Route("ObtenerLaboratorioPorId")]
        public HttpResponseMessage ObtenerLaboratorioPorId(int id)
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable($"SELECT id_laboratorio, nombre, descripcion FROM Laboratorio WHERE id_laboratorio={id}");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var laboratorio = new
                {
                    id_laboratorio = row["id_laboratorio"].ToString(),
                    nombre = row["nombre"].ToString(),
                    descripcion = row["descripcion"].ToString()
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
                string responsable = datos.responsable;

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(descripcion)|| string.IsNullOrEmpty(responsable))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
        {
            { "Nombre", nombre },
            { "Descripcion", descripcion },
            { "ResponsableID", responsable }
        };
                bool insertado = db.InsertRow("Laboratorios", data);
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
        [HttpDelete]
        [Route("EliminarLaboratorio")]
        public HttpResponseMessage EliminarLaboratorio(int id)
        {
            try
            {
                var db = new DataBaseHelper();
                bool eliminado = db.DeleteRow("Laboratorio", $"id_laboratorio={id}");
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
                var dt = db.SelectTable("SELECT ID,Nombre,rol FROM Usuarios WHERE rol IN ('profesor', 'empleado');");
                var lista = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new
                    {
                        ID = row["ID"].ToString(),
                        Rol = row["rol"].ToString(),
                        Nombre= row["Nombre"].ToString(),
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
                // Ajusta los nombres de tabla y campos según tu modelo
                var dt = db.SelectTable($"select * from Proyectos where Laboratorio= {idLaboratorio}");
                var lista = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new
                    {
                        Nombre = row["Nombre"].ToString(),
                        Descripcion = row["Descripcion"].ToString(),
                        Responsable = row["Responsable"].ToString(),
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, lista);
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
                string responsable = datos.responsable;

                if (id < 0 || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(descripcion)|| string.IsNullOrEmpty(responsable))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
        {
            { "Nombre", nombre },
            { "Descripcion", descripcion },
            { "ResponsableID", responsable }
        };
                bool actualizado = db.UpdateRow("Laboratorios", data, $"ID={id}");
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

    }

}