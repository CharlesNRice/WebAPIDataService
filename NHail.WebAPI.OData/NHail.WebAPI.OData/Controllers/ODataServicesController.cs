using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Http.OData;
using System.Web.UI.WebControls;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;
using Microsoft.Data.OData.Query;
using NHail.WebAPI.OData.Interfaces;

namespace NHail.WebAPI.OData.Controllers
{
    public abstract class ODataServicesController<TSource> : ODataMetadataController
    {
        public abstract string ODataRoute { get; }
        
        static ODataServicesController()
        {

            _firstOrDefault =
                typeof (Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                  .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Count() == 1)
                                  .MethodHandle;
                                

            _processRequest =
                typeof(ODataServicesController<TSource>).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                                         .First(
                                                             m =>
                                                             m.Name == "ProcessRequest" && m.IsGenericMethodDefinition)
                                                         .MethodHandle;

            // Load up default implementations
            ServiceLoctorConfiguration<IEdmModelFactory>(() => new DbContextEdmModel());
            ServiceLoctorConfiguration<IEdmEntityToClrConverter>(() => new DbContextEdmEntityToClrConverter());
            ServiceLoctorConfiguration<IQueryRootProvider>(() => new DbContextQueryRoot());
            
            // Default for the odata query
            oDataQuerySettings = new ODataQuerySettings();
        }

        protected ODataServicesController()
        {
            SetServiceLocator();
            // Setup data source to get resolved first time needed.
            _currentDataSource = new Lazy<TSource>(CreateDataSource, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected static ODataQuerySettings oDataQuerySettings { get; private set; }

        private static readonly RuntimeMethodHandle _processRequest;
        private static readonly RuntimeMethodHandle _firstOrDefault;


        private static ConcurrentDictionary<KeyValuePair<IEdmEntityType, Type>, Delegate> _typeToGeneric =
            new ConcurrentDictionary<KeyValuePair<IEdmEntityType, Type>, Delegate>();

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            Request.SetEdmModel(BuildEdmModel());
        }

        #region DataSource

        private readonly Lazy<TSource> _currentDataSource;

        protected TSource CurrentDataSource
        {
            get { return _currentDataSource.Value; }
        }

        protected virtual TSource CreateDataSource()
        {
            return DependencyResolver.Current.GetService<TSource>();
        }

        #endregion

        #region ServiceLocator

        private static readonly IDictionary<Type, Lazy<object>> _serviceLocatorDefaults = new Dictionary<Type, Lazy<object>>();

        protected static void ServiceLoctorConfiguration<TInterface>(Func<TInterface> valueFactory)
            where TInterface : class
        {
            _serviceLocatorDefaults[typeof(TInterface)] = new Lazy<object>(valueFactory);
        }


        private Func<Type, object> _serviceLocator;

        private T ServiceLocator<T>() where T : class
        {
            return _serviceLocator(typeof(T)) as T;
        }

        private void SetServiceLocator()
        {
            // check if datasource or controller implements IServiceProvider;
            var dataSourceResolver = (typeof(IServiceProvider).IsAssignableFrom(typeof(TSource)));
            var controllerResolver = (typeof(IServiceProvider).IsAssignableFrom(GetType()));

            if (dataSourceResolver && controllerResolver)
            {
                _serviceLocator = type =>
                {
                    var result = ((IServiceProvider)this).GetService(type);
                    if (result == null)
                    {
                        result = ((IServiceProvider)CurrentDataSource).GetService(type);
                        if (result == null)
                        {
                            result = DependencyResolver.Current.GetService(type);
                        }
                        if (result == null && _serviceLocatorDefaults.ContainsKey(type))
                        {
                            result = _serviceLocatorDefaults[type].Value;
                        }
                    }
                    return result;
                };
            }
            else if (dataSourceResolver)
            {
                _serviceLocator = type =>
                {
                    var result = ((IServiceProvider)CurrentDataSource).GetService(type);
                    if (result == null)
                    {
                        result = DependencyResolver.Current.GetService(type);
                    }
                    if (result == null && _serviceLocatorDefaults.ContainsKey(type))
                    {
                        result = _serviceLocatorDefaults[type].Value;
                    }
                    return result;
                };
            }
            else if (controllerResolver)
            {
                _serviceLocator = type =>
                {
                    var result = ((IServiceProvider)this).GetService(type);
                    if (result == null)
                    {
                        result = DependencyResolver.Current.GetService(type);
                    }
                    if (result == null && _serviceLocatorDefaults.ContainsKey(type))
                    {
                       
                        result = _serviceLocatorDefaults[type].Value;
                    }
                    return result;
                };
            }
            else
            {
                _serviceLocator = type =>
                {
                    var result = DependencyResolver.Current.GetService(type);
                    if (result == null && _serviceLocatorDefaults.ContainsKey(type))
                    {
                        result = _serviceLocatorDefaults[type].Value;
                    }
                    return result;
                };
            }
        }

        #endregion

        private IEdmEntityContainer Container
        {
            get
            {
                var metadata = GetMetadata();
                return metadata.EntityContainers().Single();
            }
        }

        private IEdmModel BuildEdmModel()
        {
            // Try and get an IEdmModel
            var edmModel = ServiceLocator<IEdmModel>();
            if (edmModel == null)
            {
                // try and get the factory
                var edmModelFactory = ServiceLocator<IEdmModelFactory>();
                if (edmModelFactory != null)
                {
                    edmModel = edmModelFactory.EdmModel(CurrentDataSource);
                }
            }

            if (edmModel == null)
            {
                throw new WebAPIDataServiceException("Can not find IEdmModel");
            }
            return edmModel;
        }

        [System.Web.Http.NonAction]
        protected virtual string GetODataPath()
        {
            var routedata = Request.GetRouteData();
            var uriTemplate = new UriTemplate(routedata.Route.RouteTemplate);
            var baseUri = new Uri(Request.RequestUri.Scheme + "://" +
                                  Request.RequestUri.Authority +
                                  Request.GetRequestContext().VirtualPathRoot.TrimEnd('/') + "/");
            var match = uriTemplate.Match(baseUri, Request.RequestUri);
            var path = "/" + String.Join("/", match.WildcardPathSegments);
            return path;
        }

        private Type GetIEdmTypeToCLRType(IEdmEntityType edmEntityType)
        {
            var asClrType = ServiceLocator<IEdmEntityToClrConverter>();
            var entityType = asClrType.AsClrType(CurrentDataSource, edmEntityType);

            return entityType;
        }

        private IEdmEntityType GetEdmEntityType(IEdmType edmType)
        {
            var edmEntityType = edmType as IEdmEntityType;
            if (edmEntityType == null)
            {
                var collectionType = edmType as IEdmCollectionType;
                if (collectionType != null)
                {
                    edmEntityType = collectionType.ElementType.AsEntity().EntityDefinition();
                }
            }

            return edmEntityType;
        }

        protected IHttpActionResult GenerateMetadataResponse()
        {
            return new ODataHttpActionResult(this, GetMetadata(), typeof(IEdmModel));
        }

        protected IQueryable QueryEntitySet(IEdmCollectionType edmCollectionType)
        {
            var edmType = GetEdmEntityType(edmCollectionType);
            if (edmType != null)
            {
                var entityType = GetIEdmTypeToCLRType(edmType);
                // Switch from system Type to generic type
                var process = SwitchToGenericMethod(edmType, entityType);
                return process(this, edmType);
            }
            throw new WebAPIDataServiceException("Entity Set not found!");
        }

        private Func<ODataServicesController<TSource>, IEdmEntityType, IQueryable> SwitchToGenericMethod(
            IEdmEntityType edmEntityType, Type clrType)
        {
            return
                (Func<ODataServicesController<TSource>, IEdmEntityType, IQueryable>)
                _typeToGeneric.GetOrAdd(new KeyValuePair<IEdmEntityType, Type>(edmEntityType, clrType),
                                        pair =>
                                        {
                                            var method = ((MethodInfo)
                                                          MethodBase.GetMethodFromHandle(
                                                              _processRequest, GetType().TypeHandle))
                                                .MakeGenericMethod(pair.Value);
                                            var controller =
                                                Expression.Parameter(
                                                    typeof(ODataServicesController<TSource>),
                                                    "oDServiceController");
                                            var edmentityType =
                                                Expression.Parameter(typeof(IEdmEntityType),
                                                                     "edmEntityType");

                                            var expr = Expression.Call(controller, method, edmentityType);
                                            return
                                                Expression.Lambda<Func<ODataServicesController<TSource>,
                                                    IEdmEntityType, IQueryable>>(expr, controller,
                                                                                 edmentityType).Compile();
                                        });
        }

        private IQueryable ProcessRequest<TEntity>(IEdmEntityType edmEntityType)
            where TEntity : class
        {

            var queryable = GetQueryRoot<TEntity>();

            // Check if there are any interceptor
            var interception = GetInterceptor<TEntity>();
            if (interception != null)
            {
                queryable = queryable.Where(interception);
            }

            return queryable;
        }

        private IQueryable<TEntity> GetQueryRoot<TEntity>()
            where TEntity : class
        {
            var queryRoot = ServiceLocator<IQueryRootProvider>();
            if (queryRoot != null)
            {
                return queryRoot.QueryRoot<TSource, TEntity>(CurrentDataSource);
            }
            return null;
        }

        private Expression<Func<TEntity, bool>> GetInterceptor<TEntity>()
        {
            var interception = ServiceLocator<IQueryInterceptor>();
            if (interception != null)
            {
                return interception.Intercept<TEntity>();
            }
            return null;
        }

        private HttpResponseMessage QueryEntity(IEdmEntityType edmType)
        {
            var entityType = GetIEdmTypeToCLRType(edmType);
            // Switch from system Type to generic type
            var process = (Func<IEdmEntityType, IQueryable>)
                          _typeToGeneric.GetOrAdd(new KeyValuePair<IEdmEntityType, Type>(edmType, entityType),
                                                  pair =>
                                                  {
                                                      var method = ((MethodInfo)
                                                                    MethodBase.GetMethodFromHandle(
                                                                        _processRequest, GetType().TypeHandle))
                                                          .MakeGenericMethod(pair.Value);
                                                      return
                                                          Delegate.CreateDelegate(
                                                              typeof(Func<IEdmEntityType, IQueryable>), this,
                                                              method);
                                                  });

            var query = process(edmType);
            var response = Request.CreateResponse();
            var odataMediaTypeFormatter = GetFormatter(query.ElementType);
            var formatter = odataMediaTypeFormatter.GetPerRequestFormatterInstance(query.ElementType, Request, null);

            response.Content = new ObjectContent(typeof(IEdmModel), GetMetadata(), formatter);
            return response;
        }

        private IHttpActionResult GenerateServiceDocument()
        {
            return new ODataHttpActionResult(this, GetServiceDocument(), typeof(ODataWorkspace));
        }

        private void MapIEdmEntitySetsToCLR()
        {
            var metaData = GetMetadata();
            var entitySets = Container.EntitySets();
            foreach (var entitySet in entitySets)
            {
                var entityType = GetIEdmTypeToCLRType(entitySet.ElementType);
                metaData.SetAnnotationValue(entitySet.ElementType, new ClrTypeAnnotation(entityType));
            }
        }

        protected virtual IHttpActionResult ProcessRequest()
        {
            var pathHandler = Request.GetODataPathHandler();
            var metadata = GetMetadata();
            var path = pathHandler.Parse(metadata, GetODataPath());
            if (path != null)
            {
                Request.SetODataPath(path);
                Request.SetODataRouteName(ODataRoute);
                if (path.Segments.Count == 0)
                {
                    // Requested service document
                    return GenerateServiceDocument();
                }

                if (path.Segments.Any(s => s.SegmentKind == ODataSegmentKinds.Metadata))
                {
                    // Requested metadata
                    return GenerateMetadataResponse();
                }

                // not great that these are strings
                if (path.Segments[0].SegmentKind == "entityset")
                {
                    MapIEdmEntitySetsToCLR();
                    return Buildup(path);
                }
            }
            throw new WebAPIDataServiceException("Can not process OData path", new ODataUnrecognizedPathException());
        }

        private IQueryable ProjectionInto(IQueryable queryable, string property)
        {
            // need to build the select clause
            var type = queryable.Expression.Type.GetQueryElementType();

            // parameter of the expression
            var navProp = type.GetProperty(property);
            var source = Expression.Parameter(type);
            var projection = Expression.Property(source, navProp);
            var proptype = navProp.PropertyType;
            var innerType = proptype.GetQueryElementType();

            Expression func;
            Expression select;
            if (innerType != null && innerType != typeof(string))
            {
                // select many if collection
                func =
                    Expression.Lambda(
                        typeof(Func<,>).MakeGenericType(type, typeof(IEnumerable<>).MakeGenericType(innerType)),
                        projection, source);
                select = Expression.Call(typeof(Queryable), "SelectMany", new[] { type, innerType }, queryable.Expression,
                                         func);

            }
            else
            {
                // select  if single
                func = Expression.Lambda(typeof(Func<,>).MakeGenericType(type, proptype), projection, source);
                select = Expression.Call(typeof(Queryable), "Select", new[] { type, proptype }, queryable.Expression,
                                         func);
            }

            return queryable.Provider.CreateQuery(select);

        }

        private IQueryable ProjectProperty(IQueryable queryable, PropertyAccessPathSegment propertyPath)
        {
            if (propertyPath == null)
            {
                throw new WebAPIDataServiceException("Missing property path");
            }

            return ProjectionInto(queryable, propertyPath.PropertyName);
        }

        private IQueryable DrillIntoNavigationProperty(IQueryable queryable, NavigationPathSegment navigationPath)
        {
            // make sure we found the key and entity
            if (navigationPath == null)
            {
                throw new WebAPIDataServiceException("Missing navigation key");
            }

            return ProjectionInto(queryable, navigationPath.NavigationPropertyName);
        }

        private IQueryable FilterOnKeys(IQueryable queryable, IEdmCollectionType edmCollectionType,
                                        KeyValuePathSegment segmentKey)
        {
            // need to filter down the entity 
            var entityType = GetEdmEntityType(edmCollectionType);

            // make sure we found the key and entity
            if (entityType == null || segmentKey == null)
            {
                throw new WebAPIDataServiceException("Keys not found", new KeyNotFoundException());
            }

            var entityKeys = entityType.DeclaredKey.Select(dk => dk.Name).ToArray();

            // key can be pass in either , or name value pair
            var segmentKeys = segmentKey.Value.Split(',');
            if (segmentKeys.Length == entityKeys.Length)
            {
                //ToDo this won't handle the edge cases where a value contains an equal sign 
                var passedInKeys = segmentKeys.Select(k => k.Split('='))
                                              .Where(k => k.Length == 2)
                                              .Select(
                                                  k =>
                                                  new KeyValuePair<string, string>(k[0], k[1]))
                                              .ToArray();

                if (passedInKeys.Length > 0)
                {
                    if (!entityKeys.OrderBy(k => k)
                                   .SequenceEqual(passedInKeys.Select(kv => kv.Key)
                                                              .OrderBy(n => n)))
                    {
                        throw new WebAPIDataServiceException("Keys don't match entity model", new KeyNotFoundException());
                    }
                }
                else
                {
                    // build up keyvalue pairs from entitykeys
                    passedInKeys = segmentKeys.Zip(entityKeys,
                                                   (s, k) => new KeyValuePair<string, string>(k, s))
                                              .ToArray();
                }

                // need to build the where clause
                var type = GetIEdmTypeToCLRType(entityType);
                var tSource = Expression.Parameter(type);
                var bodyList = new List<Expression>();

                foreach (var keyPairs in passedInKeys)
                {
                    var tProperty = Expression.Property(tSource, keyPairs.Key);
                    var proptype = type.GetProperty(keyPairs.Key).PropertyType;
                    var keyValue = keyPairs.Value;
                    if (proptype == typeof(string))
                    {
                        // strings usually start with ' and end with ' in OData Calls
                        if (keyValue.StartsWith("'") && keyValue.EndsWith("'"))
                        {
                            keyValue = keyValue.Substring(1, keyValue.Length - 2);
                        }
                    }
                    // Since using EF going to parameterize this
                    var tupleType = typeof(Tuple<>).MakeGenericType(proptype);
                    var tuple = Activator.CreateInstance(tupleType,
                                                         Convert.ChangeType(keyValue, proptype));
                    bodyList.Add(Expression.Equal(tProperty,
                                                  Expression.Property(Expression.Constant(tuple),
                                                                      "Item1")));
                }

                // build up the body expressions adding the And method between each check check
                var body = bodyList[0];
                for (var b = 1; b < bodyList.Count; b++)
                {
                    body = Expression.And(body, bodyList[b]);
                }

                // Expression<Func<TSource, bool>> predicate
                var where =
                    Expression.Lambda(typeof(Func<,>).MakeGenericType(type, typeof(bool)),
                                      body,
                                      tSource);

                // Call the Queryable.Where
                var call = Expression.Call(typeof(Queryable), "Where", new[] { type }, queryable.Expression, where);

                // return back the IQueryable
                return queryable.Provider.CreateQuery(call);
            }
            throw new WebAPIDataServiceException("Keys don't match entity model", new KeyNotFoundException());
        }

        private IHttpActionResult Buildup(ODataPath path)
        {
            var segment = path.Segments[0];
            var edmType = segment.GetEdmType(null);
            var edmCollectionType = edmType as IEdmCollectionType;
            if (edmType == null || edmCollectionType == null)
            {
                throw new WebAPIDataServiceException("Can not resolve OData Path", new ODataUnrecognizedPathException());
            }

            var query = QueryEntitySet(edmCollectionType);
            for (var i = 1; i < path.Segments.Count; i++)
            {
                edmType = segment.GetEdmType(edmType);
                segment = path.Segments[i];
                switch (segment.SegmentKind)
                {
                    case "key":
                        {
                            edmCollectionType = edmType as IEdmCollectionType;
                            var segmentKey = segment as KeyValuePathSegment;
                            query = FilterOnKeys(query, edmCollectionType, segmentKey);
                            break;
                        }
                    case "navigation":
                        {
                            var navigationPath = segment as NavigationPathSegment;
                            query = DrillIntoNavigationProperty(query, navigationPath);
                            break;
                        }
                    case "property":
                        {
                            var propertyPath = segment as PropertyAccessPathSegment;
                            query = ProjectProperty(query, propertyPath);
                            break;
                        }
                    default:
                        {
                            // break out of the loop
                            i = path.Segments.Count + 1;
                            break;
                        }
                }
            }

            // setup to have web api odata take over
            var edmEntityType = GetIEdmTypeToCLRType(GetEdmEntityType(edmType));

            var model = GetMetadata();
            var queryContext = new ODataQueryContext(model, edmEntityType);
            var queryOptions = new ODataQueryOptions(queryContext, Request);

            //ToDo this does the default Web API filtering but would like to get more control over it
            query = queryOptions.ApplyTo(query, new ODataQuerySettings(oDataQuerySettings));


            //ToDo there has to be a better way but this works for now
            //ToDo still missing complex types
            IEdmType resultEdmType;
            resultEdmType = path.EdmType as IEdmCollectionType;
            if (resultEdmType != null)
            {
                return new ODataHttpActionResult(this, query, typeof (IEdmCollectionType),
                                                  typeof (IEnumerable<>).MakeGenericType(query.ElementType));
            }
            resultEdmType = path.EdmType as IEdmEntityType;
            if (resultEdmType!= null)
            {

                // Not excited about this code but works for now
                var firstOrDeafult =
                    ((MethodInfo) MethodBase.GetMethodFromHandle(_firstOrDefault)).MakeGenericMethod(query.ElementType);
                var results = firstOrDeafult.Invoke(null, new[] {query});

                return new ODataHttpActionResult(this, results, typeof (IEdmEntityType),
                                                 query.ElementType);
            }
            if (path.EdmType.TypeKind == EdmTypeKind.Primitive)
            {
                // Not excited about this code but works for now
                var firstOrDeafult =
                    ((MethodInfo)MethodBase.GetMethodFromHandle(_firstOrDefault)).MakeGenericMethod(query.ElementType);
                var results = firstOrDeafult.Invoke(null, new[] { query });

                return new ODataHttpActionResult(this, results, typeof(IEdmPrimitiveType),
                                                 query.ElementType);
            }

            return BadRequest("Not Implemented");

        }

        private MediaTypeFormatter GetFormatter(Type objType)
        {
            //ToDo Generate error if we can't match on the requested content type
            var conneg = Configuration.Services.GetContentNegotiator();
            var result = conneg.Negotiate(objType, Request, Configuration.Formatters);
            return result.Formatter;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_currentDataSource.IsValueCreated)
                {
                    var disposer = CurrentDataSource as IDisposable;
                    if (disposer != null)
                    {
                        disposer.Dispose();
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}