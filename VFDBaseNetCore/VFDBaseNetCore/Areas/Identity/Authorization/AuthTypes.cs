using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MTF.Areas.Identity.Authorization
{
    public static class AuthOperations
    {
        public static OperationAuthorizationRequirement NoRestrictions =
          new OperationAuthorizationRequirement { Name = AuthTypes.NoRestrictions };
        public static OperationAuthorizationRequirement CanBroadcast =
          new OperationAuthorizationRequirement { Name = AuthTypes.CanBroadcast };
        public static OperationAuthorizationRequirement CanManageThisAccount =
          new OperationAuthorizationRequirement { Name = AuthTypes.CanManageThisAccount };
        public static OperationAuthorizationRequirement IsTester =
          new OperationAuthorizationRequirement { Name = AuthTypes.IsTester };
    }
    public static class AuthTypes
    {
        public const string NoRestrictions = "NoRestrictions";
        public const string CanBroadcast = "CanBroadcast";
        public const string CanManageThisAccount = "CanManageThisAccount";
        public const string IsTester = "IsTester";
    }
}
