namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CriteriaOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Criteria", "Order", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Criteria", "Order");
        }
    }
}
