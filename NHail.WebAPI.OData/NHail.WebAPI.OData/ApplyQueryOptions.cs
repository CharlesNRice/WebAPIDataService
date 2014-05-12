using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.OData.Query;
using NHail.WebAPI.OData.Interfaces;

namespace NHail.WebAPI.OData
{
    public class ApplyQueryOptions : IApplyQueryOptions
    {
        public IQueryable Apply(IQueryable query, ODataQueryOptions queryOptions, ODataQuerySettings oDataQuerySettings)
        {
            //ToDo this does the default Web API filtering but would like to get more control over it
            return queryOptions.ApplyTo(query, oDataQuerySettings);
        }
    }
}