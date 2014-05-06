using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Services.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHail.WebAPI.OData.Tests.CodeFirstPocos
{
    [DataServiceKey("CustomerId")]
    public class Customers
    {
        public Customers()
        {
            this.SalesOrderMasters = new HashSet<SalesOrderMaster>();
        }
        [Key]
        public string CustomerId { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        [ForeignKey("CustomerId")]
        public virtual ICollection<SalesOrderMaster> SalesOrderMasters { get; set; }
    }

    [DataServiceKey("SalesOrderId")]
    public class SalesOrderMaster
    {
        public SalesOrderMaster()
        {
            this.SalesOrderTransactions = new HashSet<SalesOrderTransactions>();
        }
        [Key]
        public int SalesOrderId { get; set; }
        public string CustomerId { get; set; }
        public DateTime Order { get; set; }
        public DateTime Required { get; set; }
        public virtual Customers Customers { get; set; }
        [ForeignKey("SalesOrderId")]
        public virtual ICollection<SalesOrderTransactions> SalesOrderTransactions { get; set; }
    }

    [DataServiceKey("Id")]
    public class SalesOrderTransactions
    {
        public int SalesOrderId { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal QtyOrdered { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public virtual SalesOrderMaster SalesOrderMaster { get; set; }
    }
}
