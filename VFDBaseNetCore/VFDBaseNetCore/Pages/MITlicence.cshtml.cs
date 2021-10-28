using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using MTF.Areas.Identity.Data;

namespace MTF.Pages
{
    [AllowAnonymous]
    public class MITlicenceModel : PageModel
    {
        public MITlicenceModel()
        {
        }

        public void OnGet ()
        {      
        }
    }
}
