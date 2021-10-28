using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
//using Pomelo.EntityFrameworkCore.MySql;


namespace MTF.Areas.CommonDB.Models
{
    [Table("ExchangeStore")]
    public class ExchangeStore
    {
        public int ID { get; set; }
        [Required]
        [StringLength(64)]
        public string UserUID { get; set; }
        [Required]
        [StringLength(1024)]
        public string ObjectDescr { get; set; }
    }
}
