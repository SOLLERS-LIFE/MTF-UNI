using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

using Microsoft.EntityFrameworkCore;




namespace MTF.Areas.Logging.Models
{
    [Table("ClaimsLog")]
    public class ClaimsLog
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "author")]
        [StringLength(50)]
        public string author { get; set; }
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm}")]
        public DateTime logged { get; set; }
        [Display(Name = "viewed")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm}")]
        public System.Nullable<DateTime> viewed { get; set; }
        [Display(Name = "reviewed by")]
        [StringLength(50)]
        public string viewedby { get; set; }
        [Display(Name = "claim text")]
        [StringLength(1024)]
        public string claimText { get; set; }
    }

    [Table("ClaimsLogCatched")]
    public class ClaimsLogCatched
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "actorId")]
        [StringLength(50)]
        public string actorId { get; set; }
        [Display(Name = "author")]
        [StringLength(50)]
        public string author { get; set; }
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Display(Name = "reviewed")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public System.Nullable<DateTime> viewed { get; set; }
        [Display(Name = "reviewed by")]
        [StringLength(50)]
        public string viewedby { get; set; }
        [Display(Name = "claim text")]
        [StringLength(1024)]
        public string claimText { get; set; }
        [Display(Name = "prevID")]
        public int prevID { get; set; }
        [Display(Name = "mrk")]
        [DefaultValue(false)]
        public bool marked { get; set; }
    }

    public class ClaimsLogcacheControl
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

    public class ClaimsLog_v
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "author")]
        [StringLength(50)]
        public string author { get; set; }
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm}")]
        public DateTime logged { get; set; }
        [Display(Name = "reviewed")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm}")]
        public System.Nullable<DateTime> viewed { get; set; }
        [Display(Name = "reviewed by")]
        [StringLength(50)]
        public string viewedby { get; set; }
        [Display(Name = "claim text")]
        [StringLength(1024)]
        public string claimText { get; set; }
        [Display(Name = "prevID")]
        public int prevID { get; set; }
        [Display(Name = "mrk")]
        [DefaultValue(false)]
        public bool marked { get; set; }
        [Display(Name = "numRecs")]
        public int numRecs { get; set; }
    }
}
