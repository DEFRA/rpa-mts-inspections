namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class plantnamereq : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Plants", "PlantName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Plants", "PlantName", c => c.String());
        }
    }
}
