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
            DataTable json = db.SelectTable("select * from empleado");
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
        public async Task<HttpResponseMessage> Login([FromBody] LoginDto datos)
        {
            try {
                if (datos.Correo==null || !VerificarRegex(datos.Correo,5))
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Correo mal escrito");
                }
                if (datos.Password ==null || datos.Password.Length<=5)
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Contraseña incorrecta");
                }

            string query = $"select ContrasenaHash from usuarios where correo='{datos.Correo}'";
            var db = new DataBaseHelper();
            var Dt = db.SelectTable(query);
            if (Dt.Rows.Count== 1)
            {
                string Hash = Dt.Rows[0]["ContrasenaHash"].ToString();
                bool esValida = VerifyHashedPassword(Hash, datos.Password);
                if (esValida)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Pasele a lo barrido");
                }
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Credenciales incorrectas");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Errores con usuarios");
            }
            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ex.Message);
            }
        }
        
        
        [HttpPost]
        [Route("Registrar")]
        public async Task<HttpResponseMessage> Registrar([FromBody] Usuario datos)
        {
            try { 
                if (datos.Correo == null || !VerificarRegex(datos.Correo, 4))
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Correo mal escrito");
                }
                if (datos.Nombre == null || !VerificarRegex(datos.Nombre, 3))
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Nombre mal escrito");
                }
                if (datos.Contrasena == null || datos.Contrasena.Length <= 5)
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Contraseña incorrecta");
                }
                if (datos.Moneda == null || !VerificarRegex(datos.Moneda, 1))
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Moneda Incorrecta");
                }
                if (datos.Lenguaje == null || !VerificarRegex(datos.Lenguaje, 5))
                {
                    return Request.CreateResponse(HttpStatusCode.Ambiguous, "Lenguaje Incorrecta");
                }
                //subir a base de datos
                var db = new DataBaseHelper();
                string Tabla="Usuarios";
                bool existe = db.Exists($"select top 1 Nombre from {Tabla} where correo='{datos.Correo}'");
                Respuesta regresar= new Respuesta();
                if (existe)
                {
                    regresar = new Respuesta
                    {
                        Mensaje = "El correo ya existe",
                        Codigo = 409
                    };
                    return Request.CreateResponse(HttpStatusCode.Conflict,regresar);
                }
                var hash = Hash(datos.Contrasena);
                datos.ContrasenaHash = hash;
                Dictionary<string, object> data= new Dictionary<string, object>
                {
                    { "Nombre", datos.Nombre },
                    { "Correo", datos.Correo },
                    { "ContrasenaHash", datos.ContrasenaHash },
                    { "MonedaId", datos.Moneda },
                    { "Lenguaje", datos.Lenguaje }
                };
            var Respuesta= db.InsertRow(Tabla, data);

                if (Respuesta)
                {
                    regresar = new Respuesta
                    {
                        Mensaje = "Cuenta Creada Exitosamente",
                        Codigo = 200
                    };
                    return Request.CreateResponse(HttpStatusCode.OK,regresar);
                }
                else
                {
                    regresar = new Respuesta
                    {
                        Mensaje = "Error al registrar el usuario",
                        Codigo = 500
                    };
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, regresar);
                }
                
            }
            catch (Exception ex)
            {
                Respuesta regresar = new Respuesta
                    {
                        Mensaje = ex.Message.ToString(),
                        Codigo = 500
                };
                return Request.CreateResponse(HttpStatusCode.InternalServerError, regresar);
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
public class LoginDto
{
    public string Correo { get; set; }
    public string Password { get; set; }
}
public class RegistrarDto
{
    public string Correo { get; set; }
    public string Password { get; set; }
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