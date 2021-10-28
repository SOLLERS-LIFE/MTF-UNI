using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;
using MTF.Areas.ApplicationDB.Data;
using MTF.Utilities;

namespace MTF.Pages
{
    // Idea to have TempData here permanent to this type of views
    public class ucInvariants
    {
    }
    [Authorize]
    public class ucModel : applicationPageModel<ucInvariants, ucModel>
    {
        public ucModel(CommonIdent context,
                       IAuthorizationService authorizationService,
                       UserManager<CommonUser> userManager,
                       AppDB_Context appDBContext,
                       IpSafeList ipSafeList,
                       ILogger<ucModel> logger
                      )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "Under Construction"
                  )
        {
        }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    return await Task.FromResult(Page());
                }

                await _prologue();

                await _epilogue();

                return await Task.FromResult(Page()); ;
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("/uc",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }
    }
}