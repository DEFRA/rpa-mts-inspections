namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSccToPlant : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Plants", "SCC", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plants", "SCC");
        }
    }
}
