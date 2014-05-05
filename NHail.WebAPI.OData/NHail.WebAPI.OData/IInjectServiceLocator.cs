using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NHail.WebAPI.OData
{
    public interface IInjectServiceLocator
    {
        void SetServiceLocator(IServiceLocator serviceLocator);
    }
}