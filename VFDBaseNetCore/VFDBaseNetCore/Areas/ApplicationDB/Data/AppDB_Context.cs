using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;

using MySqlConnector.Authentication.Ed25519;

using MTF.Utilities;
using MTF.Areas.ApplicationDB.Models;

namespace MTF.Areas.ApplicationDB.Data
{
    public partial class AppDB_Context
    // the trick to provide indexes definitions
    // from another parts of partial database context
    // Will be removed by compiler if not defined.
    {
        partial void _defineEntities_ext01(ModelBuilder modelBuilder);
    }
    public partial class AppDB_Context : DbContextWithScalarReturn, IProduct
    {
        public string UID { get; }
        // The idea is to prepere context for second usage with same data.
        // As such we should stop all tracing of records startted in the
        // previous life.
        public void prehireCleaning()
        {
            DetachAllEntities();
        }
        public AppDB_Context(DbContextOptions<AppDB_Context> options)
            : base(options)
        {
            UID = Guid.NewGuid().ToString();
            // increase operation timeout
            Database.SetCommandTimeout((int)TimeSpan.FromMinutes(GlobalParameters._appDB_ConnectionTimeout).TotalSeconds);
        }
        // This second protected constructor allows to avoid problems
        // with inherited classes constructor
        // protected constructor is not visible to model builder but
        // it can be used in derived classes 
        protected AppDB_Context(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<bs_security_disable> _bs_security_disable { get; set; }
        public DbSet<bs_objects_protocol> _objects_protocol { get; set; }
        public DbSet<sa_tech_ticks_protocol> _sa_tech_ticks_protocol { get; set; }
        public DbSet<sa_uidTranslator> _sa_uidTranslator { get; set; }
        public DbSet<sa_user_configs> _sa_user_configs { get; set; }
        public DbSet<sa_global_configs> _sa_global_configs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // to enable new user verification
            // with MySqlConnector.Authentication.Ed25519 NuGet Package
            Ed25519AuthenticationPlugin.Install();

            modelBuilder.Entity<bs_security_disable>()
                .HasIndex(f => new { f.id_user })
                .HasDatabaseName("IX_bs_security_disable_id_user");

            modelBuilder.Entity<bs_objects_protocol>()
                .HasIndex(f => new { f._type, f.id_object })
                .HasDatabaseName("IX_bs_protocol_type_id_object");

            modelBuilder.Entity<sa_uidTranslator>()
                .HasIndex(f => new { f.uid })
                .IsUnique();

            modelBuilder.Entity<sa_user_configs>()
                .HasKey(f => new { f.uid, f.cnfName })
                ;

            // indexes definitions from first part
            // of partial context
            _defineEntities_ext01(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }
    }
}
