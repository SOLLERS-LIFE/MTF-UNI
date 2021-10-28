using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class ChangePasswordModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;

        public ChangePasswordModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            ILogger<ChangePasswordModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Change Password")
        {
            _signInManager = signInManager;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }
        public bool isSU { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            //[Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

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

            var hasPassword = await _userManager.HasPasswordAsync(_commonUser);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword", new { id = id });
            }

            return Page();
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public async Task<IActionResult> OnPostAsync(string? id)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage(new { id = id }); ;
            }
            Input.OldPassword ??= "";

            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }
            isSU = await verifySU();

            IdentityResult changePasswordResult = new IdentityResult();
            if (!isSU)
            {
                changePasswordResult = await _userManager.ChangePasswordAsync(_commonUser, Input.OldPassword, Input.NewPassword);
            }
            else
            {
                string tk = await _userManager.GeneratePasswordResetTokenAsync(_commonUser);
                changePasswordResult = await _userManager.ResetPasswordAsync(_commonUser, tk, Input.NewPassword);
            }
            if (!changePasswordResult.Succeeded)
            {
                _statusMessage = "Error: ";
                foreach (var error in changePasswordResult.Errors)
                {
                    _statusMessage += error.Description + "; ";
                }
                return RedirectToPage(new { id = id });
            }

            if (!isSU)
            {
                await _signInManager.RefreshSignInAsync(_commonUser);
            }
            _logger.LogInformation("Password changed successfully.");
            _statusMessage = "Password has been changed.";

            return RedirectToPage(new { id = id});
        }
    }
}
