using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

using MTF.Areas.Identity.Data;
using MTF.Areas.ApplicationDB.Data;

namespace MTF.Areas.Identity.Authorization
{
    public class DealWithAccount_AuthorizationHandler
                 : AuthorizationHandler<OperationAuthorizationRequirement, object>
    {
        private UserManager<CommonUser> _userManager { get; set; }
        private AppDB_Context _appDBContext { get; set; }
        public DealWithAccount_AuthorizationHandler (UserManager<CommonUser> userManager,
                                                     AppDB_Context appDBContext)
        {
            _userManager = userManager;
            _appDBContext = appDBContext;
        }

        protected override Task
            HandleRequirementAsync (AuthorizationHandlerContext context,
                                    OperationAuthorizationRequirement requirement,
                                    object obj)
        {
            switch (obj.GetType().Name)
            {
                case "String":
                    string userId = (string)obj;
                    if (context.User == null || userId == null)
                    {
                        return Task.CompletedTask;
                    }

                    switch (requirement.Name)
                    {
                        case AuthTypes.NoRestrictions:
                            if (context.User.IsInRole(GlobalParameters._sus_Name))
                            {
                                context.Succeed(requirement);
                            }
                            break;
                        case AuthTypes.CanBroadcast:
                            if (
                                context.User.IsInRole(GlobalParameters._sus_Name)
                               )
                            {
                                context.Succeed(requirement);
                            }
                            break;
                        case AuthTypes.CanManageThisAccount:
                            var idAuth = _userManager.GetUserId(context.User);
                            if (idAuth == userId)
                            {
                                context.Succeed(requirement);
                            }
                            else if (
                                context.User.IsInRole(GlobalParameters._sus_Name)
                               )
                            {
                                context.Succeed(requirement);
                            }
                            else if (_appDBContext.getScalarValueSync("call bs_verify_authority_on", idAuth, userId).RetValueInt == -1)
                            {
                                context.Succeed(requirement);
                            }
                            break;
                        case AuthTypes.IsTester:
                            if (
                                context.User.IsInRole(GlobalParameters._testers_Name)
                               )
                            {
                                context.Succeed(requirement);
                            }
                            break;
                        default:
                            return Task.CompletedTask;
                    }
                    break;
                default:
                    throw new FormatException("Error: unexpected verification object type.");
            }
            

            return Task.CompletedTask;
        }
    }
}
