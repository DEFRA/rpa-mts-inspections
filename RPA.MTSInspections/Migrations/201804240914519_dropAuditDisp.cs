namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dropAuditDisp : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.AuditDisplays");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.AuditDisplays",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        User = c.String(),
                        NewValue = c.String(),
                        OldValue = c.String(),
                        PropertyName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
