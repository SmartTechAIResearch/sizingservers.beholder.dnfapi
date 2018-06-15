/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using sizingservers.beholder.dnfapi.Controllers;
using System.Net.Http.Headers;
using System.Web.Http;

namespace sizingservers.beholder.dnfapi {
    public static class WebApiConfig {
        public static void Register(HttpConfiguration config) {
            // Web API configuration and services
#if DEBUG
            AuthorizationHelper.Authorization = false;
#else
            AuthorizationHelper.Authorization = AppSettings.GetValue<bool>("Authorization");
#endif

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //Format data as json instead of xml.
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}
