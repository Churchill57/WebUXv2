using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebUXv2.Models
{
    public class CustomerDbContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public CustomerDbContext() : base("name=CustomerDbContext")
        {
        }

        public System.Data.Entity.DbSet<WebUXv2.Models.Customer> Customers { get; set; }

        public System.Data.Entity.DbSet<WebUXv2.Models.PropertyAddress> PropertyAddresses { get; set; }

        public System.Data.Entity.DbSet<WebUXv2.Models.SecondLineDefenceQuestion> SecondLineDefenceQuestions { get; set; }

        public System.Data.Entity.DbSet<WebUXv2.Models.PAPolicy> PAPolicies { get; set; }

    }
}
