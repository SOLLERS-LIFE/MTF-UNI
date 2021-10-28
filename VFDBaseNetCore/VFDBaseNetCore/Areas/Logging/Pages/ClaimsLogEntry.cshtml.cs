using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.Logging.Data;
using MTF.Areas.Logging.Models;

namespace MTF.Areas.Logging.Pages
{
    [Authorize]
    public class ClaimsLogEntryOperation
    {
        public string _op { get; set; }
        public string _prm { get; set; }
        public string _id { get; set; }
    }
    public class ClaimsLogEntryModel : Ext_AuthPageModel
    {
        protected LogDB_Context _logDB { get; set; }
        public ClaimsLogEntryModel(CommonIdent context,
                                  IAuthorizationService authorizationService,
                                  UserManager<CommonUser> userManager,
                                  LogDB_Context logDB,
                                  ILogger<ClaimsLogEntryModel> logger,
                                  IpSafeList ipSafeList
                                 )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Claims Entry"
                  )
        {
            _logDB = logDB;
        }
        
        public ClaimsLog_v input { get; set; }

        // Idea to keep parameters for POST routine here
        [BindProperty]
        public ClaimsLogEntryOperation oper { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST,
                                                    int? recId)
        {
            try
            {
                CommonUser currentUser = await verifySUwr();
                if (currentUser == null)
                {
                    return NotFound();
                }

                if (haveError == "Yes")
                {
                    input = new ClaimsLog_v();
                    return await Task.FromResult(Page());
                }

                input = (await _logDB.claimsLog_v
                              .FromSqlRaw("call WTFlogs.claimsLogCache_GetEntry ({0},{1});",
                                          recId, currentUser.Id)
                              .AsNoTracking()
                              .ToListAsync())
                              .First();
                if (input == null)
                {
                    throw new Exception ("Unexpected claims log record identifier");
                }

                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_statusMessage}.");

                return await Task.FromResult(RedirectToPage("./ClaimsLogEntry",
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
                if (currentUser == null)
                {
                    return NotFound();
                }

                if (oper._op == "toggle")
                {
                    var rc = (await _logDB.getScalarValue("call claimsLogEntryReviewedToggle",
                                                           currentUser.Email,
                                                           oper._prm
                                                         )); 
                }

                return await Task.FromResult(RedirectToPage("./ClaimsLogEntry",
                                                            new { fromPOST = "Yes",
                                                                  recId = oper._id}
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

                return await Task.FromResult(RedirectToPage("./ClaimsLogEntry",
                                                            new { fromPOST = "Yes",
                                                                  haveError = "Yes" }
                                                           )
                                            );
            }
        }
    }
}