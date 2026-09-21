namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schemebool : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Schemes", "Weighting");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Schemes", "Weighting", c => c.Int(nullable: false));
        }
    }
}
