using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace RPA.MTSInspections.DAL
{
    [DbConfigurationType(typeof(MTSInspectionsDbConfiguration))]
    public class MTSInspectionsContext : DbContext
    {
        // Required by DbMigrator, which creates the context itself; the migration configuration supplies the real connection.
        public MTSInspectionsContext()
            : base("MTSInspectionsContext")
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        public MTSInspectionsContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
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


    }
}