using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Pages.SURoom.Models;

namespace MTF.Areas.Identity.Pages.SURoom.Models
{
    [Table("CachedCommonUsers")]
    public partial class cachedCommonUser
    {
        public cachedCommonUser ()
        { }
        public cachedCommonUser (CommonUser cu, string actorId=null)
        {
            this.AccessFailedCount = cu.AccessFailedCount;
            this.Email = cu.Email;
            this.EmailConfirmed = cu.EmailConfirmed;
            this.FullName = cu.FullName;
            this.LockoutEnabled = cu.LockoutEnabled;
            this.LockoutEnd = cu.LockoutEnd;
            this.PhoneNumber = cu.PhoneNumber;
            this.PhoneNumberConfirmed = cu.PhoneNumberConfirmed;
            this.TwoFactorEnabled = cu.TwoFactorEnabled;
            this.UserName = cu.UserName;
            this.UserId = cu.Id;
            this.ActorId = actorId;
            this.toBeAddressed = cu.toBeAddressed;
        }

        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "ActorId")]
        public string ActorId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "UserId")]
        public string UserId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string UserName { get; set; }
        [StringLength(95)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [Required]
        [StringLength(95)]
        [Display(Name = "E-Mail")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "E-Mail Confirmed")]
        public bool EmailConfirmed { get; set; }
        [StringLength(50)]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }
        [Required]
        [Display(Name = "Ph/C")]
        [Editable(true)]
        public bool PhoneNumberConfirmed { get; set; }
        [Required]
        [Display(Name = "Two Factor Ident")]
        public bool TwoFactorEnabled { get; set; }
        [Display(Name = "Lck")]
        public DateTimeOffset? LockoutEnd { get; set; }
        [Required]
        [Display(Name = "Lck Enabled")]
        public bool LockoutEnabled { get; set; }
        [Display(Name = "Login Failed Count")]
        public int AccessFailedCount { get; set; }
        [Display(Name = "Sel")]
        [DefaultValue(false)]
        public bool Selected { get; set; }
        [StringLength(127)]
        [Display(Name = "To be addressed as")]
        public string toBeAddressed { get; set; }
    }
}

namespace MTF.Areas.CommonDB.Data
{
    public partial class CommonDB_Context
    {
        public DbSet<cachedCommonUser> cachedCommonUsers { get; set; }
    }
}
