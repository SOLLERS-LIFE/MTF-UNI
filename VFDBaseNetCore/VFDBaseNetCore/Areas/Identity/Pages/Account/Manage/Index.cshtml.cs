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
    public partial class IndexModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;

        public IndexModel(
            CommonIdent context,
            IAuthorizationService authorizationService,

            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            ILogger<IndexModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Account Management")
        {
            //_userManager = userManager;
            _signInManager = signInManager;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }

        public string Username { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full name")]
            public string FullName { get; set; }

            [StringLength(64)]
            [Display(Name = "To be addressed as")]
            public string toBeAddressed { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(64)]
            [Display(Name = "Colour Model")]
            public string colorModel { get; set; }

            [Required]
            [StringLength(32)]
            [Display(Name = "Command Bars Colour")]
            public string barsColour { get; set; }
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

            await LoadAsync(_commonUser);

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

            if (!ModelState.IsValid)
            {
                await LoadAsync(_commonUser);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(_commonUser);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(_commonUser, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(_commonUser);
                    throw new InvalidOperationException($"Unexpected error occurred setting phone number for user with ID '{userId}'.");
                }
            }

            if (Input.FullName != _commonUser.FullName)
            {
                _commonUser.FullName = Input.FullName;
            }
            if (Input.toBeAddressed != _commonUser.toBeAddressed)
            {
                _commonUser.toBeAddressed = Input.toBeAddressed;
            }
            if (String.IsNullOrEmpty(_commonUser.toBeAddressed))
            {
                _commonUser.toBeAddressed = _commonUser.FullName;
            }
            if (Input.colorModel != _commonUser.colorModel)
            {
                _commonUser.colorModel = Input.colorModel;
            }
            if (Input.barsColour != _commonUser.barsColour)
            {
                _commonUser.barsColour = Input.barsColour;
            }
            await _userManager.UpdateAsync(_commonUser);

            if (_commonUser.Id == (await _userManager.GetUserAsync(User)).Id)
            {
                await _signInManager.RefreshSignInAsync(_commonUser);
            }
            _statusMessage = "Profile has been updated";
            return RedirectToPage("./Index",new { id = _commonUser.Id });
        }
        
        private async Task LoadAsync(CommonUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FullName = user.FullName,
                toBeAddressed = user.toBeAddressed,
                PhoneNumber = phoneNumber,
                colorModel = user.colorModel,
                barsColour = user.barsColour
            };
        }
    }
}
