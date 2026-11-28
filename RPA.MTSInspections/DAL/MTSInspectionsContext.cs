using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace RPA.MTSInspections.DAL
{
    public class MTSInspectionsContext : DbContext
    {
        public MTSInspectionsContext(DbContextOptions<MTSInspectionsContext> options)
            : base(options)
        {
            // Updates are marked explicitly (SetModified), as they were with EF6.
            ChangeTracker.AutoDetectChangesEnabled = false;
        }
        public virtual DbSet<Criteria> Criteria { get; set; }

        public virtual DbSet<Option> Option { get; set; }

        public virtual DbSet<Plant> Plant { get; set; }

        public virtual DbSet<PlantOption> PlantOption { get; set; }

        public virtual DbSet<RiskScore> RiskScore { get; set; }

        public virtual DbSet<Scheme> Scheme { get; set; }

        public virtual DbSet<LastInspection> LastInspection { get; set; }
        
        public virtual DbSet<Audit> Audit { get; set; }

        public virtual void SetModified(Object obj) { Entry(obj).State = EntityState.Modified; }

        // Table, key and column types match the schema created by the previous EF6 model.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scheme>().ToTable("Schemes");
            modelBuilder.Entity<Criteria>().ToTable("Criteria");
            modelBuilder.Entity<Option>().ToTable("Options");
            modelBuilder.Entity<Plant>().ToTable("Plants");

            modelBuilder.Entity<PlantOption>(entity =>
            {
                entity.ToTable("PlantOptions");
                entity.HasKey(x => new { x.PlantId, x.SchemeId, x.OptionId });
            });

            modelBuilder.Entity<LastInspection>(entity =>
            {
                entity.ToTable("LastInspections");
                entity.HasKey(x => new { x.PlantId, x.SchemeId });
                entity.Property(x => x.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<RiskScore>(entity =>
            {
                entity.ToTable("RiskScores");
                entity.HasKey(x => new { x.PlantId, x.SchemeId });
                entity.Property(x => x.DateCalculated).HasColumnType("datetime");
            });

            modelBuilder.Entity<Audit>(entity =>
            {
                entity.ToTable("Audits");
                entity.Property(x => x.Date).HasColumnType("datetime");
            });
        }


    }
}