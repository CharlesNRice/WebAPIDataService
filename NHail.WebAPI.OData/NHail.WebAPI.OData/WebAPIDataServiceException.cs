using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NHail.WebAPI.OData
{
    public class WebAPIDataServiceException : InvalidOperationException
    {
        public WebAPIDataServiceException()
        {
        }

        public WebAPIDataServiceException(string message) : base(message)
        {
        }

        public WebAPIDataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}