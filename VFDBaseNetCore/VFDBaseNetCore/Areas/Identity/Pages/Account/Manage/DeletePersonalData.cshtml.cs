using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.CommonDB.Data;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class DeletePersonalDataModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;
        private readonly CommonDB_Context _commonDB_Context;

        public DeletePersonalDataModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            CommonDB_Context commonDB_Context,
            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Delete Personal Data")
        {
            _signInManager = signInManager;
            _commonDB_Context = commonDB_Context;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

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

            if (await verifySU())
            {
                RequirePassword = false;
            }
            else
            {
                RequirePassword = await _userManager.HasPasswordAsync(_commonUser);
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
            var isSU = await verifySU();

            if (isSU)
            {
                RequirePassword = false;
            }
            else
            {
                RequirePassword = await _userManager.HasPasswordAsync(_commonUser);
            }
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(_commonUser, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            var cnf = _context.configCommon.Find(1);
            if (cnf.SUUID == _commonUser.Id)
            {
                ModelState.AddModelError(string.Empty, "SU cannot be deleted");
                return Page();
            }

            var result = await _userManager.DeleteAsync(_commonUser);
            var userId = await _userManager.GetUserIdAsync(_commonUser);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' deleted.", userId);

            if (isSU)
            {
                return RedirectToPage("/SURoom/CommonUsersList",
                                      new { area="Identity" } 
                                     );
            }
            else
            {
                await _signInManager.SignOutAsync();
                return Redirect("~/");
            }
        }
    }
}
