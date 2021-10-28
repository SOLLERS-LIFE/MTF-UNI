using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class ResetAuthenticatorModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;

        public ResetAuthenticatorModel(
            CommonIdent context,
            IAuthorizationService authorizationService,

            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            ILogger<ResetAuthenticatorModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Reset Authenticator")
        {
            _signInManager = signInManager;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }
        public bool isSU { get; set; }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public async Task<IActionResult> OnGet(string? id)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }
            isSU = await verifySU();

            return Page();
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public async Task<IActionResult> OnPostAsync(string? id)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }
            isSU = await verifySU();

            await _userManager.SetTwoFactorEnabledAsync(_commonUser, false);
            await _userManager.ResetAuthenticatorKeyAsync(_commonUser);
            _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", _commonUser.Id);

            await _signInManager.RefreshSignInAsync(_commonUser);
            _statusMessage = "Authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToPage("./EnableAuthenticator", new { id = id });
        }
    }
}