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
    public class markersInvariants
    {
        public string searchString { get; set; }
    }
    [Authorize]
    public class MarkersModel : applicationPageModel<markersInvariants, MarkersModel>
    {
        public MarkersModel(CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 AppDB_Context appDBContext,
                                 IpSafeList ipSafeList,
                                 ILogger<MarkersModel> logger
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "My " + GlobalParameters.MarksNameAlias
                  )
        {
        }

        public List<bs_marks_v> MarksList { get; set; }

        [BindProperty]
        public post_op _operation { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    MarksList = new List<bs_marks_v>();
                    return await Task.FromResult(Page());
                }
                await _prologue();

                MarksList = (await appdbcnt._bs_marks_v.FromSqlRaw("call bm_marks_v_get_all ({0});",
                                                                   "")
                                                                .AsNoTracking()
                                                                .ToListAsync());

                await _epilogue();
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./Markers",
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
                    /*case "addMark":
                        rc = await appdbcnt.getScalarValue("call bm_mark_add", _operation._prm);
                        break;*/
                    case "deleteMark":
                        rc = await appdbcnt.getScalarValue("call bm_mark_delete", _operation._prm);
                        break;
                    case "renameMark":
                        rc = await appdbcnt.getScalarValue("call bm_mark_rename", _operation._prm, _operation._prm2);
                        break;
                    default:
                        throw new Exception("Marks - Unknown operation.");
                };

                await _epilogue();
                return await Task.FromResult(RedirectToPage("./Markers",
                                                            new { fromPOST = "Yes" }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("./Markers",
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