using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using MTF.Utilities;
using MTF.Areas.Identity.Data;

namespace MTF.Areas.Identity.Authorization
{
    // new base class for page's models
    // Of cause we should replace with it standart PageModel ancestor in all page models.
    // moreother we should add the constructors to that classes just to pass required
    // parameters to Ext_AuthPageModel constructor
    public partial class Ext_AuthPageModel : PageModel
    {
        public CommonIdent _context { get; }
        public IAuthorizationService _authorizationService { get; }
        public UserManager<CommonUser> _userManager { get; }
        public string PageName { get; }
        private readonly IpSafeList _ipSafeList;
        public ILogger _logger { get; set; }
        public bool _fromSafeIP { get; set; }

        [TempData]
        public string _statusMessage { get; set; }

        public Ext_AuthPageModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            ILogger logger,
            IpSafeList ipSafeList,
            string pageName = "")
            : base()
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
            PageName = pageName;
            _ipSafeList = ipSafeList;
            _logger = logger;
        }

        public bool IsSourceIPSafe()
        {
            if (_ipSafeList.Enabled)
            {
                var remoteIp = HttpContext.Connection.RemoteIpAddress;
                if (remoteIp == null)
                {
                    throw new ArgumentException("Remote IP is NULL, possibly due to missing ForwardedHeaders.");
                }

                if (!IsFromSafeList(_ipSafeList, remoteIp))
                {
                    _logger.LogWarning($"Request from unsafe IP: {remoteIp}");
                    return false;
                }
                else
                {
                    _logger.LogDebug($"Request from safe IP: {remoteIp}");
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private static bool IsFromSafeList(IpSafeList safeList,
                                           IPAddress remoteIp
                                           )
        {
            if (!safeList.Enabled)
            {
                return true;
            }
            List<IPAddress> ipAddresses = safeList.IpAddresses.Split(';').Select(IPAddress.Parse).ToList();
            List<IPNetwork> ipNetworks = safeList.IpNetworks.Split(';').Select(IPNetwork.Parse).ToList();

            if (remoteIp == null)
            {
                throw new ArgumentException("Remote IP is NULL, may due to missing ForwardedHeaders.");
            }

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            if (!ipAddresses.Contains(remoteIp) && !ipNetworks.Any(x => x.Contains(remoteIp)))
            {
                return false;
            }

            return true;
        }
        // Just verify a required permission for current context Claims Principial
        protected async Task<CommonUser> verifyPermission(IAuthorizationRequirement req)
        {
            var usr = await _userManager.GetUserAsync(User);
            if (usr == null)
            {
                return null;
            }
            if (
                (await _authorizationService.AuthorizeAsync(User,
                                                            usr.Id,
                                                            req
                                                           )
                ).Succeeded)
            {
                return usr;
            }
            return null;
        }
        // Verify if current context has SU permissions UNDER the user with Id id
        // i.e. current logged in user can act with user with Id=id as SU 
        // returns reference to CommonUser with Id=id if current context has
        // SU permitions
        protected async Task<CommonUser> verifySUunder(string id)
        {
            CommonUser _res = null;

            var usr = await _userManager.GetUserAsync(User);
            if (usr == null)
            {
                return null;
            }

            if (id != null)
            {
                if (
                    !(await _authorizationService.AuthorizeAsync(User,
                                                                 id,
                                                                 AuthOperations.CanManageThisAccount
                                                                ))
                        .Succeeded
                    )
                { return null; }

                _res = await _userManager.FindByIdAsync(id); ;
            }
            else
            {
                _res = usr;
            }

            return _res;
        }
        // Verify if some user is SU
        public async Task<bool> IsSU(CommonUser _usr)
        {
            return await _userManager.IsInRoleAsync(_usr, "SUS");
        }
        // Verify if current context has SU permissions in general
        protected async Task<bool> verifySU()
        {
            var usr = await _userManager.GetUserAsync(User);
            if (usr == null)
            {
                return false;
            }

            return (await _authorizationService.AuthorizeAsync(User,
                                                               usr.Id,
                                                               AuthOperations.NoRestrictions
                                                              ))
                         .Succeeded;
        }
        // The same, but returns current user id or null
        protected async Task<CommonUser> verifySUwr()
        {
            var usr = await _userManager.GetUserAsync(User);
            if (usr == null)
            {
                return null;
            }

            if(
                (await _authorizationService.AuthorizeAsync(User,
                                                       usr.Id,
                                                       AuthOperations.NoRestrictions
                                                       )
                ).Succeeded)
            {
                return usr;
            }

            return null;
        }
        // Verify if current context has SU permissions in general
        protected async Task<bool> IsTester()
        {
            var id = _userManager.GetUserId(User);
            if (id == null)
            {
                return await Task.FromResult(false);
            }

            return (await _authorizationService.AuthorizeAsync(User,
                                                               id,
                                                               AuthOperations.IsTester
                                                              ))
                         .Succeeded;
        }
    }
}
