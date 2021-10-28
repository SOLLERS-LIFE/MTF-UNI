using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

using MTF.Areas.ApplicationDB.Data;

namespace MTF.WEBAPI.ETL
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ETL")]
    [Produces("application/json")]
    [ApiController]
    [Route("etl/[controller]")]
    public class etlController : ControllerBase
    {
        private ILogger<etlController> _logger { get; set; }
        private AppDB_Context _appDB { get; set;}
        public etlController(ILogger<etlController> logger,
                               AppDB_Context appDB)
        {
            _logger = logger;
            _appDB = appDB;
        }

        public class etlState
        {
            public int vc { get; set; }
            public string un { get; set; }
            public string um { get; set; }
            public string uid { get; set; }
        }
        // curl -kv -X GET -H "Content-Type: application/json" -H "Authorization: Bearer bla-bla-bla" <...>
        /// <summary>
        /// retreive certificates' ETL subsystem state
        /// </summary>  
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetState()
        {
            string tm = string.Empty;

            if (User.Identity.IsAuthenticated)
            {
                return await Task.FromResult(
                   Ok(new etlState
                   {
                       vc = 0,
                       un = User.Identity.Name, // User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault()?.Value
                       um = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value,
                       uid = User.Claims.Where(x => x.Type == ClaimTypes.Sid).FirstOrDefault()?.Value
                   })
                );
            }
            else
            {
                return await Task.FromResult(
                   Ok(new etlState
                   {
                       vc = -1,
                       un = User.Identity.Name,
                       um = "",
                       uid = "You are ugly bastard, may an evil fuck you!"
                   })
                );
            }
        }

        // curl -kv -X 'POST' \
        // 'https://localhost:5001/etl/certs' \
        // -H 'accept: */*' \
        // -H 'Content-Type: application/json; charset=utf-8' \
        // -H "Authorization: Bearer bla-bla-bla" \
        // -d '[
        //  "yoptat;toptat ;  karamella "
        // ]'
        /// <summary>
        /// Add new certificate (or update one if a cert with same certNo already exists in the database
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddOrModify([FromBody] string[] crt)
        {
            try
            {
                foreach (string ln in crt)
                {
                }

                return await Task.FromResult(Ok(new { ETLcode = 0, descn = "" }));
            }
            catch (Exception ex)
            { 
                var _msg = "ETL-ERROR: "
                            + ex.Message
                            + "; "
                            + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_msg}.");
                return await Task.FromResult(Ok(new { ETLcode=555, description = _msg }));
            } 
        }
    }
}
