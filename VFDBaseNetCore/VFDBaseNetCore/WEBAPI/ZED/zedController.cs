using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

// https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio

// Authorization
// https://www.c-sharpcorner.com/article/authentication-and-authorization-in-asp-net-core-web-api-with-json-web-tokens/
// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-5.0
namespace MTF.WEBAPI.ZED
{
    // curl -kv -X GET -H "Content-Type: application/json" -H "Authorization: Bearer bla-bla-bla" https://localhost:5001/etl/certs
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ETL")]
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    public class zedController : ControllerBase
    {
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/recommended-tags-for-documentation-comments
        private readonly List<String> _input = new();

        public zedController()
        {
            _input.Add("Don't panic, baby!");
        }

        /// <summary>
        /// retreive solace from an application database
        /// </summary>  
        [HttpGet]
        public ActionResult<List<String>> Index()
        {
            return _input;
        }

        /// <summary>
        /// retreive a definite solace from an application database
        /// </summary>
        /// <param name="nm">Your known name</param>  
        [HttpGet("{nm}")]
        public ActionResult<String> GetWithMod(String nm)
        {
            return Ok("Oh, " + nm +"!" +_input.FirstOrDefault());
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
        /// Test non-ascii strings passing
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Test([FromBody] string[] crt)
        {
            try
            {
                string t1 = crt[0];

                return await Task.FromResult(Ok(new { ETLcode = 0, description = "Test successful." }));
            }
            catch (Exception ex)
            {
                var _msg = "ETL-ERROR: "
                            + ex.Message
                            + "; "
                            + (ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return await Task.FromResult(Ok(new { ETLcode = 555, description = _msg }));
            }
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}