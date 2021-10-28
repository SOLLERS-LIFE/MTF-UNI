// VFD Class to extend standart Identity
// VFD all occurances of standard IdentityUser are replaced by this class
using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MTF.Areas.Identity.Data
{
    public class CommonUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; }
        [StringLength(127)]
        [Display(Name = "To be addressed as")]
        public string toBeAddressed { get; set; }
        [StringLength(127)]
        [DefaultValue("stormundnatiskk")]
        [Display(Name = "AppPswd")]
        public string AppPswd { get; set; }
        [StringLength(64)]
        [DefaultValue("dark.")]
        [Display(Name = "Color Model")]
        public string colorModel { get; set; }
        [StringLength(32)]
        [DefaultValue("dark.")]
        [Display(Name = "Command Bars Colour")]
        public string barsColour { get; set; }
    }
}
