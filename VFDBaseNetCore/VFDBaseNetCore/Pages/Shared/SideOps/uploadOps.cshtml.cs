using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
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

using MTF.Utilities;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;

namespace MTF.Pages
{
    [Authorize]
    public class uploadOpsModel : Ext_AuthPageModel
    {
        public uploadOpsModel(
                               CommonIdent context,
                               IAuthorizationService authorizationService,
                               UserManager<CommonUser> userManager,
                               ILogger<uploadOpsModel> logger,
                               IpSafeList ipSafeList
                             )
            : base(context,
                   authorizationService,
                   userManager,
                   logger,
                   ipSafeList,
                   "uploadFile"
                  )
        {
            _logger = logger;
        }
        public async Task<IActionResult> OnPostUploaderAsync(IFormFile Uploader, string op)
        {
            try
            {
                IDatabase rdb = GlobalParameters._redis.GetDatabase();
                if (Uploader == null || String.IsNullOrEmpty(op))
                {
                    throw new Exception("Some parameters are null.");
                }
                String cacheKey = $"{_userManager.GetUserId(User)}~{Guid.NewGuid()}";
                using (var memStream = new MemoryStream(1*1024*1024))
                {
                    Uploader.CopyTo(memStream);
                    memStream.Position = 0;

                    if (!await rdb.StringSetAsync(cacheKey, memStream.ToArray(), TimeSpan.FromMinutes(5)))
                    {
                        throw new Exception("Unable to write to redis cache.");
                    }
                }

                return new ObjectResult(new { status = "success", redisKey = cacheKey });
            }
            catch(Exception ex)
            {
                _statusMessage = "Error: "
                                 + ex.Message
                                 + "; "
                                 + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_statusMessage}.");

                return new ObjectResult(new { status = "fail", descr = _statusMessage });
            }
        }
    }
}
