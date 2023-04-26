using Epson.Core.Domain.Users;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Epson.Controllers.API
{
    [Route("api/customer")]
    public class CustomerApiController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public CustomerApiController(
            UserManager<ApplicationUser> userManager,
            JwtSettings jwtSettings,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _jwtService = jwtService;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] BaseQueryModel<LoginModel> queryModel)
        {
            var model = queryModel.Data;
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var authClaims = new List<Claim>
                {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Issuer,
                    expires: DateTime.Now.AddDays(7),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }

            return Unauthorized("User unauthorized");
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] BaseQueryModel<RegisterModel> queryModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var model = queryModel.Data;

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _jwtService.GenerateToken(user);

                return Ok(new { token });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return BadRequest(ModelState);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("testadmin")]
        public IActionResult TestAdmin()
        {
            return Ok("Admin access successful");
        }

        [Authorize(Roles = "Sales")]
        [HttpGet("testsales")]
        public IActionResult TestSales()
        {
            return Ok("Sales user access successful");
        }
    }
}
