using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

using MTF.Pages.LookingForward.Models;

namespace MTF.Areas.ApplicationDB.Data
{
    public partial class AppDB_Context
    {
        // the trick to privide indexes definitions
        // from another parts of partial database context
        // Will be removed by compiler if not defined.
        partial void _defineEntities_ext02(ModelBuilder modelBuilder);
    }
    public partial class AppDB_Context
    {
        public DbSet<TGO_trivial_table> trivial_table { get; set; }
        public DbSet<TGO_trivial_table_v> trivial_table_v { get; set; }

        // indexes definitions from first part
        // of partial context
        partial void _defineEntities_ext01(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TGO_trivial_table>()
                .HasIndex(f => f.ActorId)
                ;

            // Pattern to ignore view or stored procedures results
            // fictive entities during tables build
            if (!GlobalParameters.IsStartedWithMain)
            {
                modelBuilder.Ignore<TGO_trivial_table_v>();
            }
            else
            {
                /*modelBuilder.Entity<TGO_trivial_table_v>()
                .HasIndex(f => f.ActorId)
                ;*/
            }
            // indexes definitions from second part
            // of partial context
            _defineEntities_ext02(modelBuilder);
        }
    }
}

namespace MTF.Pages.LookingForward.Models
{
    [Table("TGO_trivial_table")]
    public class TGO_trivial_table
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(127)]
        [Display(Name = "ActorId")]
        public string ActorId { get; set; }

        [Display(Name = "Action Date")]  // VFD
        [DataType(DataType.Date)]
        public DateTime ActionDate { get; set; }

        [Display(Name = "Exact Date")]  // VFD
        public DateTime ExactDate { get; set; }

        [Display(Name = "Sel")]
        [DefaultValue(false)]
        public bool Selected { get; set; }
    }

    // It is UPDATABLE view, thats why I use PK with it
    [Table("TGO_trivial_table_v")]
    public class TGO_trivial_table_v
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Action Date")]  // VFD
        [DataType(DataType.Date)]
        public DateTime ActionDate { get; set; }

        [Display(Name = "Exact Date")]  // VFD
        public DateTime ExactDate { get; set; }

        [Display(Name = "Sel")]
        [DefaultValue(false)]
        public bool Selected { get; set; }
    }
}
