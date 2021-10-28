using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MTF.Areas.Logging.Models
{
    [Table("GeneralLog")]
    public class GeneralLog
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "machineName")]
        [StringLength(300)]
        public string machineName { get; set; }
        [Display(Name = "appIdent")]
        [StringLength(300)]
        public string appIdent { get; set; }
        [Required]
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Required]
        [Display(Name = "level")]
        [StringLength(15)]
        public string _level { get; set; }
        [Required]
        [Display(Name = "message")]
        [StringLength(1024)]
        public string message { get; set; }
        [Display(Name = "logger")]
        [StringLength(300)]
        public string logger { get; set; }
        [Display(Name = "properties")]
        [StringLength(1024)]
        public string properties { get; set; }
        [Display(Name = "callsite")]
        [StringLength(300)]
        public string callsite { get; set; }
        [Display(Name = "exception")]
        [StringLength(4096)]
        public string _exception { get; set; }
        [Display(Name = "url")]
        [StringLength(1024)]
        public string url { get; set; }
        [Display(Name = "reqhost")]
        [StringLength(1024)]
        public string reqhost { get; set; }
        [Display(Name = "uId")]
        [StringLength(300)]
        public string uId { get; set; }
    }

    [Table("GeneralLogCatched")]
    public class GeneralLogCatched
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "actorId")]
        [StringLength(50)]
        public string actorId { get; set; }
        [Display(Name = "machineName")]
        [StringLength(300)]
        public string machineName { get; set; }
        [Display(Name = "appIdent")]
        [StringLength(300)]
        public string appIdent { get; set; }
        [Required]
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Required]
        [Display(Name = "level")]
        [StringLength(15)]
        public string _level { get; set; }
        [Required]
        [Display(Name = "message")]
        [StringLength(1024)]
        public string message { get; set; }
        [Display(Name = "logger")]
        [StringLength(300)]
        public string logger { get; set; }
        [Display(Name = "properties")]
        [StringLength(1024)]
        public string properties { get; set; }
        [Display(Name = "callsite")]
        [StringLength(300)]
        public string callsite { get; set; }
        [Display(Name = "exception")]
        [StringLength(4096)]
        public string _exception { get; set; }
        [Display(Name = "url")]
        [StringLength(1024)]
        public string url { get; set; }
        [Display(Name = "reqhost")]
        [StringLength(1024)]
        public string reqhost { get; set; }
        [Display(Name = "uId")]
        [StringLength(300)]
        public string uId { get; set; }
        [Display(Name = "prevID")]
        public int prevID { get; set; }
    }

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

    [Table("combinedFilters")]
    public class combinedFilters
    {
        [Required]
        [StringLength(50)]
        public string uid { get; set; }
        [Required]
        [StringLength(12)]
        public string subsys { get; set; }
        [Required]
        [StringLength(500)]
        public string filter { get; set; }
    }

    [Table("GeneralLog_v")]
    public class GeneralLog_v
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "machineName")]
        [StringLength(300)]
        public string machineName { get; set; }
        [Display(Name = "appIdent")]
        [StringLength(300)]
        public string appIdent { get; set; }
        [Required]
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Required]
        [Display(Name = "level")]
        [StringLength(5)]
        public string _level { get; set; }
        [Required]
        [Display(Name = "message")]
        [StringLength(1024)]
        public string message { get; set; }
        [Display(Name = "logger")]
        [StringLength(300)]
        public string logger { get; set; }
        [Display(Name = "properties")]
        [StringLength(1024)]
        public string properties { get; set; }
        [Display(Name = "callsite")]
        [StringLength(300)]
        public string callsite { get; set; }
        [Display(Name = "exception")]
        [StringLength(4096)]
        public string _exception { get; set; }
        [Display(Name = "url")]
        [StringLength(1024)]
        public string url { get; set; }
        [Display(Name = "reqhost")]
        [StringLength(1024)]
        public string reqhost { get; set; }
        [Display(Name = "uId")]
        [StringLength(300)]
        public string uId { get; set; }
        [Display(Name = "prevID")]
        public int prevID { get; set; }
        [Display(Name = "numRecs")]
        public int numRecs { get; set; }
    }

    [Table("GeneralLogPermanent")]
    public class GeneralLogPermanent
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Display(Name = "appIdent")]
        [StringLength(300)]
        public string appIdent { get; set; }
        [Required]
        [Display(Name = "logged")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yy HH:mm:ss}")]
        public DateTime logged { get; set; }
        [Required]
        [Display(Name = "level")]
        [StringLength(15)]
        public string _level { get; set; }
        [Required]
        [Display(Name = "message")]
        [StringLength(1024)]
        public string message { get; set; }
    }
}
