using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHail.WebAPI.OData.Tests.CodeFirstPocos;

namespace NHail.WebAPI.OData.Tests.Contexts
{
    public class CodeFirstContext : DbContext
    {
        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path).TrimEnd('\\') + "\\";
            }
        }

        private static string ConnectionString
        {
            get 
            { 
                var cs = new SqlConnectionStringBuilder
                    {
                        DataSource = @"(LocalDb)\v11.0",
                        IntegratedSecurity = true,
                        AttachDBFilename = AssemblyDirectory + "TestData.mdf"
                    };
                return cs.ConnectionString;
            }
        }

        public CodeFirstContext()
            : base(ConnectionString)
        {
            Database.SetInitializer(new CodeFirstSeedData());
        }

        public IDbSet<Customers> Customers { get; set; }
        public IDbSet<SalesOrderMaster> SalesOrderMasters { get; set; }
        public IDbSet<SalesOrderTransactions> SalesOrderTransactions { get; set; }
    }
}
