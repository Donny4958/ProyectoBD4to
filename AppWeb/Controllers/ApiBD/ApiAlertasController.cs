using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Alertas")]
    public class ApiAlertasController : ApiController
    {
        // GET api/Alertas/Prestamos
        [HttpGet]
        [Route("Prestamos")]
        public IHttpActionResult GetAllPrestamos()
        {
            var db = new DataBaseHelper();
            // JOIN para obtener información completa del dispositivo relacionado con la alerta
            var dt = db.SelectTable(@"
                SELECT 
                    A.id_alerta,
                    A.rfid,
                    A.momento,
                    A.ubicacion,
                    D.id_dispositivo,
                    D.nombre,
                    D.descripcion,
                    D.foto,
                    D.clave_producto,
                    D.fecha_adquisicion,
                    D.costo,
                    D.donado,
                    D.proveedor,
                    D.id_laboratorio
                FROM ALERTAS A
                INNER JOIN DISPOSITIVOS D ON A.rfid = D.rfid
                ORDER BY A.momento DESC
            ");

            var alertas = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                alertas.Add(new
                {
                    id_alerta = row["id_alerta"],
                    rfid = row["rfid"]?.ToString(),
                    momento = row["momento"] != DBNull.Value ? Convert.ToDateTime(row["momento"]) : (DateTime?)null,
                    ubicacion = row["ubicacion"]?.ToString(),
                    dispositivo = new
                    {
                        id_dispositivo = row["id_dispositivo"],
                        rfid = row["rfid"]?.ToString(),
                        nombre = row["nombre"]?.ToString(),
                        descripcion = row["descripcion"]?.ToString(),
                        foto = row["foto"]?.ToString(),
                        clave_producto = row["clave_producto"]?.ToString(),
                        fecha_adquisicion = row["fecha_adquisicion"] != DBNull.Value ? Convert.ToDateTime(row["fecha_adquisicion"]) : (DateTime?)null,
                        costo = row["costo"] != DBNull.Value ? Convert.ToDecimal(row["costo"]) : (decimal?)null,
                        donado = row["donado"] != DBNull.Value ? Convert.ToBoolean(row["donado"]) : (bool?)null,
                        proveedor = row["proveedor"]?.ToString(),
                        id_laboratorio = row["id_laboratorio"]
                    }
                });
            }

            return Ok(alertas);
        }
    }
}
