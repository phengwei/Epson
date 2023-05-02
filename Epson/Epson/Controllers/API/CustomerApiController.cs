using Epson.Core.Domain.Users;
using Epson.Model.Common;
using Epson.Model.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Epson.Model.Products;
using System.Security.Claims;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/customer")]
    public class CustomerApiController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly JwtSettings _jwtSettings;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;

        public CustomerApiController(
            UserManager<ApplicationUser> userManager,
            RoleManager<Role> roleManager,
            JwtSettings jwtSettings,
            JwtService jwtService,
            IConfiguration configuration,
            Serilog.ILogger logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings;
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] BaseQueryModel<LoginModel> queryModel)
        {
            var model = queryModel.Data;
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var generatedToken = await _jwtService.GenerateToken(user);
                
                return Ok(new
                {
                    token = generatedToken
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

        [HttpGet("getcurrentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var response = new GenericResponseModel<UserModel>();

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Unauthorized("User must be logged in!");
            }
            var roles = await _userManager.GetRolesAsync(user);

            response.Data.UserName = user.UserName;
            response.Data.Email = user.Email;
            response.Data.Roles = (List<string>)roles;

            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            _jwtService.InvalidateToken(token);

            Response.Headers.Remove("Authorization");

            return Ok(new { message = "Logout successful" });
        }

        [HttpPost("changepassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await _userManager.CheckPasswordAsync(user, currentPassword);
            if (!result)
                return BadRequest("Invalid password.");

            //Change password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var changeResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!changeResult.Succeeded)
                return BadRequest(changeResult.Errors);

            return Ok();
        }

        [HttpPost("addroletouser")]
        [AllowAnonymous]
        public async Task<IActionResult> AddRoleToUser(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound();

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
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
