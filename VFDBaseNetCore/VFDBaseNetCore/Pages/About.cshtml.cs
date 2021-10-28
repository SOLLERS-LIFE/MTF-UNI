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

namespace MTF.Pages.About
{
    [AllowAnonymous]
    public class AboutModel : Ext_AuthPageModel
    { 
        public AboutModel(CommonIdent context,
                        IAuthorizationService authorizationService,
                        UserManager<CommonUser> userManager,
                        ILogger<AboutModel> logger,
                        IpSafeList ipSafeList
                    )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "About Us"
                  )
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_statusMessage}.");

                return await Task.FromResult(RedirectToPage("./About",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                return await Task.FromResult(RedirectToPage("./About",
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
                _logger.LogError($"{_statusMessage}.");

                return await Task.FromResult(RedirectToPage("./About",
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