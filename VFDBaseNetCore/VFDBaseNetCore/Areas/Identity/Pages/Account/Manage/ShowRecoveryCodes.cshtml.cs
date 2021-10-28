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
    public class ShowRecoveryCodesModel : Ext_AuthPageModel
    {
        public ShowRecoveryCodesModel (CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            ILogger<ChangePasswordModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Show Recovery Codes")
        { }
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

            if (RecoveryCodes == null || RecoveryCodes.Length == 0)
            {
                return RedirectToPage("./TwoFactorAuthentication", new { id = id });
            }

            return Page();
        }
    }
}
