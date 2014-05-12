using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData.Query;

namespace NHail.WebAPI.OData.Interfaces
{
    public interface IApplyQueryOptions
    {
        IQueryable Apply(IQueryable query, ODataQueryOptions queryOptions, ODataQuerySettings oDataQuerySettings);
    }
}
