using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Formatter;

namespace NHail.WebAPI.OData
{
    public class ODataHttpActionResult : IHttpActionResult
    {
        private readonly ODataController _controller;
        private readonly object _content;
        private readonly Type _contentType;
        private readonly Type _formatType;

        public ODataHttpActionResult(ODataController controller, object content, Type contentType)
            : this(controller, content, contentType, contentType)
        {
        }

        public ODataHttpActionResult(ODataController controller, object content, Type contentType, Type formatType)
        {
            _controller = controller;
            _content = content;
            _contentType = contentType;
            _formatType = formatType;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateResponse());
        }

        private HttpResponseMessage CreateResponse()
        {
            var response = _controller.Request.CreateResponse();
            var odataMediaTypeFormatter = GetFormatter(_formatType);
            var formatter = odataMediaTypeFormatter.GetPerRequestFormatterInstance(_contentType, _controller.Request,
                                                                                   null);
            response.Content = new ObjectContent(_formatType, _content, formatter);
            return response;
        }

        private MediaTypeFormatter GetFormatter(Type objType)
        {
            //ToDo Generate error if we can't match on the requested content type
            var conneg = _controller.Configuration.Services.GetContentNegotiator();
            var result = conneg.Negotiate(objType, _controller.Request, _controller.Configuration.Formatters);
            return result.Formatter;
        }
    }
}