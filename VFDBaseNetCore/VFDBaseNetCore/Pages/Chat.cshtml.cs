using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities;

namespace MTF.Pages
{
    [Authorize]
    public class ChatModel : Ext_AuthPageModel
    {
        public ChatModel(CommonIdent context,
                                 IAuthorizationService authorizationService,
                                 UserManager<CommonUser> userManager,
                                 ILogger<ChatModel> logger,
                                 IpSafeList ipSafeList
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "Simple Chat"
                  )
        {
        }

        // Idea to have TempData here permanent to this type of views
        public class PermanentParameters
        {
            public string pp1 { get; set; }
        }
        public PermanentParameters permanent { get; set; }

        // Idea to keep parameters for POST routine here
        public class InputParameters
        {
            public string ip1 { get; set; }
        }
        [BindProperty]
        public InputParameters input { get; set; }

        // idea to keep here all parameters for page html generation
        public class PageParameters
        {
            public string pp1 { get; set; }
        }
        public PageParameters pageParams { get; set; }


        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST)
        {
            try
            {
                permanent = TempData.Get<PermanentParameters>("permanent-VFDTemplate")?? new PermanentParameters();
                TempData.Keep("permanent-VFDTemplate");
                input = new InputParameters();
                pageParams = new PageParameters();

                TempData.Put("permanent-VFDTemplate", permanent);
                return await Task.FromResult(Page());
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                TempData.Put("permanent-VFDTemplate", permanent);
                return await Task.FromResult(RedirectToPage("./VFDTemplate",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                permanent = TempData.Get<PermanentParameters>("permanent-VFDTemplate") ?? new PermanentParameters();
                TempData.Keep("permanent-VFDTemplate");

                TempData.Put("permanent-VFDTemplate", permanent);
                return await Task.FromResult(RedirectToPage("./VFDTemplate",
                                                            new { fromPOST = "Yes" }
                                                           )
                                            );
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                TempData.Put("permanent-VFDTemplate", permanent);
                return await Task.FromResult(RedirectToPage("./VFDTemplate",
                                                            new
                                                            {
                                                                fromPOST = "Yes",
                                                                haveError = "Yes"
                                                            }
                                                           )
                                            );
            }
        }
    }
}