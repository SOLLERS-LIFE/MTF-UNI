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
    [Table("bs_security_disable")]
    public class bs_security_disable
    {
        public enum SecurityDisableReasons
        {
            bsInviteTrapSatisfaction = 1
        }
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(64)]
        public string id_user { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public SecurityDisableReasons _state { get; set; }
    }
}
