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
using Epson.Infrastructure;
using Epson.Services.Interface.Users;
using AutoMapper;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/customer")]
    public class CustomerApiController : BaseApiController
    {
        private readonly IWorkContext _workContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly JwtSettings _jwtSettings;
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly Serilog.ILogger _logger;
        private readonly IMapper _mapper;

        public CustomerApiController(
            IWorkContext workContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<Role> roleManager,
            JwtSettings jwtSettings,
            JwtService jwtService,
            IUserService userService,
            IConfiguration configuration,
            Serilog.ILogger logger,
            IMapper mapper)
        {
            _workContext = workContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings;
            _jwtService = jwtService;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// Login to generate jwt token
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///{
        ///  "data": {
        ///    "userName": "salesuser",
        ///    "password": "Abcde123."
        ///  }
        ///}
        /// </remarks>
        /// <param name="login">User login.</param>

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

        [HttpPost("addnewuser")]
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
                PhoneNumber = model.Phone,
                TeamId = model.TeamId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            try
            {
                if (result.Succeeded)
                {
                    foreach (var roleName in model.Roles)
                    {
                        if (await _roleManager.RoleExistsAsync(roleName))
                        {
                            await _userManager.AddToRoleAsync(user, roleName);
                        }
                    }

                    var token = await _jwtService.GenerateToken(user);

                    return Ok(new { token });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }catch(Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return BadRequest();
        }

        [HttpPost("edituser")]
        [AllowAnonymous]
        public async Task<IActionResult> EditUser([FromBody] BaseQueryModel<RegisterModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                user.UserName = model.Username;
                user.Email = model.Email;
                user.PhoneNumber = model.Phone;
                user.TeamId = model.TeamId;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var rolesToAdd = model.Roles.Except(userRoles);
                    var rolesToRemove = userRoles.Except(model.Roles);

                    foreach (var role in rolesToAdd)
                        if (await _roleManager.RoleExistsAsync(role))
                            await _userManager.AddToRoleAsync(user, role);

                    foreach (var role in rolesToRemove)
                        if (await _roleManager.RoleExistsAsync(role))
                            await _userManager.RemoveFromRoleAsync(user, role);
                    return Ok();
                }
            }
            else
            {
                return NotFound();
            }
            return BadRequest();
        }

        [HttpDelete("deleteuser")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var rolesForUser = await _userManager.GetRolesAsync(user);


            IdentityResult result;
            foreach (var role in rolesForUser)
                result = await _userManager.RemoveFromRoleAsync(user, role);

            result = await _userManager.DeleteAsync(user);

            return Ok();
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

            response.Data.Id = user.Id;
            response.Data.UserName = user.UserName;
            response.Data.Email = user.Email;
            response.Data.Roles = roles;

            return Ok(response);
        }

        [HttpGet("getavailableteams")]
        public async Task<IActionResult> GetAvailableTeam()
        {
            var response = new GenericResponseModel<List<TeamModel>>();

            var teams = _userService.GetTeams();

            foreach (var team in teams)
            {
                TeamModel teamModel = new TeamModel();
                teamModel.Id = team.Id;
                teamModel.Name = team.Name;

                response.Data.Add(teamModel);
            }

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
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var currentUser = _workContext.CurrentUser;
            var user = await _userManager.FindByIdAsync(currentUser.Id);
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


        [HttpPost("adminchangepassword")]
        [AllowAnonymous]
        public async Task<IActionResult> AdminChangePassword(string newPassword)
        {
            var currentUser = _workContext.CurrentUser;
            var user = await _userManager.FindByIdAsync(currentUser.Id);
            if (user == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            try
            {
                var changeResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!changeResult.Succeeded)
                    return BadRequest(changeResult.Errors);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


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

        [HttpGet("getallstaff")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStaff()
        {
            var response = new GenericResponseModel<List<UserModel>>();

            var productUsers = await _userManager.GetUsersInRoleAsync("Product");
            var salesUsers = await _userManager.GetUsersInRoleAsync("Sales");

            var productSalesUsers = productUsers.Concat(salesUsers).Distinct(new ApplicationUserComparer()).ToList();

            response.Data = productSalesUsers.Select(user => new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            }).ToList();

            return Ok(response);
        }

        [HttpGet("getallrequesters")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRequester()
        {
            var response = new GenericResponseModel<List<UserModel>>();

            var salesUsers = await _userManager.GetUsersInRoleAsync("Sales");

            response.Data = salesUsers.Select(user => new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            }).ToList();

            return Ok(response);
        }

        [HttpGet("getallfulfillers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllFulfiller()
        {
            var response = new GenericResponseModel<List<UserModel>>();

            var salesUsers = await _userManager.GetUsersInRoleAsync("Product");

            response.Data = salesUsers.Select(user => new UserModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            }).ToList();

            return Ok(response);
        }

        [HttpGet("getallusers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = new GenericResponseModel<List<UserModel>>();

            var users = _userService.GetAllUsers();
            var userModels = new List<UserModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var userModel = new UserModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    TeamId = user.TeamId,
                    Teams = _mapper.Map<Team>(_userService.GetTeamById(user.TeamId)).Name
                };

                userModels.Add(userModel);
            }

            response.Data = userModels;

            return Ok(response);
        }

        [HttpGet("getallroles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = new GenericResponseModel<List<Role>>();

            var roles = _roleManager.Roles.ToList();

            response.Data = roles;

            return Ok(response);
        }
    }
}
