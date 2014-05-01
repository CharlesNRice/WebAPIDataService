using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using NHail.WebAPI.OData.Interfaces;

namespace NHail.WebAPI.OData
{
    public class DbContextQueryRoot : IQueryRootProvider
    {
        public IQueryable<TEntity> QueryRoot<TSource, TEntity>(TSource source)
            where TEntity : class
        {
            var dbContext = source as DbContext;
            if (dbContext == null)
            {
                return null;
            }

            return QueryRoot<TEntity>(dbContext);
        }

        private IQueryable<TEntity> QueryRoot<TEntity>(DbContext dbContext)
            where TEntity : class
        {
            return dbContext.Set<TEntity>().AsNoTracking();
        }
    }
}