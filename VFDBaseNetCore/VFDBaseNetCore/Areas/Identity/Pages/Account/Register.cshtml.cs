using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MTF.Areas.Identity.Data;
using MTF.Areas.CommonDB.Data;

// from nuget and https://github.com/michaelvs97/AspNetCore.ReCaptcha
using AspNetCore.ReCaptcha;

using MTF.Utilities;
using MTF.Areas.ApplicationDB.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Areas.Identity.Pages.Account
{
#if USERECAPCHA
    [ValidateReCaptcha]
#endif
    [AllowAnonymous]
    public class RegisterModel : Ext_AuthPageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;
        private readonly IEmailSender _emailSender;
        protected CommonDB_Context _commonDB_Context { get; set; }
        protected RoleManager<CommonRole> _roleManager { get; set; }
        protected AppDB_Context _appDBContext { get; set; }

        public RegisterModel(
            CommonIdent context,
            IAuthorizationService authorizationService,
            UserManager<CommonUser> userManager,
            SignInManager<CommonUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            CommonDB_Context commonDB_Context,
            AppDB_Context appDBContext,
            RoleManager<CommonRole> roleManager,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Register User")
        {
            _signInManager = signInManager;
            _emailSender = emailSender;
            _commonDB_Context = commonDB_Context;
            _roleManager = roleManager;
            _appDBContext = appDBContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            // VFD add field for Identity
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Full name")]
            public string FullName { get; set; }
            // VFD add field for Identity END

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var su_room_action = User.Identity.IsAuthenticated; // if not authenticated - then it's ordinal
                                                                // user registration, if definite account -
                                                                // it's new user operation from SU Room
            if (su_room_action)
            {
                // check SU permittions
                if (!await verifySU())
                {
                    // block SU Room operation
                    return NotFound();
                }
            }

            // common branch
            if (ModelState.IsValid)
            {
                var user = new CommonUser { 
                    // VFD add field for Identity
                    FullName = Input.FullName,
                    // VFD add field for Identity END
                    UserName = Input.Email,
                    Email = Input.Email,
                    // if we are inside SU Room - don't require e-mail confirmation
                    EmailConfirmed = su_room_action
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation(@$"New account created: {user.Email}, uId {user.Id}.");

                    // Add user to role 'EVERYONE'
                    var cnf = await _context.configCommon.FindAsync(1);
                    _ = await _userManager.AddToRoleAsync(user, (await _roleManager.FindByIdAsync(cnf.EORID)).Name);

                    var rc = await _appDBContext.getScalarValue("call bt_invitations_check", user.Email);

                    if (!su_room_action)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else 
                    {
                        return RedirectToPage("../SURoom/CommonUsersList");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
