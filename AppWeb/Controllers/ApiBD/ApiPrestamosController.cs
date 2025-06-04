using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Prestamos")]
    public class ApiPrestamosController : ApiController
    {
        // 1. Pedir un préstamo
        [HttpPost]
        [Route("Pedir")]
        public IHttpActionResult PedirPrestamo([FromBody] PrestamoRequest request)
        {
            if (request == null || request.id_usuario_prestamo <= 0 || request.id_dispositivo <= 0)
                return BadRequest("Datos incompletos.");

            var db = new DataBaseHelper();
            var data = new Dictionary<string, object>
            {
                {"fecha_salida", DateTime.Now.Date},
                {"fecha_estimada", request.fecha_estimada},
                {"tipo_prestamo", request.tipo_prestamo},
                {"id_usuario_prestamo", request.id_usuario_prestamo},
                {"id_dispositivo", request.id_dispositivo}
            };
            var id = db.InsertRowReturnId("PRESTAMOS", data);

            if (id > 0)
                return Ok("Solicitud de préstamo registrada.");
            return InternalServerError();
        }

        [HttpGet]
        [Route("Revisar")]
        public IHttpActionResult RevisarPrestamos([FromUri] string estado = null, [FromUri] string tipo = null, [FromUri] string Expirado = null)
        {
            var db = new DataBaseHelper();
            var query = @"
                SELECT p.*, u.nom_usuario, d.nombre AS dispositivo_nombre
                FROM PRESTAMOS p
                JOIN USUARIO u ON p.id_usuario_prestamo = u.id_usuario
                JOIN DISPOSITIVOS d ON p.id_dispositivo = d.id_dispositivo
                WHERE 1=1
            ";

            if (!string.IsNullOrEmpty(estado))
            {
                if (estado == "activo")
                {
                    query += " AND p.fecha_regreso IS NULL";
                }
                else if (estado == "inactivo")
                {
                    query += " AND p.fecha_regreso IS NOT NULL";
                }
            }
            if (!string.IsNullOrEmpty(tipo))
            {
                query += $" AND p.tipo_prestamo = '{tipo}'";
            }
            if (!string.IsNullOrEmpty(Expirado))
            {
                var hoy = DateTime.Now.Date.ToString("yyyy-MM-dd");
                query += $" AND p.fecha_estimada < '{hoy}' AND p.fecha_regreso IS NULL";
            }

            var dt = db.SelectTable(query);
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_prestamo = row["id_prestamo"],
                    fecha_salida = row["fecha_salida"],
                    fecha_estimada = row["fecha_estimada"],
                    fecha_regreso = row["fecha_regreso"],
                    tipo_prestamo = row["tipo_prestamo"],
                    id_usuario_prestamo = row["id_usuario_prestamo"],
                    id_login_autorizado = row["id_login_autorizado"],
                    id_dispositivo = row["id_dispositivo"],
                    usuario = row["nom_usuario"],
                    dispositivo = row["dispositivo_nombre"]
                });
            }
            return Ok(lista);
        }

        [HttpPut]
        [Route("Aceptar/{id_prestamo:int}")]
        public IHttpActionResult AceptarPrestamo(int id_prestamo, [FromBody] PrestamoAutorizacionRequest request)
        {
            if (request == null || request.id_autorizador <= 0)
                return BadRequest("Datos incompletos.");

            var db = new DataBaseHelper();
            var query = $@"
                SELECT Autoriza FROM TIPO_USUARIO where id_usuario={request.id_autorizador}
            ";
            var dt = db.SelectTable(query);

            if (dt.Rows.Count == 0 || dt.Rows[0]["Autoriza"].ToString()!="1")
                return NotFound();

            var data = new Dictionary<string, object>
            {
                {"id_login_autorizado", request.id_autorizador}
            };
            var updated = db.UpdateRow("PRESTAMOS", data, $"id_prestamo = {id_prestamo}");

            if (updated)
                return Ok("Préstamo autorizado.");
            return InternalServerError();
        }

        // 4. Préstamos expirados y no regresados
        [HttpGet]
        [Route("Expirados")]
        public IHttpActionResult PrestamosExpirados()
        {
            var db = new DataBaseHelper();
            var hoy = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var query = $@"
                SELECT p.*, u.nom_usuario, d.nombre AS dispositivo_nombre
                FROM PRESTAMOS p
                JOIN USUARIO u ON p.id_usuario_prestamo = u.id_usuario
                JOIN DISPOSITIVOS d ON p.id_dispositivo = d.id_dispositivo
                WHERE p.fecha_estimada < '{hoy}' AND p.fecha_regreso IS NULL
            ";

            var dt = db.SelectTable(query);
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_prestamo = row["id_prestamo"],
                    fecha_salida = row["fecha_salida"],
                    fecha_estimada = row["fecha_estimada"],
                    tipo_prestamo = row["tipo_prestamo"],
                    usuario = row["nom_usuario"],
                    dispositivo = row["dispositivo_nombre"]
                });
            }
            return Ok(lista);
        }

        // 5. Regresar un componente
        [HttpPut]
        [Route("Regresar/{id_prestamo:int}")]
        public IHttpActionResult RegresarComponente(int id_prestamo)
        {
            var db = new DataBaseHelper();
            var data = new Dictionary<string, object>
            {
                {"fecha_regreso", DateTime.Now.Date}
            };
            var updated = db.UpdateRow("PRESTAMOS", data, $"id_prestamo = {id_prestamo} AND fecha_regreso IS NULL");

            if (updated)
                return Ok("Componente regresado.");
            return BadRequest("No se pudo actualizar el préstamo.");
        }
    }

    // Modelos auxiliares para los requests
    public class PrestamoRequest
    {
        public int id_usuario_prestamo { get; set; }
        public int id_dispositivo { get; set; }
        public string tipo_prestamo { get; set; } // "interno" o "externo"
        public DateTime? fecha_estimada { get; set; }
    }

    public class PrestamoAutorizacionRequest
    {
        public int id_autorizador { get; set; }
    }
}