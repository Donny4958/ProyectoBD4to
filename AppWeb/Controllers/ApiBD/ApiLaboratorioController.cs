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
            var dt = db.SelectTable("SELECT id_laboratorio, nombre, descripcion FROM Laboratorio");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_laboratorio = row["id_laboratorio"].ToString(),
                    nombre = row["nombre"].ToString(),
                    descripcion = row["descripcion"].ToString()
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

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(descripcion))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
        {
            { "nombre", nombre },
            { "descripcion", descripcion }
        };
                bool insertado = db.InsertRow("Laboratorio", data);
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


        [HttpPut]
        [Route("ModificarLaboratorio")]
        public HttpResponseMessage ModificarLaboratorio([FromBody] dynamic datos)
        {
            try
            {
                int id = datos.id_laboratorio;
                string nombre = datos.nombre;
                string descripcion = datos.descripcion;

                if (id <= 0 || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(descripcion))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
        {
            { "nombre", nombre },
            { "descripcion", descripcion }
        };
                bool actualizado = db.UpdateRow("Laboratorio", data, $"id_laboratorio={id}");
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