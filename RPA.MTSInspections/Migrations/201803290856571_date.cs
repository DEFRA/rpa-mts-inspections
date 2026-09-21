namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class date : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Options", "Date", c => c.DateTime());
            AddColumn("dbo.Options", "Discriminator", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Options", "Discriminator");
            DropColumn("dbo.Options", "Date");
        }
    }
}
