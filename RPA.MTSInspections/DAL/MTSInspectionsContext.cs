using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace RPA.MTSInspections.DAL
{
    [DbConfigurationType(typeof(MTSInspectionsDbConfiguration))]
    public class MTSInspectionsContext : DbContext
    {
        static MTSInspectionsContext()
        {
            // The schema is released separately (DBA / pipeline); the runtime must never create or alter it implicitly.
            Database.SetInitializer<MTSInspectionsContext>(null);
        }

        // Required by DbMigrator, which creates the context itself; the migration configuration supplies the real connection.
        public MTSInspectionsContext()
            : base("MTSInspectionsContext")
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        // The connection is created explicitly so the Microsoft.Data.SqlClient provider is always used.
        public MTSInspectionsContext(string connectionString)
            : base(new SqlConnection(connectionString), true)
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