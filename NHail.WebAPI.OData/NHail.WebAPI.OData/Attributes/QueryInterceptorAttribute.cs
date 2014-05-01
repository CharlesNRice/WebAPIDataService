using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NHail.WebAPI.OData.Attributes
{
    [AttributeUsageAttribute(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class QueryInterceptorAttribute : Attribute
    {
    }
}