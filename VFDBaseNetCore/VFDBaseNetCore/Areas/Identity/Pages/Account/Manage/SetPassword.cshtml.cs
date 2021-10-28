using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class SetPasswordModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;

        public SetPasswordModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            ILogger<SetPasswordModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Set Password")
        {
            _signInManager = signInManager;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }

            var hasPassword = await _userManager.HasPasswordAsync(_commonUser);

            if (hasPassword)
            {
                return RedirectToPage("./ChangePassword", new { id = id });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(_commonUser, Input.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    _statusMessage = "Error: ";
                    _statusMessage += error.Description + "; ";
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(_commonUser);
            _statusMessage = "Your password has been set.";

            return RedirectToPage(new { id = id });
        }
    }
}
