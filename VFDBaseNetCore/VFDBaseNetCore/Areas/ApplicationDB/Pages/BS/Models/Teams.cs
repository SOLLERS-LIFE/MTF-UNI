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
    [Table("bs_teams")]
    public class bs_teams
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
    [Table("bs_teams_v")]
    public class bs_teams_v
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(127)]
        [Display(Name = "name")]
        public string name { get; set; }
        [Required]
        public int numRecs { get; set; }
    }

    [Table("bs_team_users")]
    public class bs_team_users
    {
        public enum roles { manager = 1, ordinal = 0, banned = 2 }
        
        [Required]
        public int id_bst { get; set; }
        [Required]
        [StringLength(64)]
        public string id_user { get; set; }
        [Column(TypeName = "tinyint")]
        public roles _role { get; set; }
    }

    [Table("bs_team_marks")]
    public class bs_team_marks
    {
        [Required]
        public int id_bst { get; set; }
        [Required]
        public int id_mark { get; set; }
    }

    [Table("bs_invite_traps")]
    public class bs_invite_traps
    {
        public enum states { active = 0, complited = 1, cancelled = 2, excluded = 3 }

        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int id_bst { get; set; }
        [Required]
        [StringLength(64)]
        public string user_email { get; set; }
        [Column(TypeName = "tinyint")]
        public states _state { get; set; }
        [Required]
        [StringLength(64)]
        public string change_id_user { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime change_logged { get; set; }
    }

    [Table("bs_team_users_v")]
    public class bs_team_users_v
    {
        public enum roles { manager = 1, ordinal = 0, banned = 2 }

        [Required]
        public int id_bst { get; set; }
        [Required]
        [StringLength(64)]
        public string id_user { get; set; }
        [Column(TypeName = "tinyint")]
        public roles _role { get; set; }
        [StringLength(64)]
        public string name { get; set; }
        [StringLength(64)]
        public string email { get; set; }
        [StringLength(64)]
        public string phone { get; set; }
        [Required]
        public int numRecs { get; set; }
    }

    [Table("bs_invite_traps_v")]
    public class bs_invite_traps_v
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int id_bst { get; set; }
        [Required]
        [StringLength(64)]
        public string user_email { get; set; }
        [Column(TypeName = "tinyint")]
        public bs_invite_traps.states _state { get; set; }
        [Required]
        [StringLength(64)]
        public string _stateVal { get; set; }
        [Required]
        [StringLength(64)]
        public string change_id_user { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime change_logged { get; set; }
    }

    [Table("bs_team_configs_avl")]
    [Index (nameof(cnfName), IsUnique = true)]
    public class bs_team_configs_avl
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(127)]
        [Display(Name = "cnfName")]
        public string cnfName { get; set; }
        [Required]
        [StringLength(127)]
        [Display(Name = "cnfDefault")]
        public string cnfDefault { get; set; }
    }

    [Table("bs_team_configs")]
    public class bs_team_configs
    {
        [Required]
        public int id_team { get; set; }
        [Required]
        public int id_cnf { get; set; }
        [Required]
        [StringLength(127)]
        public string cnfValue { get; set; }
    }

    // It is UPDATABLE view, thats why I use PK with it
    [Table("bs_team_configs_v")]
    public class bs_team_configs_v
    {
        [Required]
        public int id_team { get; set; }
        [Required]
        public int id_cnf { get; set; }
        [Required]
        [StringLength(127)]
        public string cnfName { get; set; }
        [Required]
        [StringLength(127)]
        public string teamName { get; set; }
        [Required]
        [StringLength(127)]
        public string cnfValue { get; set; }
    }
}

namespace MTF.Areas.ApplicationDB.Data
{
    public partial class AppDB_Context
    {
        // the trick to privide indexes definitions
        // from another parts of partial database context
        // Will be removed by compiler if not defined.
        partial void _defineEntities_ext03(ModelBuilder modelBuilder);
    }
    public partial class AppDB_Context
    {
        public DbSet<bs_teams> _bs_teams { get; set; }
        public DbSet<bs_teams_v> _bs_teams_v { get; set; }
        public DbSet<bs_team_users> _bs_team_users { get; set; }
        public DbSet<bs_invite_traps> _bs_invite_traps { get; set; }
        public DbSet<bs_team_users_v> _bs_team_users_v { get; set; }
        public DbSet<bs_invite_traps_v> _bs_invite_traps_v { get; set; }
        public DbSet<bs_team_marks> _bs_team_marks { get; set; }
        public DbSet<bs_team_configs_avl> _bs_team_configs_avl { get; set; }
        public DbSet<bs_team_configs> _bs_team_configs { get; set; }
        public DbSet<bs_team_configs_v> _bs_team_configs_v { get; set; }

        // indexes definitions from first part
        // of partial context
        partial void _defineEntities_ext02(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<bs_teams>()
                .HasIndex(f => f.name)
                
                .IsUnique();

            modelBuilder.Entity<bs_team_users>()
               .HasKey(f => new { f.id_bst, f.id_user });

            modelBuilder.Entity<bs_team_users>(entity =>
            {
                entity.Property(f => f._role)
                   .HasDefaultValue((bs_team_users.roles.ordinal));
            });

            modelBuilder.Entity<bs_invite_traps>()
                .HasIndex(f => new { f.user_email, f._state })
                
                .HasDatabaseName("IX_bs_invite_traps_fnd");

            modelBuilder.Entity<bs_invite_traps>(entity =>
            {
                entity.Property(f => f._state)
                   .HasDefaultValue((bs_invite_traps.states.active));
            });

            modelBuilder.Entity<bs_team_users_v>()
               .HasKey(f => new { f.id_bst, f.id_user });

            modelBuilder.Entity<bs_invite_traps_v>()
               .HasKey(f => new { f.Id });

            modelBuilder.Entity<bs_team_marks>()
               .HasKey(f => new { f.id_bst, f.id_mark });

            modelBuilder.Entity<bs_team_configs>()
               .HasKey(f => new { f.id_cnf, f.id_team });
            modelBuilder.Entity<bs_team_configs_v>()
               .HasKey(f => new { f.id_cnf, f.id_team });

            // Pattern to ignore view or stored procedures results
            // fictive entities during tables build
            if (!GlobalParameters.IsStartedWithMain)
            {
                modelBuilder.Ignore<bs_teams_v>();
                modelBuilder.Ignore<bs_team_users_v>();
                modelBuilder.Ignore<bs_invite_traps_v>();
                modelBuilder.Ignore<bs_team_configs_v>();
            }
            else
            {}
            // indexes definitions from second part
            // of partial context
            _defineEntities_ext03(modelBuilder);
        }
    }
}