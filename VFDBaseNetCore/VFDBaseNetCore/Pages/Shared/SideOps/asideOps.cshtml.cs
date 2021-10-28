using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using StackExchange.Redis;

// client to k8s instance of https://thecodingmachine.github.io/gotenberg/
// https://github.com/ChangemakerStudios/GotenbergSharpApiClient
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Pages
{
    [Authorize]
    public class asideOpsModel : Ext_AuthPageModel
    {
        private GotenbergSharpClient _sharpClient { get; set; }
        public asideOpsModel(
                                   CommonIdent context,
                                   IAuthorizationService authorizationService,
                                   UserManager<CommonUser> userManager,
                                   ILogger<asideOpsModel> logger,
                                   GotenbergSharpClient sharpClient,
                                   IpSafeList ipSafeList
                                )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "downloadOpers"
                  )
        {
            _logger = logger;
            _sharpClient = sharpClient;
        }

        public async Task<IActionResult> OnGetAsync ()
        {
            return await Task.FromResult(Page());
        }

        public async Task<IActionResult> OnGetBabyAsync (string op, string pf,
                                                         string tp="application/pdf", string dn= "ReportExample.pdf"
                                                        )
        {
            try
            {
                if ((String.IsNullOrEmpty(op)) || (pf.Split('~')[0] != _userManager.GetUserId(User)))
                {
                    throw new Exception("illegal parameters or intrusion attempt.");
                }
                IDatabase rdb = GlobalParameters._redis.GetDatabase();
                switch (op)
                {
                    case "getFromCache":
                        return await Task.FromResult(File((byte[])(await rdb.StringGetAsync(pf)), tp, dn));

                    default:
                        throw new Exception("Unknown operation.");
                }
            }
            catch (Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_statusMessage}.");

                return await Task.FromResult(Content(ex.Message, "text/plain"));
            }
        }
    }
}
