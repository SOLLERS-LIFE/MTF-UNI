// VFD additional class to extend standart Identity
// VFD to add Context for model and Migration
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using MySqlConnector.Authentication.Ed25519;

namespace MTF.Areas.Identity.Data
{
    public partial class CommonIdent
    // the trick to provide indexes definitions
    // from another parts of partial database context
    // Will be removed by compiler if not defined.
    {
        partial void _defineEntities_ext01(ModelBuilder modelBuilder);
    }
    // This class is more complex then usually - added class to represent
    // extended Identity Roles and a type to build of primary keys for
    // users and roles in the Identity Context database
    public partial class CommonIdent : IdentityDbContext<CommonUser,CommonRole,string>
    {
        public CommonIdent(DbContextOptions<CommonIdent> options)
            : base(options)
        {
            
        }

        public DbSet<CommonConfig> configCommon { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // to enable new user verification
            // with MySqlConnector.Authentication.Ed25519 NuGet Package
            Ed25519AuthenticationPlugin.Install();

            builder.Entity<CommonUser>(entity =>
            {
                entity.Property(f => f.colorModel)
                   .HasDefaultValue("mdb.dark.min.css");
            });
            builder.Entity<CommonUser>(entity =>
            {
                entity.Property(f => f.barsColour)
                   .HasDefaultValue("#0000d4");
            });
            // indexes definitions from first part
            // of partial context
            _defineEntities_ext01(builder);

            base.OnModelCreating(builder);
        }
    }
}