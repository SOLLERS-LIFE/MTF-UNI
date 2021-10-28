using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;

namespace MTF.Pages
{
    [AllowAnonymous]
    [ServiceFilter(typeof(ClientIpCheckActionFilter))]
    public class IndexModel : Ext_AuthPageModel
    {
        public class pageParameters
        {
            public string uid { get; set; }
            public string warnedAboutCookies { get; set; }
            public DateTime birnDate { get; set; }
        }
        public pageParameters pageParams { get; set; }
        public CommonUser currentUser { get; set; }
        public string _announcement { get; set; }

        public IndexModel(CommonIdent context,
                          IAuthorizationService authorizationService,
                          UserManager<CommonUser> userManager,
                          ILogger<IndexModel> logger,
                          IpSafeList ipSafeList
                         )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Home"
                  )
        {
        }

        public async Task<IActionResult> OnGetAsync(string ChangeByPost
                                                   )
        {
            currentUser = await _userManager.GetUserAsync(User);

            pageParams = TempData.Get<pageParameters>("pageParams-Index");
            ifWarnedAboutCookies();
            TempData.Keep("pageParams-Index");

            // TO DO

            TempData.Put("pageParams-Index", pageParams);
            TempData.Keep("pageParams-Index");

            return await Task.FromResult(Page());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await Task.FromResult(RedirectToPage("./Index",
                                                         new { ChangeByPost = "" }
                                                         )
                                         );
        }

        private void ifWarnedAboutCookies ()
        {
            if (pageParams == null)
            {
                pageParams = new pageParameters();
                pageParams.uid = (currentUser != null) ? currentUser.Id : null;
                pageParams.birnDate = DateTime.Now;
                pageParams.warnedAboutCookies = "No";
            }
            else if (pageParams.uid != ((currentUser != null) ? currentUser.Id : null))
            {
                pageParams.uid = (currentUser != null) ? currentUser.Id : null;
                pageParams.warnedAboutCookies = (currentUser != null) ? "Yes" : "No";
            }
            else
            {
                pageParams.birnDate = (pageParams.birnDate == DateTime.MinValue) ? DateTime.Now : pageParams.birnDate;
                if (DateTime.Now.Subtract(pageParams.birnDate) > new TimeSpan(1, 0, 0))
                {
                    pageParams.birnDate = DateTime.Now;
                    pageParams.warnedAboutCookies = "No";
                }
                else
                {
                    pageParams.warnedAboutCookies = "Yes";
                }
            }
        }
    }
}