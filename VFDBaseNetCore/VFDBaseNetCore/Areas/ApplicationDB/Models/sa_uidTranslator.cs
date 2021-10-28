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
    [Table("sa_uidTranslator")]
    public class sa_uidTranslator
    {
        [Key]
        [StringLength(64)]
        [Column(TypeName = "VARCHAR(64)")]
        public string id_user { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int uid { get; set; }
    }
}
