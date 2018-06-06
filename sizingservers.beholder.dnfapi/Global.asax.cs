/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System.Web.Http;

namespace sizingservers.beholder.dnfapi {
    public class WebApiApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
