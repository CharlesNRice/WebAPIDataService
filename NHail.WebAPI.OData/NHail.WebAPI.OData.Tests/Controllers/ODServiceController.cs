using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NHail.WebAPI.OData.Controllers;
using NHail.WebAPI.OData.Tests.Contexts;

namespace NHail.WebAPI.OData.Tests.Controllers
{
    public class ODServiceController : ODataServicesController<CodeFirstContext>
    {
        public IHttpActionResult Get()
        {
            return ProcessRequest();
        }

        public override string ODataRoute
        {
            get { return "DefaultOData"; }
        }
    }
}
