using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dispatcher;
using Microsoft.Data.Edm;

namespace NHail.WebAPI.OData.Interfaces
{
    public interface IEdmEntityToClrConverter
    {
        Type AsClrType<TSource>(TSource source, IEdmEntityType edmEntityType);
    }
}