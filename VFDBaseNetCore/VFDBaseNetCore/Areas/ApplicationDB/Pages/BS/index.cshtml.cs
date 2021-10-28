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

using Microsoft.EntityFrameworkCore;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.ApplicationDB.Data;
using MTF.Utilities;
using MTF.Areas.ApplicationDB.Pages.BS.Models;

namespace MTF.Areas.ApplicationDB.BS
{
    // Idea to have TempData here permanent to this type of views
    public class bsInvariants
    {
    }
    [Authorize]
    public class BSindexModel : applicationPageModel<bsInvariants, BSindexModel>
    {
       public BSindexModel(CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 AppDB_Context appDBContext,
                                 IpSafeList ipSafeList,
                                 ILogger<BSindexModel> logger
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   MTF.GlobalParameters.BusinessStructureAlias
                  )
        {
            _pageParams = new PageParams();
        }

        // idea to keep here all parameters for page html generation
        public class PageParams
        {   
            public int teams_number { get; set; }
            public int teams_members_number { get; set; }
            public int teams_invitations_number { get; set; }

        }
        public PageParams _pageParams { get; set; }

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

                DbContextWithScalarReturn.ScalarReturn rc;
                rc = await appdbcnt.getScalarValue("call bs_teams_number");
                _pageParams.teams_number = rc.RetValueInt;
                rc = await appdbcnt.getScalarValue("call bs_teams_members_number");
                _pageParams.teams_members_number = rc.RetValueInt;
                rc = await appdbcnt.getScalarValue("call bs_teams_invitations_number");
                _pageParams.teams_invitations_number = rc.RetValueInt;

                await _epilogue();
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./index",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await _prologue();

                await _epilogue();
                return await Task.FromResult(RedirectToPage("./index",
                                                            new { fromPOST = "Yes" }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./index",
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