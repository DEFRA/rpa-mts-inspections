namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dateOption : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LastInspections",
                c => new
                    {
                        PlantId = c.Guid(nullable: false),
                        SchemeId = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlantId, t.SchemeId })
                .ForeignKey("dbo.Plants", t => t.PlantId, cascadeDelete: true)
                .ForeignKey("dbo.Schemes", t => t.SchemeId, cascadeDelete: true)
                .Index(t => t.PlantId)
                .Index(t => t.SchemeId);
            
            DropColumn("dbo.Options", "Date");
            DropColumn("dbo.Options", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Options", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Options", "Date", c => c.DateTime());
            DropForeignKey("dbo.LastInspections", "SchemeId", "dbo.Schemes");
            DropForeignKey("dbo.LastInspections", "PlantId", "dbo.Plants");
            DropIndex("dbo.LastInspections", new[] { "SchemeId" });
            DropIndex("dbo.LastInspections", new[] { "PlantId" });
            DropTable("dbo.LastInspections");
        }
    }
}
