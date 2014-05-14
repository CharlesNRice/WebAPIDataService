using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;

namespace NHail.WebAPI.OData
{
    public class ODataRouteWrapper : IHttpRoute
    {
        private IHttpRoute _httpRoute;
        public ODataRouteWrapper(IHttpRoute httpRoute)
        {
            _httpRoute = httpRoute;
            DataTokens = httpRoute.DataTokens ?? new Dictionary<string, object>();
        }

        public IDictionary<string, object> Constraints
        {
            get { return _httpRoute.Constraints; }
        }

        public IDictionary<string, object> DataTokens { get; private set; }

        public IDictionary<string, object> Defaults
        {
            get { return _httpRoute.Defaults; }
        }

        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            return _httpRoute.GetRouteData(virtualPathRoot, request);
        }

        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            return _httpRoute.GetVirtualPath(request, values);
        }

        public HttpMessageHandler Handler
        {
            get { return _httpRoute.Handler; }
        }

        public string RouteTemplate
        {
            get { return _httpRoute.RouteTemplate; }
        }
    }
}