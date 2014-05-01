using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using NHail.WebAPI.OData.Attributes;
using NHail.WebAPI.OData.Interfaces;

namespace NHail.WebAPI.OData
{
    //ToDo make sure this class works - haven't tested or created unit test for it.
    public abstract class QueryInterceptor : IQueryInterceptor
    {
        private readonly MethodInfo[] _methodInfos;

        protected QueryInterceptor()
        {
            _methodInfos = GetType().GetMethods()
                                    .Where(
                                        m =>
                                        m.GetCustomAttributesData()
                                         .Any(a => a.AttributeType == typeof(QueryInterceptorAttribute)) &&
                                        !m.GetParameters().Any() &&
                                        m.IsGenericMethod == false).ToArray();
        }

        public Expression<Func<TEntity, bool>> Intercept<TEntity>()
        {
            var inteceptor = _methodInfos.FirstOrDefault(m => m.ReturnType == typeof(Expression<Func<TEntity, bool>>));
            if (inteceptor != null)
            {
                return inteceptor.Invoke(this, new object[0]) as Expression<Func<TEntity, bool>>;
            }
            return null;
        }
    }
}