namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class plantIdtoAudit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Audits", "PlantId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Audits", "PlantId");
        }
    }
}
