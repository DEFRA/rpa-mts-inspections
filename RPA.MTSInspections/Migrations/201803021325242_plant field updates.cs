namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class plantfieldupdates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plants", "PlantName", c => c.String());
            AddColumn("dbo.Plants", "LicenceNo", c => c.String(nullable: false));
            DropColumn("dbo.Plants", "Name");
            DropColumn("dbo.Plants", "Licence");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Plants", "Licence", c => c.String(nullable: false));
            AddColumn("dbo.Plants", "Name", c => c.String());
            DropColumn("dbo.Plants", "LicenceNo");
            DropColumn("dbo.Plants", "PlantName");
        }
    }
}
