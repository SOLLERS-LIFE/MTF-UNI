using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Microsoft.EntityFrameworkCore;

// client to k8s instance of https://thecodingmachine.github.io/gotenberg/
// https://github.com/ChangemakerStudios/GotenbergSharpApiClient
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

// ysing memcache server from k8s
// using Enyim.Caching;

using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.ApplicationDB.Data;
using MTF.Utilities;
using MTF.Areas.ApplicationDB.Pages.BS.Models;

namespace MTF.Pages
{
    // Idea to have TempData here permanent to this type of views
    public class aotInvariants
    {
    }
    [Authorize]
    public class asideOpsTestsModel : applicationPageModel<aotInvariants, asideOpsTestsModel>
    {
        public GotenbergSharpClient _sharpClient { get; set; }

        public asideOpsTestsModel(CommonIdent context,
                                  IAuthorizationService authorizationService,
                                  UserManager<CommonUser> userManager,
                                  AppDB_Context appDBContext,
                                  ILogger<asideOpsTestsModel> logger,
                                  IpSafeList ipSafeList,
                                  GotenbergSharpClient sharpClient
                                 )
            : base(context,
                   authorizationService,
                   userManager,
                   appDBContext,
                   logger,
                   ipSafeList,
                   "asideOpsTest"
                  )
        {
            _sharpClient = sharpClient;
            _pageParams = new PageParams();
        }

        // idea to keep here all parameters for page html generation
        public class PageParams
        {
        }
        public PageParams _pageParams { get; set; }

        public async Task<IActionResult> OnGetAsync(string haveError,
                                                    string fromPOST,
                                                    string reqcontent)
        {
            try
            {
                if (haveError == "Yes" && fromPOST != "Yes")
                {
                    return await Task.FromResult(Page());
                }

                await _prologue();

                if (reqcontent == "Yes")
                { 
                    ViewData["reqcontent"] = "1";
                }

                //_pageParams.keyForCache = $"{_userRef.Id}~{Guid.NewGuid()}";

                await _epilogue();

                return await Task.FromResult(Page()); ;
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return await Task.FromResult(RedirectToPage("/LookingForward/asideOpsTests",
                                                            new { haveError = "Yes" }
                                                           )
                                            );
            }          
        }

        // Long-running method to prepare a report
        public async Task<IActionResult> OnPostAsideOpsMngrAsync(string op, string prm)
        {
            try
            {
                await _prologue();

                if (String.IsNullOrEmpty(op))
                {
                    throw new Exception("No operation specified.");
                }

                byte[] _body;
                switch (op)
                {
                    case "getPageHTML":
                        _body = System.Text.Encoding.UTF8.GetBytes(prm);
                        break;
                    case "getReportExample":
                        _body = (await example()).ToArray();
                        break;
                    default:
                        throw new Exception("Unknown operation.");
                }

                String _key = $"{_userRef.Id}~{Guid.NewGuid()}";
                IDatabase rdb = GlobalParameters._redis.GetDatabase();
                if (!await rdb.StringSetAsync(_key, _body, TimeSpan.FromMinutes(5)))
                {
                    throw new Exception("Unable to write into cache.");
                }

                await _epilogue();
                return new ObjectResult(new { status = "success", redisKey = _key });
            }
            catch (Exception ex)
            {
                await _epilogue_catcher(ex);
                return new ObjectResult(new { status = "fail" });
            }
        }

        private async Task<MemoryStream> example()
        {
            var builder = new HtmlRequestBuilder()
                                 .AddDocument(doc =>
                                     doc.SetBody(GetBody("")).SetFooter(GetFooter("MTF 3.1.04"))
                                 ).WithDimensions(dims =>
                                 {
                                     dims.SetPaperSize(PaperSizes.A4)
                                         .SetMargins(Margins.Normal)
                                         .LandScape(true);
                                 }).ConfigureRequest(config =>
                                 {
                                     config.ChromeRpccBufferSize(1024);
                                 });
            var req = await builder.BuildAsync();

            var result = await _sharpClient.HtmlToPdfAsync(req);

            MemoryStream dst;
            await result.CopyToAsync(dst = new MemoryStream(1 * 1024 * 1024));
            dst.Position = 0;

            IDatabase rdb = GlobalParameters._redis.GetDatabase();
            String key = $"{_userManager.GetUserId(User)}~{Guid.NewGuid()}";
            if (!await rdb.StringSetAsync(key, dst.ToArray(), TimeSpan.FromMinutes(5)))
            {
                throw new Exception("Unable to write to redis cache.");
            }

            dst.Position = 0;
            return dst;
        }

        static string GetBody(string _url)
        {
            return @"<!doctype html>
			<html lang=""en"">
    			<style>
					body { margin: auto;} h1 { font-size: 55px; }
					h1, h3{ text-align: center; margin-top: 5px; } .photo-container { padding-bottom: 20px;  }
					figure { width:548px; height:600px; } img { border: 10px solid #000; } 
					figcaption { text-align: right; font-size: 10pt; } 	a:link, a:visited { color: black !important; }
				</style>
				<head><meta charset=""utf-8""><title>Thanks to TheCodingMachine</title></head>  
				<body>
					<h1>Hello world</h1>
					<div class=""container-fluid photo-container"">
						<figure>
						    <img src=""https://db5pap001files.storage.live.com/y4mYOh9qAEadezQvgW2KKxeBnWz8sGK0zJ9HG1pol6ToLVFAJXgAKBfGe103NNaPD3AhCg34UomP6c4aH9LDH8IQmjVtAlt6_qozWVU12R1E11mLZV-OeCPzz7M881YkMGsioKn9yElcRgHxc9Sk9mzcbVOxsXyOMdTT35ia3KrwwTQ-3kNGdBxZbeKcwJnle4i?width=4000&height=3000&cropmode=none"" width=""600"" height=""450""/>
                            <figcaption>Photo by <a href=""https://www.linkedin.com/in/valerii-danilov-851505124/"">Valerii Danilov</a>.</figcaption>
						 </figure>   
					</div>
					<h3>Powered by <a href=""https://hub.docker.com/r/thecodingmachine/gotenberg"">The Coding Machine</a></h3>
				</body>
			</html>";
        }

        static string GetFooter(string _url)
        {

            var s = @$"<html>
                        <head>
                            <style>
                                body {{ font-size: 8rem; font-family: Roboto,""Helvetica Neue"",Arial,sans-serif; margin: 4rem auto; }}
                            </style>
                        </head>
	                    <body>
                            <p>Page <span class=""pageNumber""></span> of <span class=""totalPages""> pages</span> PDF Created on <span class=""date""></span> Source URL: <span>""{_url}""</span></p>
                        </body>
                    </html>";
            return s;
        }
    }
}