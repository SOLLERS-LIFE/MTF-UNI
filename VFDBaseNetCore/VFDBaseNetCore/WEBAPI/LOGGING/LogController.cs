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

using MTF.Utilities;
using MTF.Areas.Logging.Data;

namespace MTF.WEBAPI.LOGGING

{
    [AllowAnonymous]
    [Produces("application/json")]
    [ApiController]
    [Route("LOGGING/[controller]")]
    public class logController : ControllerBase
    {
        private ILogger<logController> _logger { get; set; }
        private LogDB_Context _logDB { get; set;}
        public logController(ILogger<logController> logger,
                             LogDB_Context logDB
                            )
        {
            _logger = logger;
            _logDB = logDB;
        }

        public class logState
        {
            public string state { get; set; }
        }
        public class logRecord
        {
            public string level { get; set; }
            public string appIdent { get; set; }
            public string callsite { get; set; }
            public string expn { get; set; }
            public string logger { get; set; }
            public string machineName { get; set; }
            public string message { get; set; }
            public string reqhost { get; set; }
            public string uId { get; set; }
            public string url { get; set; }
        }
        // curl -kv -X GET -H "Content-Type: application/json" https://localhost:5001/logging/log
        /// <summary>
        /// retreive logging subsystem's state
        /// </summary>  
        [AllowAnonymous]
        [ServiceFilter(typeof(ClientIpCheckActionFilter))]
        [HttpGet]
        public async Task<IActionResult> GetState()
        {
            
            return await Task.FromResult(
                   Ok(new logState
                   {
                       state = "stable"
                   })
                );
        }

        // curl -kv -X 'POST' \
        // 'https://localhost:5001/logging/log' \
        // -H 'accept: */*' \
        // -H 'Content-Type: application/json; charset=utf-8' \
        // -d '[
        //  "yoptat;toptat ;  karamella "
        // ]'
        /// <summary>
        /// Add log record
        /// </summary>
        [ServiceFilter(typeof(ClientIpCheckActionFilter))]
        [HttpPost]
        public async Task<IActionResult> AddRecord([FromBody] logRecord rec)
        {
            try
            {
                var rc = await _logDB.getScalarValue("call NLog_AddRecord",
                                                         rec.machineName,
                                                         rec.appIdent,
                                                         DateTime.Now,
                                                         rec.level,
                                                         rec.message,
                                                         rec.logger,
                                                         "",
                                                         rec.callsite,
                                                         rec.expn,
                                                         rec.url,
                                                         rec.reqhost,
                                                         rec.uId
                                                         ); ;
                return await Task.FromResult(Ok(new { LOGcode = 0, desc = "" }));
            }
            catch (Exception ex)
            { 
                var _msg = "LOG-ERROR: "
                            + ex.Message
                            + "; "
                            + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                _logger.LogError($"{_msg}.");
                return await Task.FromResult(Ok(new { LOGcode=555, desc = _msg }));
            } 
        }
    }
}
