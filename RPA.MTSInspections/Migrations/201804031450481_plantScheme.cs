namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class plantScheme : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Schemes", "Weighting", c => c.Int(nullable: false));
            AddColumn("dbo.Plants", "Bcc", c => c.Boolean(nullable: false));
            AddColumn("dbo.Plants", "Bls", c => c.Boolean(nullable: false));
            AddColumn("dbo.Plants", "Pcg", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Plants", "Pcg");
            DropColumn("dbo.Plants", "Bls");
            DropColumn("dbo.Plants", "Bcc");
            DropColumn("dbo.Schemes", "Weighting");
        }
    }
}
