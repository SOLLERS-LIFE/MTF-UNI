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
    [Table("bs_objects_protocol")]
    public class bs_objects_protocol
    {
        public enum ObjectsTypes
        {
            bs_invitation = 1
        }
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public ObjectsTypes _type { get; set; }
        [Required]
        [Column(TypeName = "int")]
        public int id_object { get; set; }
        [Required]
        [StringLength(64)]
        public string id_user { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Required]
        [StringLength(64)]
        public string action_dscr { get; set; }
    }
}
