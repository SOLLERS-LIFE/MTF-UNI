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
    public class EventLogEntryModel : Ext_AuthPageModel
    {
        protected LogDB_Context _logDB { get; set; }
        public EventLogEntryModel(CommonIdent context,
                                  IAuthorizationService authorizationService,
                                  UserManager<CommonUser> userManager,
                                  LogDB_Context logDB,
                                  ILogger<EventLogEntryModel> logger,
                                  IpSafeList ipSafeList
                                 )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Log Entry"
                  )
        {
            _logDB = logDB;
        }
        
        public GeneralLog_v input { get; set; }

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
                    input = new GeneralLog_v();
                    return await Task.FromResult(Page());
                }

                input = (await _logDB.generalLog_v
                              .FromSqlRaw("call WTFlogs.eventsLogCache_GetEntry ({0},{1});",
                                          recId, currentUser.Id)
                              .AsNoTracking()
                              .ToListAsync())
                              .First();
                if (input == null)
                {
                    throw new Exception ("Unexpected event log record identifier");
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

                return await Task.FromResult(RedirectToPage("./EventLogEntry",
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
                
                return await Task.FromResult(RedirectToPage("./EventLogEntry",
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

                return await Task.FromResult(RedirectToPage("./EventLogEntry",
                                                            new { fromPOST = "Yes",
                                                                  haveError = "Yes" }
                                                           )
                                            );
            }
        }
    }
}