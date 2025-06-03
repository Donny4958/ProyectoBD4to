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
    [RoutePrefix("api/Main")]
    public class TalleresController : ApiController
    {
        [HttpGet]
        [Route("probarConexion")]
        public HttpResponseMessage ObtenerTalleres()
        {
            var db = new DataBaseHelper();
            DataTable json = db.SelectTable("select * from Negros");
            return Request.CreateResponse(HttpStatusCode.OK, json);
        }
        public bool VerificarRegex(string palabra,int Tipo)
        {
            try
            {
                Regex reg= null;
                switch (Tipo)
                {
                    case 0://NumeroDecimal
                        reg= new Regex(@"^\d+(.\d+)?$");
                        break;
                    case 1://Numero Entero
                        reg = new Regex(@"^\d+$");
                        break;
                    case 2://Palabra Con o sin espacios
                        reg = new Regex(@"^(\w+(\s)?)+$");
                        break;
                    case 3://Palabra sin espacios
                        reg = new Regex(@"^\w+$");
                        break;
                    case 4://Correo
                        reg = new Regex(@"^[^@]+@[^@]+\.[a-zA-Z]{2,}$");
                        break;
                    case 5://Lenguaje
                        reg = new Regex(@"^\w{1,3}$");
                        break;
                    default:
                        reg = new Regex(@"asd");
                        break;
                }
                var respuesta=reg.Match(palabra);


                return respuesta.Success;
            }
            catch
            {
                return false;
            }
        }
        [HttpPost]
        [Route("login")]
        public async Task<HttpResponseMessage> Login([FromBody] dynamic datos)
        {
            try
            {
                string Correo = datos.Correo;
                string password = datos.Password;

                if (string.IsNullOrEmpty(Correo) || string.IsNullOrEmpty(password))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                var db = new DataBaseHelper();
                var dt = db.SelectTable($"select * from USUARIOS where correo='{Correo}'");
                var Confirmar = Hash(password);
                if (dt.Rows.Count == 1)
                {
                    string hash = dt.Rows[0]["password"].ToString();
                    if (VerifyHashedPassword(hash, password))
                    {
                        // Devuelve datos para sessionStorage
                        var result = new
                        {
                            logueado = true,
                            ID = dt.Rows[0]["id_usuario"].ToString(),
                            tipo = dt.Rows[0]["puede_autorizar"].ToString() == "1",
                            nombre = dt.Rows[0]["Nombre"].ToString()
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, result);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, "Contraseña incorrecta");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, "No existe usuario con esos datos");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        [HttpPost]
        [Route("ObtenerUsuarioInfo")]
        public HttpResponseMessage ObtenerUsuarioInfo([FromBody] object var)
        {
            // Deserializa el objeto dinámicamente
            dynamic datos = var;
            string tipo = datos.tipo;
            string id = datos.id;

            var db = new DataBaseHelper();
            if (tipo == "alumno")
            {
                var dt = db.SelectTable($"SELECT nombre, matricula, foto FROM Alumno WHERE matricula='{id}'");
                if (dt.Rows.Count == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        nombre = dt.Rows[0]["nombre"].ToString(),
                        matricula = dt.Rows[0]["matricula"].ToString(),
                        foto = dt.Rows[0]["foto"].ToString()
                    });
                }
            }
            else if (tipo == "empleado")
            {
                var dt = db.SelectTable($"SELECT nombre, num_empleado, tipo_empleado, foto FROM empleado WHERE id_empleado='{id}'");
                if (dt.Rows.Count == 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        nombre = dt.Rows[0]["nombre"].ToString(),
                        num_empleado = dt.Rows[0]["num_empleado"].ToString(),
                        tipo_empleado = dt.Rows[0]["tipo_empleado"].ToString(),
                        foto = dt.Rows[0]["foto"].ToString()
                    });
                }
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, "Usuario no encontrado");
        }

        [HttpPost]
        [Route("RegistrarPersona")]
        public async Task<HttpResponseMessage> RegistrarPersona([FromBody] dynamic datos)
        {
            try
            {
                string tipo = datos.TipoUsuario;
                string Pass= datos.Password;
                if (tipo == "alumno")
                {
                    string nombre = datos.Nombre;
                    string matricula = datos.Matricula;
                    string foto = datos.Foto;

                    // Validaciones básicas
                    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(matricula) || string.IsNullOrEmpty(foto))
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                    var db = new DataBaseHelper();
                    // Verifica si ya existe la matrícula
                    bool existe = db.Exists($"SELECT 1 FROM Alumno WHERE matricula='{matricula}'");
                    if (existe)
                        return Request.CreateResponse(HttpStatusCode.Conflict, "La matrícula ya existe");
                    string Contra = Hash(Pass);
                    var data = new Dictionary<string, object>
            {
                { "nombre", nombre },
                { "matricula", matricula },
                { "foto", foto },
                { "Pass", Contra }
            };
                    bool insertado = db.InsertRow("Alumno", data);
                    if (insertado)
                        return Request.CreateResponse(HttpStatusCode.OK, "Alumno registrado");
                    else
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al registrar alumno");
                }
                else if (tipo == "empleado")
                {
                    string nombre = datos.Nombre;
                    string numEmpleado = datos.NumEmpleado;
                    string tipoEmpleado = datos.TipoEmpleado;
                    string foto = datos.Foto;

                    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(numEmpleado) || string.IsNullOrEmpty(tipoEmpleado) || string.IsNullOrEmpty(foto))
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Datos incompletos");

                    var db = new DataBaseHelper();
                    // Verifica si ya existe el número de empleado
                    bool existe = db.Exists($"SELECT 1 FROM empleado WHERE num_empleado='{numEmpleado}'");
                    if (existe)
                        return Request.CreateResponse(HttpStatusCode.Conflict, "El número de empleado ya existe");
                    string Contra = Hash(Pass);
                    var data = new Dictionary<string, object>
            {
                { "nombre", nombre },
                { "num_empleado", numEmpleado },
                { "tipo_empleado", tipoEmpleado },
                { "foto", foto },
                { "Pass", Contra }
            };
                    bool insertado = db.InsertRow("empleado", data);
                    if (insertado)
                        return Request.CreateResponse(HttpStatusCode.OK, "Empleado registrado");
                    else
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error al registrar empleado");
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Tipo de usuario no válido");
                }
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


        public static bool VerifyHashedPassword(string hashedPassword, string Password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (Password == null)
            {
                throw new ArgumentNullException(nameof(Password));
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if (src.Length != 49 || src[0] != 0)
            {
                return false;
            }
            byte[] dst = new byte[16];
            Buffer.BlockCopy(src, 1, dst, 0, 16);
            byte[] buffer3 = new byte[32];
            Buffer.BlockCopy(src, 17, buffer3, 0, 32);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(Password, dst, 1000))
            {
                buffer4 = bytes.GetBytes(32);
            }
            return buffer3.SequenceEqual(buffer4);
        }
    }

}
public class Usuario
{
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string Contrasena{ get; set; }
    public string ContrasenaHash { get; set; }
    public string Moneda { get; set; }
    public string Lenguaje { get; set; }
    
}
public class Respuesta
{
    public string Mensaje { get; set; }
    public int Codigo { get; set; }
}