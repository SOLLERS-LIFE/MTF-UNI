using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MTF.Areas.Identity.Data;

// https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio

// Authorization
// https://www.c-sharpcorner.com/article/authentication-and-authorization-in-asp-net-core-web-api-with-json-web-tokens/
// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-5.0
namespace MTF.WEBAPI
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class authController : ControllerBase
    {
        public class loginInfo
        {
            [Required(ErrorMessage = "User Name is required")]
            public string Username { get; set; }
            [Required(ErrorMessage = "Password is required")]
            public string Password { get; set; }
        }
        private UserManager<CommonUser> _userManager { get; set; }
        private IConfiguration _configuration { get; set; }
        private ILogger<authController> _logger { get; set; }

        public authController(IConfiguration configuration,
                              ILogger<authController> logger,
                              UserManager<CommonUser> userManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Obtain jwtoken on a basis of valid user name and password
        /// </summary>
        /// <param name="linf">a structure to represent a pare of login and password</param>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> login([FromBody] loginInfo linf)
        {
            var user = await _userManager.FindByNameAsync(linf.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, linf.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Sid, user.Id),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.NormalizedEmail),

                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(_configuration.GetValue<int>("JWT:TTL", 60)),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                String _msg01 = "Bearer Token was given to user " + user.Email;
                _logger.LogInformation($"{_msg01}.");

                return await Task.FromResult(
                    Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    })
                );
            }

            String _msg = "Unsuccessful Bearer Token request for user " + linf.Username;
            _logger.LogError($"{_msg}.");
            return await Task.FromResult(Unauthorized());
        }
    }
}