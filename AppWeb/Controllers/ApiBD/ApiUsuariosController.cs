using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppWeb.Controllers.ApiBD
{
    [RoutePrefix("api/Usuarios")]
    public class UsuariosController : ApiController
    {
        [HttpGet]
        [Route("ObtenerTodos")]
        public HttpResponseMessage ObtenerTodos()
        {
            var db = new DataBaseHelper();
            var dt = db.SelectTable(@"
                SELECT 
                    id_usuario, 
                    correo, 
                    rol, 
                    tipoempleado, 
                    matricula, 
                    puede_autorizar, 
                    Nombre,
                    foto
                FROM USUARIOS
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_usuario = row["id_usuario"],
                    correo = row["correo"],
                    rol = row["rol"],
                    tipoempleado = row["tipoempleado"],
                    matricula = row["matricula"],
                    puede_autorizar = Convert.ToBoolean(row["puede_autorizar"]),
                    Nombre = row["Nombre"],
                    foto = row["foto"] == DBNull.Value ? null : row["foto"].ToString()
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
                    id_usuario, 
                    correo, 
                    rol, 
                    tipoempleado, 
                    matricula, 
                    puede_autorizar, 
                    Nombre,
                    foto
                FROM USUARIOS
                WHERE id_usuario = {id}
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var usuario = new
                {
                    id_usuario = row["id_usuario"],
                    correo = row["correo"],
                    rol = row["rol"],
                    tipoempleado = row["tipoempleado"],
                    matricula = row["matricula"],
                    puede_autorizar = Convert.ToBoolean(row["puede_autorizar"]),
                    Nombre = row["Nombre"],
                    foto = row["foto"] == DBNull.Value ? null : row["foto"].ToString()
                };
                return Request.CreateResponse(HttpStatusCode.OK, usuario);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Usuario no encontrado");
        }

        [HttpPost]
        [Route("Crear")]
        public HttpResponseMessage Crear([FromBody] dynamic datos)
        {
            try
            {
                string correo = datos.correo;
                string rol = datos.rol;
                string tipoempleado = datos.tipoempleado;
                string matricula = datos.matricula;
                string password = datos.password;
                bool puede_autorizar = datos.puede_autorizar;
                string nombre = datos.Nombre;
                string foto = datos.foto;

                if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(tipoempleado) || string.IsNullOrEmpty(password))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "correo", correo },
                    { "rol", rol },
                    { "tipoempleado", tipoempleado },
                    { "matricula", matricula },
                    { "password", password },
                    { "puede_autorizar", puede_autorizar ? 1 : 0 },
                    { "Nombre", nombre },
                    { "foto", foto }
                };
                bool insertado = db.InsertRow("USUARIOS", data);
                if (insertado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Usuario creado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al crear usuario");
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
                int id_usuario = datos.id_usuario;
                string correo = datos.correo;
                string rol = datos.rol;
                string tipoempleado = datos.tipoempleado;
                string matricula = datos.matricula;
                string password = datos.password;
                bool puede_autorizar = datos.puede_autorizar;
                string nombre = datos.Nombre;
                string foto = datos.foto;

                if (id_usuario <= 0 || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(tipoempleado))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var data = new Dictionary<string, object>
                {
                    { "correo", correo },
                    { "rol", rol },
                    { "tipoempleado", tipoempleado },
                    { "matricula", matricula },
                    { "password", password },
                    { "puede_autorizar", puede_autorizar ? 1 : 0 },
                    { "Nombre", nombre },
                    { "foto", foto }
                };
                bool actualizado = db.UpdateRow("USUARIOS", data, $"id_usuario={id_usuario}");
                if (actualizado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Usuario modificado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el usuario o no se modificó");
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
                bool eliminado = db.DeleteRow("USUARIOS", $"id_usuario={id}");
                if (eliminado)
                    return Request.CreateResponse(HttpStatusCode.OK, "Usuario eliminado correctamente");
                else
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No se encontró el usuario");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
