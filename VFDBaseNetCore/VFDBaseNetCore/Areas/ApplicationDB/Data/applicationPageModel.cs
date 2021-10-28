using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;

namespace MTF.Areas.ApplicationDB.Data
{
    public class applicationPageModel<T,L> : mtfPageModel<T, L> where T : class, new()
    {
        private AppDB_Context _appDBContext { get; set; }
        public AppDB_Context appdbcnt { get; set; }
        public applicationPageModel(
                                CommonIdent context,
                                IAuthorizationService authorizationService,
                                UserManager<CommonUser> userManager,
                                AppDB_Context appDBContext,
                                ILogger<L> logger,
                                IpSafeList ipSafeList,
                                string pageName = "")
               : base(context,
                     authorizationService,
                     userManager,
                     logger,
                     ipSafeList,
                     pageName)
        {
            _appDBContext = appDBContext;
        }
        public override async Task _prologue(bool _init = false)
        {
            await base._prologue(_init);
            // get user-based context from factory
            appdbcnt = (AppDB_Context)(await GlobalParameters._UAStore.getAESElemValue(_userRef.Id,
                                                                                        new
                                                                                        {
                                                                                            userManager = _userManager,
                                                                                            appDBContext = _appDBContext
                                                                                        }));
        }
        public override async Task _epilogue()
        {
            await GlobalParameters._UAStore.ReleaseAESElemValue(_userRef.Id, appdbcnt);
            await base._epilogue();
        }
        public override async Task _epilogue_catcher(Exception ex, string errPrefix = "Error")
        {
            //await GlobalParameters._UAStore.ReleaseAESElemValue(_userRef.Id, appdbcnt);
            await base._epilogue_catcher(ex, errPrefix);
        }
        public async Task<string> user_config_get (string cnfName, string cnfDefault)
        {
            if (appdbcnt == null)
            {
                throw new Exception("_prologue was not called.");
            }

            var rc = await appdbcnt.getScalarValue("call sa_user_config_get", cnfName, cnfDefault);

            return rc.RetValueString;
        }
        public async Task<string> user_config_set(string cnfName, string cnfValue)
        {
            if (appdbcnt == null)
            {
                throw new Exception("_prologue was not called.");
            }

            var rc = await appdbcnt.getScalarValue("call sa_user_config_set", cnfName, cnfValue);

            return rc.RetValueString;
        }
        public async Task<string> team_config_get(string cnfName)
        {
            if (appdbcnt == null)
            {
                throw new Exception("_prologue was not called.");
            }

            var rc = await appdbcnt.getScalarValue("call bs_team_config_currentuser_get", cnfName);

            return rc.RetValueString;
        }
    }
}
