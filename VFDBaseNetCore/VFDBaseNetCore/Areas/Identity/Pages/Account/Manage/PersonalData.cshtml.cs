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
    public class PersonalDataModel : Ext_AuthPageModel
    {
        public PersonalDataModel(
            CommonIdent context,
            IAuthorizationService authorizationService,

            UserManager<CommonUser> userManager,
            ILogger<PersonalDataModel> logger,
            IpSafeList ipSafeList)
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Personal Data")
        {
        }

        // Added to all predefined identity management models
        // to work with ANY account
        public CommonUser _commonUser { get; set; }

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

            return Page();
        }
    }
}