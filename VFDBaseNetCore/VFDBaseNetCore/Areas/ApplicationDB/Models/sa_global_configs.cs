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
    [Table("sa_global_configs")]
    public class sa_global_configs
    {
        [Key]
        [StringLength(15)]
        public string cnfName { get; set; }
        [Required]
        [StringLength(127)]
        public string cnfValue { get; set; }
    }
}