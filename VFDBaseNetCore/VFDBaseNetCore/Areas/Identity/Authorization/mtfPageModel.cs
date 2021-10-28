using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using MTF.Utilities;
using MTF.Areas.Identity.Data;

namespace MTF.Areas.Identity.Authorization
{
    public class mtfPageModel<T, L> : Ext_AuthPageModel where T : class, new()
    {
        public CommonUser _userRef { get; set; }
        // public ILogger<L> _logger { get; set; }
        public string permDataName { get; set; }
        public T _invariants { get; set; }

        public mtfPageModel(
                                CommonIdent context,
                                IAuthorizationService authorizationService,
                                UserManager<CommonUser> userManager,
                                ILogger<L> logger,
                                IpSafeList ipSafeList,
                                string pageName = "")
               : base(context, authorizationService, userManager, logger, ipSafeList, pageName)
        {
            permDataName = "Prmt-" + Regex.Replace(pageName, @"\s+", "");
        }

        public virtual async Task _prologue(bool _init = false)
        {
            _userRef = await _userManager.GetUserAsync(User);
            if (_userRef == null) { throw new InvalidOperationException($"Unauthenticated User."); };
            if (_init)
            {
                _invariants = new T();
            }
            else
            {
                _invariants = TempData.Peek<T>(permDataName) ?? new T();
            }
        }
        public virtual async Task _epilogue()
        {
            TempData.Put(permDataName, _invariants);
            await Task.CompletedTask;
        }
        public virtual async Task _epilogue_catcher(Exception ex, string errPrefix = "Error")
        {
            _statusMessage = errPrefix + ": "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
            _logger.LogError($"{_statusMessage}.");
            TempData.Put(permDataName, _invariants);

            await Task.CompletedTask;
        }
    }
}
