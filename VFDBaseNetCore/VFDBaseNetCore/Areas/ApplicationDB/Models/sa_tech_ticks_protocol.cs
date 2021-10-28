using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace MTF.Areas.ApplicationDB.Models
{
    [Table("sa_tech_ticks_protocol")]
    public class sa_tech_ticks_protocol
    {
        public enum sa_tech_tick_results
        {
            OK = 0
        }
        [Key]
        [Editable(false)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public sa_tech_tick_results _type { get; set; }
        [Required]
        [StringLength(127)]
        public string dscr { get; set; }
    }
}
