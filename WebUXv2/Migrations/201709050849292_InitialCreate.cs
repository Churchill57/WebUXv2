namespace WebUXv2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LuTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ClientRef = c.String(nullable: false),
                        State = c.String(),
                        Status = c.Int(nullable: false),
                        ParentLuTaskId = c.Int(),
                        UserName = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LuTasks", t => t.ParentLuTaskId)
                .Index(t => t.ParentLuTaskId);
            
            CreateTable(
                "dbo.TaskTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Host = c.String(nullable: false),
                        Name = c.String(),
                        RootComponentName = c.String(nullable: false),
                        SearchTags = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TaskInputs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.String(),
                        TaskTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaskTypes", t => t.TaskTypeId, cascadeDelete: true)
                .Index(t => t.TaskTypeId);
            
            CreateTable(
                "dbo.UxTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ClientRef = c.String(nullable: false),
                        State = c.String(),
                        ParentLuTaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LuTasks", t => t.ParentLuTaskId, cascadeDelete: true)
                .Index(t => t.ParentLuTaskId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UxTasks", "ParentLuTaskId", "dbo.LuTasks");
            DropForeignKey("dbo.TaskInputs", "TaskTypeId", "dbo.TaskTypes");
            DropForeignKey("dbo.LuTasks", "ParentLuTaskId", "dbo.LuTasks");
            DropIndex("dbo.UxTasks", new[] { "ParentLuTaskId" });
            DropIndex("dbo.TaskInputs", new[] { "TaskTypeId" });
            DropIndex("dbo.LuTasks", new[] { "ParentLuTaskId" });
            DropTable("dbo.UxTasks");
            DropTable("dbo.TaskInputs");
            DropTable("dbo.TaskTypes");
            DropTable("dbo.LuTasks");
        }
    }
}
