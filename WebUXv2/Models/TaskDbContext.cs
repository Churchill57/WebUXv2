using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WebUXv2.Models
{
    public class TaskDbContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public TaskDbContext() : base("name=TaskDbContext")
        {
            //Database.Log = (sql => Debug.Write(sql));
        }

        public System.Data.Entity.DbSet<WebUXv2.Models.TaskType> TaskTypes { get; set; }

        public System.Data.Entity.DbSet<WebUXv2.Models.LuTask> LuTasks { get; set; }

        public System.Data.Entity.DbSet<WebUXv2.Models.UxTask> UxTasks { get; set; }
    }
}
