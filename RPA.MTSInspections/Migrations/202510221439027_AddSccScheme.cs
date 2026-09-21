namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSccScheme : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO Schemes (SchemeId, Name, Active, Weighting) VALUES (NEWID(), 'SCC', 1, 0)");
        }
        
        public override void Down()
        {
            Sql("DELETE FROM Schemes WHERE Name = 'SCC'");
        }
    }
}
