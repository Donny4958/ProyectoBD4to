using System.Web.Http;

namespace AppWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilita [Route] y [RoutePrefix]
            config.MapHttpAttributeRoutes();

            // Ruta de fallback
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
