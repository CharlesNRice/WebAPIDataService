using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData.Formatter;
using System.Web.Http.Routing;
using Owin;

namespace NHail.WebAPI.OData.Tests
{
    public class Startup
    {
        static Startup()
        {
            
        }

        private static HttpServer makeHttpServer()
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultOData",
                routeTemplate: "api/odata/{*wildcard}",
                defaults: new
                {
                    controller = "ODService",
                    action = "Get",
                    wildcard = RouteParameter.Optional
                });
            config.Routes.IgnoreRoute("ODataIgnore", "api/ODService/{*wildcard}");
            config.Formatters.Clear();
            config.Formatters.AddRange(ODataMediaTypeFormatters.Create());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            return new HttpServer(config);
        }

        private static readonly Lazy<HttpServer> _httpServer = new Lazy<HttpServer>(makeHttpServer);

        public static HttpServer HttpServer
        {
            get { return _httpServer.Value; }
        }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {

            appBuilder.UseWebApi(HttpServer);
        }
    } 
}
