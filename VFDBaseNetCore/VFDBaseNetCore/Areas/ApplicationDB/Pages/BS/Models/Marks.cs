using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using MTF.Areas.ApplicationDB.Pages.BS.Models;

namespace MTF.Areas.ApplicationDB.Pages.BS.Models
{
    [Table("bs_marks")]
    public class bs_marks
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(127)]
        [Display(Name = "name")]
        public string name { get; set; }
    }

    // It is UPDATABLE view, thats why I use PK with it
    [Table("bs_marks_v")]
    public class bs_marks_v
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(127)]
        [Display(Name = "mark_name")]
        public string mark_name { get; set; }
        [Required]
        public int numRecs { get; set; }
        [Required]
        [StringLength(1024)]
        public string accessedBy { get; set; }
        [Required]
        [StringLength(1024)]
        public string team_name { get; set; }
    }

    [Table("bs_marks_to_users")]
    public class bs_marks_to_users
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "id_mark")]
        public int id_mark { get; set; }
        [Required]
        [Display(Name = "id_team")]
        public int id_team { get; set; }

        [Required]
        [StringLength(64)]
        public string id_user { get; set; }
    }
}

namespace MTF.Areas.ApplicationDB.Data
{
    public partial class AppDB_Context
    {
        // the trick to privide indexes definitions
        // from another parts of partial database context
        // Will be removed by compiler if not defined.
        partial void _defineEntities_ext04(ModelBuilder modelBuilder);
    }

    public partial class AppDB_Context
    {
        public DbSet<bs_marks> _bs_marks { get; set; }
        public DbSet<bs_marks_v> _bs_marks_v { get; set; }
        public DbSet<bs_marks_to_users> _bs_marks_to_users { get; set; }
        partial void _defineEntities_ext03(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<bs_marks>()
                .HasIndex(f => f.name)
                
                .IsUnique();

            modelBuilder.Entity<bs_marks_to_users>()
               .HasIndex(f => new { f.id_mark, f.id_user })
               
               .IsUnique()
               .HasDatabaseName("IX__bs_marks_to_users_id_mark_id_user");

            // Pattern to ignore view or stored procedures results
            // fictive entities during tables build
            if (!GlobalParameters.IsStartedWithMain)
            {
                modelBuilder.Ignore<bs_marks_v>();
            }
            else
            { }

            // indexes definitions from second part
            // of partial context
            _defineEntities_ext04(modelBuilder);
        }
    }
}

