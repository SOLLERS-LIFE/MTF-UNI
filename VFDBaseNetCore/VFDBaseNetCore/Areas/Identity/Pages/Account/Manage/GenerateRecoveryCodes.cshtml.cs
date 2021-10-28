using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MTF.Areas.Identity.Data;

using MTF.Utilities;
using Microsoft.AspNetCore.Authorization;
using MTF.Areas.Identity.Authorization;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class GenerateRecoveryCodesModel : Ext_AuthPageModel
    {
        public GenerateRecoveryCodesModel(
            CommonIdent context,
            IAuthorizationService authorizationService,

            UserManager<CommonUser> userManager,
            ILogger<GenerateRecoveryCodesModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Generate Recovery Code")
        {
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }
        public bool isSU { get; set; }

        [TempData]
        public string[] RecoveryCodes { get; set; }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public async Task<IActionResult> OnGetAsync(string? id)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }
            isSU = await verifySU();

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(_commonUser);
            if (!isTwoFactorEnabled)
            {
                var userId = await _userManager.GetUserIdAsync(_commonUser);
                throw new InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' because they do not have 2FA enabled.");
            }

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

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(_commonUser);
            var userId = await _userManager.GetUserIdAsync(_commonUser);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException($"Cannot generate recovery codes for user with ID '{userId}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(_commonUser, 10);
            RecoveryCodes = recoveryCodes.ToArray();

            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
            _statusMessage = "You have generated new recovery codes.";
            return RedirectToPage("./ShowRecoveryCodes",new { id = id });
        }
    }
}