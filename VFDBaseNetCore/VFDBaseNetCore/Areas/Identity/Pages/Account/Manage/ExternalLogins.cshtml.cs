using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ExternalLoginsMdl : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;

        public ExternalLoginsMdl(
            CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            
            ILogger<ExternalLoginsMdl> logger,
            SignInManager<CommonUser> signInManager,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "External Logins")
        {
            _signInManager = signInManager;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }

        public IList<UserLoginInfo> CurrentLogins { get; set; }

        public IList<AuthenticationScheme> OtherLogins { get; set; }

        public bool ShowRemoveButton { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id); ;
            if (_commonUser == null)
            {
                return NotFound();
            }

            CurrentLogins = await _userManager.GetLoginsAsync(_commonUser);
            OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            ShowRemoveButton = _commonUser.PasswordHash != null || CurrentLogins.Count > 1;

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey, string id)
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id); ;
            if (_commonUser == null)
            {
                return NotFound();
            }

            var result = await _userManager.RemoveLoginAsync(_commonUser, loginProvider, providerKey);
            if (!result.Succeeded)
            {
                _statusMessage = "The external login was not removed.";
                return RedirectToPage(new { id = id });
            }

            if (_commonUser.Id == (await _userManager.GetUserAsync(User)).Id)
            {
                await _signInManager.RefreshSignInAsync(_commonUser);
            }
            _statusMessage = "The external login was removed.";
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostLinkLoginAsync(string provider, string id)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetLinkLoginCallbackAsync(string id)
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id); ;
            if (_commonUser == null)
            {
                return NotFound();
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(_commonUser));
            if (info == null)
            {
                throw new InvalidOperationException($"Unexpected error occurred loading external login info for user with ID '{_commonUser.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(_commonUser, info);
            if (!result.Succeeded)
            {
                _statusMessage = "The external login was not added. External logins can only be associated with one account.";
                return RedirectToPage(new { id = id });
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            _statusMessage = "The external login was added.";
            return RedirectToPage(new { id = id });
        }
    }
}
