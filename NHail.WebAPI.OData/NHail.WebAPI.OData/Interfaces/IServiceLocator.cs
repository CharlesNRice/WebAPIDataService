﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NHail.WebAPI.OData.Interfaces
{
    public interface IServiceLocator
    {
        T ServiceLocator<T>() where T : class;
    }
}