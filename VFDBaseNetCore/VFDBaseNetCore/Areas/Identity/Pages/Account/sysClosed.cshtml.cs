using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MTF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class sysClosedModel : PageModel
    {
        
        public void OnGet()
        {
           
        }
    }
}
