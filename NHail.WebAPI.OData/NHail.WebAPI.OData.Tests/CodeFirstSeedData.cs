using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHail.WebAPI.OData.Tests.CodeFirstPocos;
using NHail.WebAPI.OData.Tests.Contexts;

namespace NHail.WebAPI.OData.Tests
{
    public class CodeFirstSeedData : CreateDatabaseIfNotExists<CodeFirstContext>
    {
        protected override void Seed(CodeFirstContext context)
        {
            context.Customers.Add(
                new Customers()
                    {
                        CustomerId = "GOO001",
                        Address = "Google Mountain View",
                        City = "Mountain View",
                        Company = "Google",
                        Phone = "(650) 253-0000",
                        State = "CA"
                    });
            context.Customers.Add(
                new Customers()
                    {
                        CustomerId = "MS001",
                        Address = "One Microsoft Way",
                        City = "Redmond",
                        Company = "Microsoft",
                        Phone = "(425) 882-8080",
                        State = "WA"
                    });
            context.Customers.Add(
                new Customers()
                    {
                        CustomerId = "APP001",
                        Address = "1 Infinite Loop",
                        City = "Cupertino",
                        Company = "Apple",
                        Phone = "(408) 996-1010",
                        State = "CA"
                    });

            context.SalesOrderMasters.Add(
                new SalesOrderMaster()
                    {
                        CustomerId = "APP001",
                        Order = DateTime.Now,
                        Required = DateTime.Now + new TimeSpan(3, 0, 0, 0),
                        SalesOrderId = 1
                    });
            context.SalesOrderMasters.Add(
                new SalesOrderMaster()
                    {
                        CustomerId = "MS001",
                        Order = DateTime.Now - new TimeSpan(-7, 0, 0, 0),
                        Required = DateTime.Now,
                        SalesOrderId = 2
                    });
            context.SalesOrderMasters.Add(
                new SalesOrderMaster()
                {
                    CustomerId = "APP001",
                    Order = DateTime.Now + new TimeSpan(-1, 0, 0, 0),
                    Required = DateTime.Now + new TimeSpan(5, 0, 0, 0),
                    SalesOrderId = 3
                });
            context.SalesOrderMasters.Add(
                new SalesOrderMaster()
                {
                    CustomerId = "APP001",
                    Order = DateTime.Now - new TimeSpan(-7, 0, 0, 0),
                    Required = DateTime.Now + new TimeSpan(30, 0, 0, 0),
                    SalesOrderId = 4
                });
            context.SalesOrderTransactions.Add(new SalesOrderTransactions()
                {
                    SalesOrderId = 1,
                    Description = "Item One",
                    Item = "ITEM1",
                    Price = 3M,
                    QtyOrdered = 5M
                });
            context.SalesOrderTransactions.Add(new SalesOrderTransactions()
            {
                SalesOrderId = 1,
                Description = "Item Two",
                Item = "ITEM2",
                Price = 2M,
                QtyOrdered = 7M
            }); context.SalesOrderTransactions.Add(new SalesOrderTransactions()
            {
                SalesOrderId = 2,
                Description = "SO 2 Item One",
                Item = "SO2ITEM1",
                Price = 9M,
                QtyOrdered = 15M
            }); 
            context.SalesOrderTransactions.Add(new SalesOrderTransactions()
            {
                SalesOrderId = 3,
                Description = "SO 3 Item One",
                Item = "SO3ITEM1",
                Price = 83M,
                QtyOrdered = 95M
            });
            context.SalesOrderTransactions.Add(new SalesOrderTransactions()
            {
                SalesOrderId = 3,
                Description = "SO 3 Item two",
                Item = "SO3ITEM2",
                Price = 183M,
                QtyOrdered = 195M
            });
            context.SalesOrderTransactions.Add(new SalesOrderTransactions()
            {
                SalesOrderId = 4,
                Description = "SO 4 Item 1",
                Item = "SO4ITEM1",
                Price = 1M,
                QtyOrdered = 1925M
            });
            base.Seed(context);
        }
    }
}
