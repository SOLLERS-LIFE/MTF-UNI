using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

using MTF.Hubs;

namespace MTF.Areas.Logging.Data
{
    public partial class LogDB_Context
    {
        // the trick to privide indexes definitions
        // from another parts of partial database context
        // Will be removed by compiler if not defined.
        partial void _defineEntities_ext02(ModelBuilder modelBuilder);
    }
    public partial class LogDB_Context
    {
        public DbSet<ChatHistory> chatHistory { get; set; }
        public DbSet<ChatHistory_v> chatHistory_v { get; set; }

        // indexes definitions from first part
        // of partial context
        partial void _defineEntities_ext01(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatHistory>()
                .HasIndex(f => f.dateIn)
                ;
            modelBuilder.Entity<ChatHistory>()
                .HasIndex(f => f.uId)
                ;
            modelBuilder.Entity<ChatHistory>()
                .HasIndex(f => f.msg)
                ;
            modelBuilder.Entity<ChatHistory>()
                .HasIndex(f => f.machineName)
                ;
            modelBuilder.Entity<ChatHistory>()
                .HasIndex(f => f.appIdent)
                ;

            // Pattern to ignore view or stored procedures results
            // fictive entities during tables build
            if (!GlobalParameters.IsStartedWithMain)
            {
                modelBuilder.Ignore<ChatHistory_v>();
            }
            else
            {
            }
            // indexes definitions from second part
            // of partial context
            _defineEntities_ext02(modelBuilder);
        }
    }
}

namespace MTF.Hubs
{
    [Table("ChatHistory")]
    public class ChatHistory
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public DateTime dateIn { get; set; }
        [Required]
        [StringLength(64)]
        public string uId { get; set; }
        [StringLength(1024)]
        public string msg { get; set; }
        [Display(Name = "machineName")]
        [StringLength(300)]
        public string machineName { get; set; }
        [StringLength(64)]
        public string appIdent { get; set; }
    }

    [Table("ChatHistory_v")]
    public class ChatHistory_v
    {
        [Key]
        [Editable(false)]
        public int ID { get; set; }
        [Required]
        public DateTime dateIn { get; set; }
        [Required]
        [StringLength(64)]
        public string uId { get; set; }
        [StringLength(1024)]
        public string msg { get; set; }
        [Display(Name = "machineName")]
        [StringLength(300)]
        public string machineName { get; set; }
        [StringLength(64)]
        public string appIdent { get; set; }
    }
}