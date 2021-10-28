using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Pages.SURoom.Models;

namespace MTF.Areas.Identity.Pages.SURoom.Models
{
    public class announcements
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(16920)]
        [Column(TypeName = "TEXT")]
        public string content { get; set; }
        [Display(Name = "DateIn")]
        public DateTime DateIn { get; set; }
    }

    public class loginsdisablers
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(16920)]
        [Column(TypeName = "TEXT")]
        public string content { get; set; }
        [Display(Name = "DateIn")]
        public DateTime DateIn { get; set; }
    }
}

namespace MTF.Areas.CommonDB.Data
{
    public partial class CommonDB_Context
    {
        public DbSet<announcements> _announcements { get; set; }
        public DbSet<loginsdisablers> _loginsdisablers { get; set; }
    }
}
