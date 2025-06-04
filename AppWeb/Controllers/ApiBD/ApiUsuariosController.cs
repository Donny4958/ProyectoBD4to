using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
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
                    U.id_usuario, 
                    U.correo, 
                    U.nom_usuario, 
                    U.rol, 
                    U.foto,
                    T.tipousuario, 
                    T.login, 
                    T.prestamo_externo
                FROM USUARIO U
                INNER JOIN TIPO_USUARIO T ON U.id_usuario = T.id_usuario
            ");
            var lista = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new
                {
                    id_usuario = row["id_usuario"],
                    correo = row["correo"],
                    nombre = row["nom_usuario"],
                    rol = row["rol"],
                    foto = row["foto"] == DBNull.Value ? null : row["foto"].ToString(),
                    tipousuario = row["tipousuario"],
                    login = Convert.ToBoolean(row["login"]),
                    prestamo_externo = Convert.ToBoolean(row["prestamo_externo"])
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
                    U.id_usuario, 
                    U.correo, 
                    U.nom_usuario, 
                    U.rol, 
                    U.foto,
                    T.tipousuario, 
                    T.login, 
                    T.prestamo_externo
                FROM USUARIO U
                INNER JOIN TIPO_USUARIO T ON U.id_usuario = T.id_usuario
                WHERE U.id_usuario = {id}
            ");
            if (dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                var usuario = new
                {
                    id_usuario = row["id_usuario"],
                    correo = row["correo"],
                    nombre = row["nom_usuario"],
                    rol = row["rol"],
                    foto = row["foto"] == DBNull.Value ? null : row["foto"].ToString(),
                    tipousuario = row["tipousuario"],
                    login = Convert.ToBoolean(row["login"]),
                    prestamo_externo = Convert.ToBoolean(row["prestamo_externo"])
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
                string nombre = datos.nombre;
                string rol = datos.rol;
                string password = datos.password;
                string foto = datos.foto;

                string tipousuario = datos.tipousuario != null ? (string)datos.tipousuario : null;
                bool login = datos.login != null ? (bool)datos.login : false;

                // El correo es obligatorio solo si tendrá login o es externo
                bool correoObligatorio = (login || (tipousuario != null && tipousuario == "externo"));
                if (correoObligatorio && string.IsNullOrEmpty(correo))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "El correo es obligatorio para usuarios con login o externos.");

                var db = new DataBaseHelper();

                // Si el correo es obligatorio, verifica que no exista
                if (correoObligatorio && db.Exists($"SELECT 1 FROM USUARIO WHERE correo='{correo}'"))
                    return Request.CreateResponse(HttpStatusCode.Conflict, "El correo ya existe");

                // Inserta en USUARIO
                var dataUsuario = new Dictionary<string, object>
        {
            { "correo", correo },
            { "nom_usuario", nombre },
            { "rol", rol },
            { "password", Hash(password) },
            { "foto", foto }
        };
                int id_usuario = db.InsertRowReturnId("USUARIO", dataUsuario);

                // Si se le da login o alguna opción especial, insertar en TIPO_USUARIO
                if (tipousuario != null && datos.login != null && datos.prestamo_externo != null)
                {
                    bool prestamo_externo = datos.prestamo_externo;
                    var dataTipo = new Dictionary<string, object>
            {
                { "id_usuario", id_usuario },
                { "tipousuario", tipousuario },
                { "login", login },
                { "prestamo_externo", prestamo_externo }
            };
                    db.InsertRow("TIPO_USUARIO", dataTipo);
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Usuario creado correctamente");
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
                string nombre = datos.nombre;
                string rol = datos.rol;
                string password = datos.password;
                string foto = datos.foto;

                var db = new DataBaseHelper();

                // Actualiza USUARIO
                var dataUsuario = new Dictionary<string, object>
        {
            { "correo", correo },
            { "nom_usuario", nombre },
            { "rol", rol },
            { "password", password },
            { "foto", foto }
        };
                db.UpdateRow("USUARIO", dataUsuario, $"id_usuario={id_usuario}");

                // Verifica si hay datos de tipo_usuario
                bool tieneTipo = datos.tipousuario != null && datos.login != null && datos.prestamo_externo != null;
                bool existeTipo = db.Exists($"SELECT 1 FROM TIPO_USUARIO WHERE id_usuario={id_usuario}");

                if (tieneTipo)
                {
                    string tipousuario = datos.tipousuario;
                    bool login = datos.login;
                    bool prestamo_externo = datos.prestamo_externo;

                    var dataTipo = new Dictionary<string, object>
            {
                { "tipousuario", tipousuario },
                { "login", login },
                { "prestamo_externo", prestamo_externo }
            };

                    if (existeTipo)
                        db.UpdateRow("TIPO_USUARIO", dataTipo, $"id_usuario={id_usuario}");
                    else
                    {
                        dataTipo.Add("id_usuario", id_usuario);
                        db.InsertRow("TIPO_USUARIO", dataTipo);
                    }
                }
                else if (existeTipo)
                {
                    // Si ya no debe tener acceso, eliminar de TIPO_USUARIO
                    db.DeleteRow("TIPO_USUARIO", $"id_usuario={id_usuario}");
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Usuario modificado correctamente");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public static string Hash(string textoPlano)
        {
            byte[] salt;
            byte[] buffer;
            if (textoPlano == null)
            {
                throw new ArgumentException(nameof(textoPlano));
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(textoPlano, 16, 1000))
            {
                salt = bytes.Salt;
                buffer = bytes.GetBytes(32);
            }
            byte[] dst = new byte[49];
            Buffer.BlockCopy(salt, 0, dst, 1, 16);
            Buffer.BlockCopy(buffer, 0, dst, 17, 32);
            return Convert.ToBase64String(dst);
        }

        [HttpDelete]
        [Route("Eliminar")]
        public HttpResponseMessage Eliminar(int id)
        {
            try
            {
                var db = new DataBaseHelper();
                // Elimina primero de TIPO_USUARIO por la FK
                db.DeleteRow("TIPO_USUARIO", $"id_usuario={id}");
                bool eliminado = db.DeleteRow("USUARIO", $"id_usuario={id}");
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
