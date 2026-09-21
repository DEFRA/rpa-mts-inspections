namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newAudit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Audits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        User = c.String(),
                        Action = c.String(),
                        PropertyName = c.String(),
                        Value = c.String(),
                        NewValue = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.AuditLogs");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        EntityFullName = c.String(),
                        Entity = c.Binary(),
                        EntityId = c.String(),
                        User = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                        PropertyName = c.String(),
                        Operation = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.Audits");
        }
    }
}
