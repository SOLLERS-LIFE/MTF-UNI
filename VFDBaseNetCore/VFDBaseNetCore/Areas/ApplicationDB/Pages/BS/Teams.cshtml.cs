using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

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
    public class teamsInvariants
    {
        public string searchString { get; set; }
    }
    [Authorize]
    public class TeamsModel : applicationPageModel<teamsInvariants, TeamsModel>
    {
        public List<SelectListItem> AvailableRoles { get; set; }
        public TeamsModel(CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 AppDB_Context appDBContext,
                                 IpSafeList ipSafeList,
                                 ILogger<TeamsModel> logger
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "My "+ GlobalParameters.TeamsNameAlias
                  )
        {
            AvailableRoles =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "0", Text = "ordinal"},
                                                 new SelectListItem { Value = "1", Text = "manager"},
                                                 new SelectListItem { Value = "2", Text = "banned"}
                                               }
                                        );
        }

        // idea to keep here all parameters for page html generation
        public List<bs_teams_v> TeamsList { get; set; }

        [BindProperty]
        public post_op _operation { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    TeamsList = new List<bs_teams_v>();
                    return await Task.FromResult(Page());
                }

                await _prologue();

                TeamsList = (await appdbcnt._bs_teams_v.FromSqlRaw("call bt_teams_v_get ({0},{1});",
                                                                            _invariants.searchString,
                                                                            _invariants.searchString)
                                                                .AsNoTracking()
                                                                .ToListAsync());


                await _epilogue();
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./Teams",
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

                DbContextWithScalarReturn.ScalarReturn rc;
                switch (_operation._op)
                {
                    case "addTeam":
                        rc = await appdbcnt.getScalarValue("call bs_team_add", _operation._prm);
                        break;
                    case "deleteTeam":
                        rc = await appdbcnt.getScalarValue("call bs_team_delete", _operation._prm);
                        break;
                    case "renameTeam":
                        rc = await appdbcnt.getScalarValue("call bs_team_rename", _operation._prm, _operation._prm2);
                        break;
                    default:
                        throw new Exception("Teams - Unknown operation.");
                };


                await _epilogue();
                return await Task.FromResult(RedirectToPage("./Teams",
                                                            new { fromPOST = "Yes" }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./Teams",
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