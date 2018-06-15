/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System;
using System.Web;
using System.Web.Http;

namespace sizingservers.beholder.dnfapi {
    public class WebApiApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
        void Application_BeginRequest(object sender, EventArgs e) {
            var context = HttpContext.Current;
            var response = context.Response;

            // Enable CORS
            response.AddHeader("Access-Control-Allow-Origin", "*");

            // Enable other methods besides GET
            if (context.Request.HttpMethod == "OPTIONS") {
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                response.End();
            }
        }
    }
}
