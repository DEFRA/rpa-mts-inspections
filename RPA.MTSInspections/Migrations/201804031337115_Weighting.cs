namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Weighting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Schemes", "Weighting", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Schemes", "Weighting");
        }
    }
}
