using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;

using MySqlConnector.Authentication.Ed25519;

using MTF.Utilities;
using MTF.Areas.Logging.Models;

namespace MTF.Areas.Logging.Data
{
    public partial class LogDB_Context
    // the trick to provide indexes definitions
    // from another parts of partial database context
    // Will be removed by compiler if not defined.
    {
        partial void _defineEntities_ext01(ModelBuilder modelBuilder);
    }
    public partial class LogDB_Context : DbContextWithScalarReturn
    {
        public DbSet<GeneralLog> generalLog { get; set; }
        public DbSet<GeneralLogCatched> generalLogCatched { get; set; }
        public DbSet<GeneralLog_v> generalLog_v { get; set; }
        public DbSet<cacheControl> cacheControls { get; set; }
        public DbSet<combinedFilters> _combinedFilters { get; set; }

        public DbSet<ClaimsLog> claimsLog { get; set; }
        public DbSet<ClaimsLogCatched> claimsLogCatched { get; set; }
        public DbSet<ClaimsLog_v> claimsLog_v { get; set; }
        public DbSet<ClaimsLogcacheControl> claimsLogcacheControl { get; set; }

        public DbSet<GeneralLogPermanent> GeneralLogPermanent { get; set; }

        public LogDB_Context(DbContextOptions<LogDB_Context> options)
            : base(options)
        {        
        }
        // This second protected constructor allows to avoid problems
        // with inherited classes constructor
        // protected constructor is not visible to model builder but
        // it can be used in derived classes 
        protected LogDB_Context(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // to enable new user verification
            // with MySqlConnector.Authentication.Ed25519 NuGet Package
            Ed25519AuthenticationPlugin.Install();

            modelBuilder.Entity<GeneralLog>()
                .HasIndex(f => f.machineName)
                ;
            modelBuilder.Entity<GeneralLog>()
                .HasIndex(f => f.appIdent)
                ;
            modelBuilder.Entity<GeneralLog>()
                .HasIndex(f => f.logged)
                ;
            modelBuilder.Entity<GeneralLog>()
                .HasIndex(f => f.message)
                ;
            modelBuilder.Entity<GeneralLog>()
                .HasIndex(f => f.uId)
                ;
            modelBuilder.Entity<GeneralLog>(entity =>
            {
                entity.Property(f => f.machineName)
                   .HasDefaultValue("");
                entity.Property(f => f.appIdent)
                   .HasDefaultValue("");
                entity.Property(f => f.message)
                   .HasDefaultValue("");
                entity.Property(f => f.callsite)
                   .HasDefaultValue("");
                entity.Property(f => f.logger)
                   .HasDefaultValue("");
                entity.Property(f => f.properties)
                   .HasDefaultValue("");
                entity.Property(f => f.reqhost)
                   .HasDefaultValue("");
                entity.Property(f => f.uId)
                   .HasDefaultValue("");
                entity.Property(f => f.url)
                   .HasDefaultValue("");
                entity.Property(f => f._exception)
                   .HasDefaultValue("");
                entity.Property(f => f._level)
                   .HasDefaultValue("");
            });

            modelBuilder.Entity<GeneralLogCatched>()
                .HasIndex(f => f.actorId)
                ;
            modelBuilder.Entity<GeneralLogCatched>()
                .HasIndex(f => f.machineName)
                ;
            modelBuilder.Entity<GeneralLogCatched>()
                .HasIndex(f => f.appIdent)
                ;
            modelBuilder.Entity<GeneralLogCatched>()
                .HasIndex(f => f.logged)
                ;
            modelBuilder.Entity<GeneralLogCatched>()
                .HasIndex(f => f.message)
                ;

            modelBuilder.Entity<combinedFilters>()
                .HasKey(f => new { f.uid, f.subsys })
                ;

            modelBuilder.Entity<ClaimsLog>()
               .HasIndex(f => f.author)
               ;
            modelBuilder.Entity<ClaimsLog>()
               .HasIndex(f => new { f.logged, f.viewed })
               ;
            modelBuilder.Entity<ClaimsLog>()
               .HasIndex(f => f.claimText)
               ;

            modelBuilder.Entity<ClaimsLogCatched>()
               .HasIndex(f => f.actorId)
               ;
            modelBuilder.Entity<ClaimsLogCatched>()
               .HasIndex(f => new { f.logged, f.viewed })
               ;

            modelBuilder.Entity<GeneralLogPermanent>()
                .HasIndex(f => f.logged)
                ;
            modelBuilder.Entity<GeneralLogPermanent>()
                .HasIndex(f => f.message)
                ;

            if (!GlobalParameters.IsStartedWithMain)
            {
                modelBuilder.Ignore<GeneralLog_v>();
                modelBuilder.Ignore<ClaimsLog_v>();
            }
            else
            {
            }

            // indexes definitions from first part
            // of partial context
            _defineEntities_ext01(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}
