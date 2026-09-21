namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class plantDetails : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plants", "AddressOne", c => c.String());
            AddColumn("dbo.Plants", "AddressTwo", c => c.String());
            AddColumn("dbo.Plants", "TownCity", c => c.String());
            AddColumn("dbo.Plants", "County", c => c.String());
            AddColumn("dbo.Plants", "PostCode", c => c.String());
            AddColumn("dbo.Plants", "Licence", c => c.String(nullable: false));
            CreateIndex("dbo.Criteria", "SchemeId");
            AddForeignKey("dbo.Criteria", "SchemeId", "dbo.Schemes", "SchemeId", cascadeDelete: false);
            DropColumn("dbo.Plants", "License");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Plants", "License", c => c.String());
            DropForeignKey("dbo.Criteria", "SchemeId", "dbo.Schemes");
            DropIndex("dbo.Criteria", new[] { "SchemeId" });
            DropColumn("dbo.Plants", "Licence");
            DropColumn("dbo.Plants", "PostCode");
            DropColumn("dbo.Plants", "County");
            DropColumn("dbo.Plants", "TownCity");
            DropColumn("dbo.Plants", "AddressTwo");
            DropColumn("dbo.Plants", "AddressOne");
        }
    }
}
