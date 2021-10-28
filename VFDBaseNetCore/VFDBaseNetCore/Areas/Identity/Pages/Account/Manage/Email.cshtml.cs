using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Authorization;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public partial class EmailModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            IEmailSender emailSender,
            ILogger<EmailModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Change EMail")
        {
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(CommonUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync(string id)
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

        public async Task<IActionResult> OnPostChangeEmailAsync(string id)
        {
            id ??= (await _userManager.GetUserAsync(User)).Id;

            _commonUser = await verifySUunder(id);
            if (_commonUser == null)
            {
                return NotFound();
            }
            var isSU = await verifySU();

            if (!ModelState.IsValid)
            {
                await LoadAsync(_commonUser);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(_commonUser);
            if (Input.NewEmail != email)
            {
                if (isSU)
                {
                    var token = await _userManager.GenerateChangeEmailTokenAsync(_commonUser, Input.NewEmail);
                    var res = await _userManager.ChangeEmailAsync(_commonUser, Input.NewEmail, token);
                    if (res.Succeeded)
                    {
                        var res1 = await _userManager.SetUserNameAsync(_commonUser, Input.NewEmail);
                        if (!res1.Succeeded)
                        {
                            _statusMessage = "E-mail is changed, User Name is NOT changed.";
                            return RedirectToPage(new { id = id});
                        }

                        _statusMessage = "Email and User Name are changed.";
                        return RedirectToPage(new { id = id });
                    }
                    else 
                    {
                        _statusMessage = "Error: ";
                        foreach (var error in res.Errors)
                        {
                            _statusMessage += error.Description + "; ";
                        }
                        
                        return RedirectToPage(new { id = id });
                    }
                }
                else
                {
                    var userId = await _userManager.GetUserIdAsync(_commonUser);
                    var code = await _userManager.GenerateChangeEmailTokenAsync(_commonUser, Input.NewEmail);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmailChange",
                        pageHandler: null,
                        values: new { userId = userId, email = Input.NewEmail, code = code },
                        protocol: Request.Scheme);
                    await _emailSender.SendEmailAsync(
                        Input.NewEmail,
                        "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    _statusMessage = "Confirmation link to change email sent. Please check your email.";
                    return RedirectToPage(new { id = id });
                }
            }

            _statusMessage = "Email is unchanged.";
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync(string id)
        {
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

            var userId = await _userManager.GetUserIdAsync(_commonUser);
            var email = await _userManager.GetEmailAsync(_commonUser);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(_commonUser);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            _statusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage(new { id = id });
        }
    }
}
