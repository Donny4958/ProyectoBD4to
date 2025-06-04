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
                var dt = db.SelectTable($"select * from USUARIO U inner join TIPO_USUARIO TP on U.id_usuario=TP.id_usuario where login=1 and correo='{Correo}'; ");
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
                            tipo = dt.Rows[0]["Autoriza"].ToString() == "1",
                            nombre = dt.Rows[0]["nom_usuario"].ToString()
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