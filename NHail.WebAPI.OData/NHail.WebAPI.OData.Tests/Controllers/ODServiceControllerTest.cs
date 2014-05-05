using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Validation;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHail.WebAPI.OData;
using NHail.WebAPI.OData.Controllers;
//using NHail.WebAPI.OData.Tests.CodeFirstPocos;
using NHail.WebAPI.OData.Tests.Contexts;
using Newtonsoft.Json.Linq;
using Pocos;

namespace NHail.WebAPI.OData.Tests.Controllers
{
    [TestClass]
    public class ODServiceControllerTest
    {
        public TestContext TestContext { get; set; }

        private static IDisposable _owinHost;
        private const string BaseAddress = "http://localhost:9000/";
        private const string ServiceAddress = BaseAddress + "api/odata/";
        private readonly static HttpClient _httpClient = new HttpClient(Startup.HttpServer);

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            // Start OWIN host 
            _owinHost = WebApp.Start<Startup>(BaseAddress);

            // set the default headers
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", .9));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml", .8));
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            _owinHost.Dispose();
            _httpClient.Dispose();

        }

        [TestMethod]
        public async Task CheckMetaData()
        {
            var response = await _httpClient.GetAsync(ServiceAddress + "$metadata");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();

            // make sure we can parse the metadata
            IEdmModel model;
            IEnumerable<EdmError> errors;
            Assert.IsTrue(EdmxReader.TryParse(XmlReader.Create(new StringReader(content)), out model, out errors));
        }

        [TestMethod]
        public async Task CheckServiceDocument()
        {
            var response = await _httpClient.GetAsync(ServiceAddress);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Customers") &&
                          content.Contains("SalesOrderMasters") &&
                          content.Contains("SalesOrderTransactions"));

        }

        [TestMethod]
        public async Task CheckCustomers()
        {
            var context = new DataServiceContext(new Uri(ServiceAddress), DataServiceProtocolVersion.V3)
                {
                    MergeOption = MergeOption.OverwriteChanges
                };
            var query = context.CreateQuery<Customers>("Customers");
            var response =
                await
                Task.Factory.FromAsync<IEnumerable<Customers>>(query.BeginExecute, query.EndExecute, null)
                    .ConfigureAwait(false);
            var customers = response.ToArray();
            // Make sure we got our 3 customers back
            Assert.AreEqual(3, customers.Length);

            //Check if MS001 is in there
            Assert.IsTrue(customers.Any(c => c.CustomerId == "MS001"));
        }

        [TestMethod]
        public async Task CheckKey()
        {
            var response = await _httpClient.GetAsync(ServiceAddress + "Customers('GOO001')");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("Customers/@Element") && content.Contains("Google"));
        }

        [TestMethod]
        public async Task CheckNavigation()
        {
            var response = await _httpClient.GetAsync(ServiceAddress + "Customers('APP001')/SalesOrderMasters");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(3, Regex.Matches(content, "SalesOrderId").Count);
        }

        [TestMethod]
        public async Task CheckProperty()
        {
            var response =
                await
                _httpClient.GetAsync(ServiceAddress +
                                     "Customers('APP001')/SalesOrderMasters(1)/SalesOrderTransactions(1)/Item");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(content.Contains("ITEM1"));
        }

        [TestMethod]
        public async Task CheckProjection()
        {
            var context = new DataServiceContext(new Uri(ServiceAddress), DataServiceProtocolVersion.V3)
            {
                MergeOption = MergeOption.OverwriteChanges
            };
            var query = (DataServiceQuery)context.CreateQuery<Customers>("Customers").Select(c => new
                {
                    c.CustomerId,
                    c.Company,
                    c.State
                });
            var response =
                await
                Task.Factory.FromAsync<IEnumerable>(query.BeginExecute, query.EndExecute, null)
                   .ConfigureAwait(false);
            var check = ((IEnumerable<object>) response).ToArray();
            
             Assert.AreEqual(3, check.Length);
        }
    }
}
