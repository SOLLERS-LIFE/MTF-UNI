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
    public class cacheControl
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string uid { get; set; }
        [Required]
        [StringLength(50)]
        public string wfid { get; set; }
    }
}

namespace MTF.Areas.CommonDB.Data
{
    public partial class CommonDB_Context
    {
        public DbSet<cacheControl> cacheControls { get; set; }
    }
}
