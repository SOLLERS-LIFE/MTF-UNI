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
    public class markerViewInvariants
    {
        public string searchString { get; set; }
    }
    public class markerViewPostOp : post_op
    {
        public string _prm3 { get; set; }
        public string _prm4 { get; set; }
    }
    [Authorize]
    public class MarkerViewModel : applicationPageModel<markerViewInvariants, MarkerViewModel>
    {
        public MarkerViewModel(CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 AppDB_Context appDBContext,
                                 IpSafeList ipSafeList,
                                 ILogger<MarkerViewModel> logger
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "About " + GlobalParameters.MarkNameAlias
                  )
        {
        }

        public bs_marks_v _marker { get; set; }
        public List<bs_team_users_v> _teamUsersWithAccess { get; set; }
        public List<bs_team_users_v> _teamUsersDisallowed { get; set; }

        [BindProperty]
        public markerViewPostOp _operation { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST,
                                                    int id)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    _marker = new bs_marks_v();
                    _teamUsersWithAccess = new List<bs_team_users_v>();
                    _teamUsersDisallowed = new List<bs_team_users_v>();
                    return await Task.FromResult(Page());
                }
                await _prologue();

                _marker = (await appdbcnt._bs_marks_v.FromSqlRaw("call bt_marks_v_get_byid ({0});",
                                                                   id)
                                                                .AsNoTracking()
                                                                .ToListAsync()).FirstOrDefault();
                _teamUsersWithAccess = (await appdbcnt._bs_team_users_v.FromSqlRaw("call bt_team_users_marker_allowed ({0},{1});",
                                                                                   _marker.team_name,
                                                                                   id)
                                                                .AsNoTracking()
                                                                .ToListAsync());
                _teamUsersDisallowed = (await appdbcnt._bs_team_users_v.FromSqlRaw("call bt_team_users_marker_disallowed ({0},{1});",
                                                                                   _marker.team_name,
                                                                                   id)
                                                                .AsNoTracking()
                                                                .ToListAsync());

                await _epilogue();
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./MarkerView",
                                                            new { haveError = "Yes",
                                                                id = id
                                                            }
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
                    case "include":
                        rc = await appdbcnt.getScalarValue("call bm_mark_user_allow", _operation._prm, _operation._prm2, _operation._prm3, _operation._prm4);
                        break;
                    case "exclude":
                        rc = await appdbcnt.getScalarValue("call bm_mark_user_disallow", _operation._prm, _operation._prm2, _operation._prm3, _operation._prm4);
                        break;
                    default:
                        throw new Exception("MarkerView - Unknown operation.");
                };

                await _epilogue();
                return await Task.FromResult(RedirectToPage("./MarkerView",
                                                            new { fromPOST = "Yes",
                                                                id = _operation._src
                                                            }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./MarkerView",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes",
                                                                id = _operation._src
                                                            }
                                                           )
                                            );
            }
        }
    }
}