using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Dispatcher;
using Microsoft.Data.Edm;
using NHail.WebAPI.OData.Interfaces;

namespace NHail.WebAPI.OData
{
    public class DbContextEdmEntityToClrConverter : IEdmEntityToClrConverter, IInjectServiceLocator
    {
        private readonly static ConcurrentDictionary<IEdmEntityType, Type> _cachedConversions =
            new ConcurrentDictionary<IEdmEntityType, Type>();

        private IServiceLocator _serviceLocator;

        public Type AsClrType<TSource>(TSource source, IEdmEntityType edmEntityType)
        {
            var dbContext = source as DbContext;
            if (dbContext == null)
            {
                return null;
            }

            // Can't use GetOrAdd want to trap for null
            Type result;
            if (!_cachedConversions.TryGetValue(edmEntityType, out result))
            {
                result = ConvertIEdmEntityTypeToClr(edmEntityType, dbContext);
                if (result != null)
                {
                    _cachedConversions.TryAdd(edmEntityType, result);
                }
            }

            return result;
        }

        private Type ConvertIEdmEntityTypeToClr(IEdmEntityType edmEntityType, DbContext context)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var oSpace = metadata.GetItemCollection(DataSpace.OSpace);
            var typeName = oSpace.GetItems<EntityType>().Select(e => e.FullName).FirstOrDefault(name =>
            {
                var fullname = name + ":" + edmEntityType.FullName();
                MappingBase map;
                return metadata.TryGetItem(fullname, DataSpace.OCSpace, out map);
            });

            if (typeName != null)
            {
                return Type.GetType(typeName, null, GetTypeFromAssembly, false, false);
            }
            return null;
        }

        private Type GetTypeFromAssembly(Assembly assembly, string nameOfType, bool ignoreCase)
        {
            if (assembly == null)
            {
                var resolver = _serviceLocator.ServiceLocator<IAssembliesResolver>();
                return resolver.GetAssemblies()
                               .Select(a => a.GetType(nameOfType, false, ignoreCase))
                               .FirstOrDefault(t => t != null);
            }
            return assembly.GetType(nameOfType, false, ignoreCase);
        }

        public void SetServiceLocator(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }
    }
}