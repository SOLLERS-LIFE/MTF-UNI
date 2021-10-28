using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;
using MTF.Services;

namespace MTF.Areas.Identity.Pages.SURoom
{
    [Authorize]
    public class BroadcastModel : Ext_AuthPageModel
    {
        private TechTickle _tt { get; set; }

        public BroadcastModel(CommonIdent context,
                             IAuthorizationService authorizationService,
                             UserManager<CommonUser> userManager,
                             ILogger<BroadcastModel> logger,
                             IpSafeList ipSafeList
                            )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Broadcast"
                  )
        {
            _tt = GlobalParameters._TechTickle;
        }

        // Idea to keep parameters for POST routine here
        public class InputParameters
        {
            [Required]
            [Display(Name = "Send Broadcast after (mins):")]
            public int tOut { get; set; } = 0;
            [Required]
            [DataType(DataType.MultilineText)]
            [StringLength(255)]
            [Display(Name = "Broadcast Message:")]
            public string broadcastMsg { get; set; } = $"Hi Guys!\n";
        }
        [BindProperty]
        public InputParameters input { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                CommonUser currentUser = await verifyPermission(AuthOperations.CanBroadcast);
                if (currentUser == null)
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
                return await Task.FromResult(RedirectToPage("./Broadcast",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                CommonUser currentUser = await verifyPermission(AuthOperations.CanBroadcast);
                if (currentUser == null)
                {
                    return NotFound();
                }

                await _tt.RequireBroadcast(input.tOut, input.broadcastMsg);
                _statusMessage = $"Broadcast requested in {input.tOut} min(s).";

                return await Task.FromResult(RedirectToPage("./Broadcast",
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
                return await Task.FromResult(RedirectToPage("./Broadcast",
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