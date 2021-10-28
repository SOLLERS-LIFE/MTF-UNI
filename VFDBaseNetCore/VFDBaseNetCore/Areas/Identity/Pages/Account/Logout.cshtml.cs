using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MTF.Areas.Identity.Data;



namespace MTF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<CommonUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly UserManager<CommonUser> _userManager;
        
        public LogoutModel(SignInManager<CommonUser> signInManager, 
                           ILogger<LogoutModel> logger,
                           UserManager<CommonUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync (string returnUrl)
        {
            if (_signInManager.IsSignedIn(User))
            {
                await GlobalParameters._UAStore.EntryComprimizedHard(_userManager.GetUserId(User));
                await _signInManager.SignOutAsync();
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync (string returnUrl)
        {
            if (_signInManager.IsSignedIn(User))
            {
                await GlobalParameters._UAStore.EntryComprimizedHard(_userManager.GetUserId(User));
                await _signInManager.SignOutAsync();
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
