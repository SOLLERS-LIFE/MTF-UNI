using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MTF.Areas.CommonDB.Models;
using MTF.Areas.Identity.Pages.SURoom.Models;
using MTF.Utilities;

using MySqlConnector.Authentication.Ed25519;

namespace MTF.Areas.CommonDB.Data
{
    public partial class CommonDB_Context : DbContextWithScalarReturn
    {
        public CommonDB_Context(DbContextOptions<CommonDB_Context> options)
            : base(options)
        {
        }
        // This second protected constructor allows to avoid problems
        // with inherited classes constructor
        // protected constructor is not visible to model builder but
        // it can be used in derived classes 
        protected CommonDB_Context(DbContextOptions options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<ExchangeStore> exchangeStore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // to enable new user verification
            // with MySqlConnector.Authentication.Ed25519 NuGet Package
            Ed25519AuthenticationPlugin.Install();

            modelBuilder.Entity<ExchangeStore>()
                .HasIndex(f => f.UserUID)
                ;

            modelBuilder.Entity<cachedCommonUser>()
                .HasIndex(f => f.ActorId)
                ;

            modelBuilder.Entity<cacheControl>()
                .HasIndex(f => f.uid)
                ;

            base.OnModelCreating(modelBuilder);
        }
        public async Task putToHead(string qname, // name of lifo (usuall UserUID)
                                    string s // value to keep
                                   )
        {
            this.exchangeStore.Add(new ExchangeStore { UserUID = qname, ObjectDescr = s });
            await this.SaveChangesAsync();
        }
        public async Task<string> getTail(string qname // name of substore (usuall UserUID)
                                         )
        {
            var lstrings = await this.exchangeStore
                                     .FromSqlRaw("execute [dbo].[getTail] {0}", qname)
                                     .ToListAsync();
            if (lstrings.Count() == 0)
            {
                return string.Empty;
            }
            return lstrings.ElementAtOrDefault(0).ObjectDescr;
        }
    }
}
