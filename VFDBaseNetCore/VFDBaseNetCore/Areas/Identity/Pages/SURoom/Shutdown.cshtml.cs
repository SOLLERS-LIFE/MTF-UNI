using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;
using MTF.Services;

namespace MTF.Areas.Identity.Pages.SURoom
{
    [Authorize]
    public class ShutdownModel : Ext_AuthPageModel
    {
        private TechTickle _tt { get; set; }
        public ShutdownModel(CommonIdent context,
                             IAuthorizationService authorizationService,
                             UserManager<CommonUser> userManager,
                             ILogger<ShutdownModel> logger,
                             IpSafeList ipSafeList
                            )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Restart"
                  )
        {
            _tt = GlobalParameters._TechTickle;
        }

        // Idea to keep parameters for POST routine here
        public class InputParameters
        {
            [Required]
            [Display(Name = "Restart Host After (mins):")]
            public int tOut { get; set; } = 5;
            [Required]
            [DataType(DataType.MultilineText)]
            [StringLength(255)]
            [Display(Name = "Broadcast Message:")]
            public string broadcastMsg { get; set; } = $"Host Restart initiated. Please finish your work asap.";
        }
        [BindProperty]
        public InputParameters input { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                CommonUser currentUser = await verifySUwr();
                if (currentUser == null || !IsSourceIPSafe())
                {
                    return NotFound();
                }

                input = new InputParameters();
                
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return await Task.FromResult(RedirectToPage("./Shutdown",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                CommonUser currentUser = await verifySUwr();
                if (currentUser == null || !IsSourceIPSafe())
                {
                    return NotFound();
                }

                await _tt.RequireShutdown(input.tOut, input.broadcastMsg,MainRetCodes.Restart);
                _statusMessage = $"Restart requested in {input.tOut} min(s).";

                return await Task.FromResult(RedirectToPage("./Shutdown",
                                                            new { fromPOST = "Yes" }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return await Task.FromResult(RedirectToPage("./Shutdown",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes"
                                                            }
                                                           )
                                            );
            }
        }
    }
}