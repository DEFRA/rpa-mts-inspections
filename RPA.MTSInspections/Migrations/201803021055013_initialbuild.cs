namespace RPA.MTSInspections.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialbuild : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuditDisplays",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        User = c.String(),
                        NewValue = c.String(),
                        OldValue = c.String(),
                        PropertyName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        EntityFullName = c.String(),
                        Entity = c.Binary(),
                        EntityId = c.String(),
                        User = c.String(),
                        OldValue = c.String(),
                        NewValue = c.String(),
                        PropertyName = c.String(),
                        Operation = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Criteria",
                c => new
                    {
                        CriteriaId = c.Guid(nullable: false),
                        SchemeId = c.Guid(nullable: false),
                        Name = c.String(),
                        Weighting = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CriteriaId);
            
            CreateTable(
                "dbo.Options",
                c => new
                    {
                        OptionId = c.Guid(nullable: false),
                        CriteriaId = c.Guid(nullable: false),
                        Name = c.String(),
                        Score = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.OptionId)
                .ForeignKey("dbo.Criteria", t => t.CriteriaId, cascadeDelete: true)
                .Index(t => t.CriteriaId);
            
            CreateTable(
                "dbo.Plants",
                c => new
                    {
                        PlantId = c.Guid(nullable: false),
                        Name = c.String(),
                        License = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PlantId);
            
            CreateTable(
                "dbo.PlantOptions",
                c => new
                    {
                        PlantId = c.Guid(nullable: false),
                        SchemeId = c.Guid(nullable: false),
                        OptionId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlantId, t.SchemeId, t.OptionId })
                .ForeignKey("dbo.Options", t => t.OptionId, cascadeDelete: true)
                .ForeignKey("dbo.Plants", t => t.PlantId, cascadeDelete: true)
                .ForeignKey("dbo.Schemes", t => t.SchemeId, cascadeDelete: true)
                .Index(t => t.PlantId)
                .Index(t => t.SchemeId)
                .Index(t => t.OptionId);
            
            CreateTable(
                "dbo.Schemes",
                c => new
                    {
                        SchemeId = c.Guid(nullable: false),
                        Name = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.SchemeId);
            
            CreateTable(
                "dbo.RiskScores",
                c => new
                    {
                        PlantId = c.Guid(nullable: false),
                        SchemeId = c.Guid(nullable: false),
                        Score = c.Int(nullable: false),
                        DateCalculated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlantId, t.SchemeId })
                .ForeignKey("dbo.Plants", t => t.PlantId, cascadeDelete: true)
                .ForeignKey("dbo.Schemes", t => t.SchemeId, cascadeDelete: true)
                .Index(t => t.PlantId)
                .Index(t => t.SchemeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RiskScores", "SchemeId", "dbo.Schemes");
            DropForeignKey("dbo.RiskScores", "PlantId", "dbo.Plants");
            DropForeignKey("dbo.PlantOptions", "SchemeId", "dbo.Schemes");
            DropForeignKey("dbo.PlantOptions", "PlantId", "dbo.Plants");
            DropForeignKey("dbo.PlantOptions", "OptionId", "dbo.Options");
            DropForeignKey("dbo.Options", "CriteriaId", "dbo.Criteria");
            DropIndex("dbo.RiskScores", new[] { "SchemeId" });
            DropIndex("dbo.RiskScores", new[] { "PlantId" });
            DropIndex("dbo.PlantOptions", new[] { "OptionId" });
            DropIndex("dbo.PlantOptions", new[] { "SchemeId" });
            DropIndex("dbo.PlantOptions", new[] { "PlantId" });
            DropIndex("dbo.Options", new[] { "CriteriaId" });
            DropTable("dbo.RiskScores");
            DropTable("dbo.Schemes");
            DropTable("dbo.PlantOptions");
            DropTable("dbo.Plants");
            DropTable("dbo.Options");
            DropTable("dbo.Criteria");
            DropTable("dbo.AuditLogs");
            DropTable("dbo.AuditDisplays");
        }
    }
}
