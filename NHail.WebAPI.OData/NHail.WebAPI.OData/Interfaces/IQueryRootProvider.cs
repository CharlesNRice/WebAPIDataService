using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHail.WebAPI.OData.Interfaces
{
    public interface IQueryRootProvider
    {
        IQueryable<TEntity> QueryRoot<TSource, TEntity>(TSource source)
            where TEntity : class;
    }
}
