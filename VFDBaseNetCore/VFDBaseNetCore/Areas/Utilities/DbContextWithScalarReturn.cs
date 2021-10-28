using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTF.Utilities
{
    public class DbContextWithScalarReturn : DbContext
    {
        public class ScalarReturn
        {
            [Required]
            public int RetValueInt { get; set; }
            [StringLength(2048)]
            public string RetValueString { get; set; }
        }
        
        // these two constructors allows to avoid problem with inhereted
        // classes constructors
        public DbContextWithScalarReturn(DbContextOptions<DbContextWithScalarReturn> options)
            : base(options)
        {
        }
        protected DbContextWithScalarReturn(DbContextOptions options)
            : base(options)
        {
            // protected constructor is not visible to model builder but can be used
            // in derived classes
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Pattern to ignore view or stored procedures results
            // fictive entities during tables build
            if (!GlobalParameters.IsStartedWithMain)
            {
                modelBuilder.Ignore<ScalarReturn>();
            }
            else
            {
                modelBuilder
                  .Entity<ScalarReturn>().HasNoKey();
            }
        }

        // Method to stop tracking all the entities
        // The idea is to reuse context in different
        // queries and branches with the same user
        public void DetachAllEntities()
        {
            var attachedEntries = this.ChangeTracker.Entries()
                .ToList();

            foreach (var entry in attachedEntries)
                entry.State = EntityState.Detached;
        }

        // VFD Create datasets for every Entity from Models
        public DbSet<ScalarReturn> _sr { get; set; } // VFD for returning scalar values

        // VFD to return scalar values
        public async Task<ScalarReturn> getScalarValue(string sql_query,
                                                       params object[] prms
                                                      )
        {
            var lng = prms.Length;
            var sql_query_md = sql_query+"(";
            if (lng > 0)
            {
                sql_query_md += "{0}";
                for (int i = 1; i < lng; i++)
                {
                    sql_query_md += ",{" + i.ToString() + "}";
                }
            }
            sql_query_md += ");";

            try
            {
                var items = await _sr
                                  .FromSqlRaw(sql_query_md,
                                              prms
                                             )
                                  .AsNoTracking()
                                  .ToListAsync();
                return items.Count() == 0
                       ? new ScalarReturn { RetValueInt = -1, RetValueString = "" }
                       : items.ElementAtOrDefault(0);
            }
            catch
            { 
                throw; 
            }
        }

        public ScalarReturn getScalarValueSync(string sql_query,
                                               params object[] prms
                                              )
        {
            var lng = prms.Length;
            var sql_query_md = sql_query + "(";
            if (lng > 0)
            {
                sql_query_md += "{0}";
                for (int i = 1; i < lng; i++)
                {
                    sql_query_md += ",{" + i.ToString() + "}";
                }
            }
            sql_query_md += ");";

            try
            {
                var items = _sr
                            .FromSqlRaw(sql_query_md,
                                        prms
                                       )
                            .AsNoTracking()
                            .ToList();
                return items.Count() == 0
                       ? new ScalarReturn { RetValueInt = -1, RetValueString = "" }
                       : items.ElementAtOrDefault(0);
            }
            catch
            {
                throw;
            }
        }
    }
}

