using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MTF.Areas.Identity.Data
{
    [Table("CommonConfig")]
    public partial class CommonConfig
    {
        [Key]
        [Editable(false)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        [Display(Name = "SOLE version")]
        [StringLength(64)]
        public string VersionOfSOLE { get; set; }
        [Required]
        [Display(Name = "Updated")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = false)]
        public DateTime UpdateDate { get; set; }
        [Required]
        [Display(Name = "Deployed")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yy}", ApplyFormatInEditMode = false)]
        public DateTime DeployDate { get; set; }
        [Display(Name = "SUUID")] // super user Id
        [StringLength(64)]
        public string SUUID { get; set; }
        [Display(Name = "EVERYONEROLEID")] // EVERYONE role Id
        [StringLength(64)]
        public string EORID { get; set; }
        [Display(Name = "SUSROLEID")] // SUS rol Id
        [StringLength(64)]
        public string SUSRID { get; set; }
        [Display(Name = "TESTERSROLEID")] // SUS rol Id
        [StringLength(64)]
        public string TESTERSRID { get; set; }
    }
}