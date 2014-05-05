using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Batch;
using System.Web.Http.Routing;

namespace NHail.WebAPI.OData
{
    public static class RouteCollectionExtensions
    {
        public static IHttpRoute MapODataServiceBatchRoute(this HttpRouteCollection routes, string routeName,
                                                           string routeTemplate, HttpBatchHandler batchHandler)
        {
            return routes.MapODataServiceBatchRoute(routeName, routeTemplate, null, null, batchHandler);
        }

        public static IHttpRoute MapODataServiceBatchRoute(this HttpRouteCollection routes, string name,
                                                           string routeTemplate)
        {
            return routes.MapODataServiceBatchRoute(name, routeTemplate, null, null, null);
        }

        public static IHttpRoute MapODataServiceBatchRoute(this HttpRouteCollection routes, string name,
                                                           string routeTemplate,
                                                           object defaults)
        {
            return routes.MapODataServiceBatchRoute(name, routeTemplate, defaults, null, null);
        }

        public static IHttpRoute MapODataServiceBatchRoute(this HttpRouteCollection routes, string name,
                                                           string routeTemplate,
                                                           object defaults, object constraints)
        {
            return routes.MapODataServiceBatchRoute(name, routeTemplate, defaults, constraints, null);
        }

        public static IHttpRoute MapODataServiceBatchRoute(this HttpRouteCollection routes, string name,
                                                           string routeTemplate,
                                                           object defaults, object constraints,
                                                           HttpMessageHandler handler)
        {
            var route = routes.MapHttpRoute(name, routeTemplate, defaults, constraints, handler);
            route.DataTokens.Add("ID", name);
            return route;
        }
    }
}