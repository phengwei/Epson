using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Epson.Factories;
using Epson.Core.Domain.Requests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Epson.Services.Interface.Requests;
using Epson.Model.Request;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Core.Domain.Enum;
using Epson.Services.Interface.Products;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Requests;
using System.Globalization;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Users;
using Epson.Services.DTO.Products;
using Microsoft.EntityFrameworkCore;
using Epson.Data;
using System.Web.Razor.Generator;
using Newtonsoft.Json;
using Epson.Services.Services.Requests;
using Epson.Services.Interface.AuditTrails;
using Epson.Services.Services.AuditTrails;
using Epson.Model.Users;

namespace Epson.Controllers.API
{
    [Route("api/serviceRequest")]
    public class ServiceRequestApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IProductService _productService;
        private readonly IAuditTrailService _auditService;
        private readonly IUserService _userService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Team> _teamRepository;
        private readonly IConfiguration _configuration;
        private readonly IDraftService _draftService;

        public ServiceRequestApiController(
            IRequestService requestService,
            IServiceRequestService serviceRequestService,
            IProductService productService,
            IAuditTrailService auditService,
            IUserService userService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IRepository<Team> teamRepository,
            IConfiguration configuration,
            IDraftService draftService)
        {
            _requestService = requestService;
            _serviceRequestService = serviceRequestService;
            _productService = productService;
            _auditService = auditService;
            _userService = userService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
            _teamRepository = teamRepository;
            _configuration = configuration;
            _draftService = draftService;
        }

        [HttpGet("GetServiceRequestByID")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> RequestById(int id)
        {
            var response = new GenericResponseModel<ServiceRequestDTO>();

            if (id == null || id == 0)
                return BadRequest("Id must not be empty");

            ServiceRequestDTO request = _serviceRequestService.GetRequestById(id);
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);
            var approver = await _userManager.FindByIdAsync(request.approvedBy);

            if (approver != null)
            {
                request.approvedByName = approver.UserName;
            }

            response.Data = request;
            return Ok(response);
        }

        [HttpGet("GetServiceRequests")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetRequests(string search = null, int? page = null, int? itemsPerPage = null, bool breached = false, int month = 0, int approvalState = 0)
        {
            try
            {
                var response = new GenericResponseModel<List<ServiceRequestDTO>>();
                var currentUser = _workContext.CurrentUser;
                var currentUserDetail = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

                
                int totalItems;

                List<ServiceRequestDTO> serviceRequests = new List<ServiceRequestDTO>();

                serviceRequests = _serviceRequestService.GetServiceRequests();

                if (page.HasValue && itemsPerPage.HasValue && itemsPerPage.Value != -1)
                {
                    serviceRequests = serviceRequests
                                     .Skip((page.Value - 1) * itemsPerPage.Value)
                                     .Take(itemsPerPage.Value)
                                     .ToList();
                }

                response.Data = serviceRequests;
                response.Count = serviceRequests.Count;

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception message for debugging purposes
                return StatusCode(500, new { message = "An error occurred while fetching requests.", error = ex.Message });
            }
        }

        [HttpPost("CreateServiceRequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> CreateRequest([FromBody] BaseQueryModel<ServiceRequestDTO> queryModel)
        {
            if (queryModel == null || queryModel.Data == null)
            {
                return BadRequest("Invalid request.");
            }

            var model = queryModel.Data;

            var user = _workContext.CurrentUser;
            var dbUser = await _userManager.FindByIdAsync(user.Id);


            model.createdOnUTC = DateTime.UtcNow;
            model.updatedOnUTC = DateTime.UtcNow;
            model.createdByID = user.Id;
            model.createdByStr = user.Name;
            model.updatedByID = user.Id;
            model.updatedByStr = user.Name;

            if (_serviceRequestService.InsertServiceRequest(model))
                return Ok();
            else
                return BadRequest("Failed to create request");
        }

        [HttpGet("GetPendingManagerItems")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetPendingManagerItems(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<ServiceRequestDTO>>();
            var currentUser = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (currentUser == null)
            {
                response.Data = new List<ServiceRequestDTO>();
                response.Count = 0;
                return Ok(response);
            }

            var userTeam = _teamRepository.GetAll().Where(x => x.Id == currentUser.TeamId).FirstOrDefault();

            if (userTeam == null)
            {
                response.Data = new List<ServiceRequestDTO>();
                response.Count = 0;
                return Ok(response);
            }

            var classificationPath = userTeam.Name;

            var serviceRequests = _serviceRequestService.GetServiceRequests().Where(x => x.serviceRequestStatus == (int)ServiceRequestStatusEnum.PendingAssignment).ToList();

            serviceRequests = serviceRequests
                .Where(sr => sr.classificationPath == classificationPath)
                .ToList();

            //if (!string.IsNullOrEmpty(search))
            //{
            //    serviceRequests = serviceRequests
            //        .Where(sr => sr.SomeField.Contains(search, StringComparison.OrdinalIgnoreCase)
            //                  || sr.AnotherField.Contains(search, StringComparison.OrdinalIgnoreCase))
            //        .ToList();
            //}

            if (page.HasValue && itemsPerPage.HasValue && itemsPerPage.Value > 0)
            {
                serviceRequests = serviceRequests
                    .Skip((page.Value - 1) * itemsPerPage.Value)
                    .Take(itemsPerPage.Value)
                    .ToList();
            }

            response.Data = serviceRequests;
            response.Count = serviceRequests.Count;

            return Ok(response);
        }

        [HttpGet("GetDepartmentUsers")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> GetDepartmentUsers()
        {
            var response = new GenericResponseModel<List<UserModel>>();

            // Get the current user
            var currentUser = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);
            if (currentUser == null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            // Get the current user's team
            var userTeam = _teamRepository.GetAll()
                                            .Where(x => x.Id == currentUser.TeamId)
                                            .FirstOrDefault();

            if (userTeam == null)
            {
                return NotFound(new { message = "User's team not found." });
            }

            // Get all users in the same team
            var teamUsers = await _userManager.Users
                .Where(u => u.TeamId == userTeam.Id)
                .ToListAsync();

            var allTeams = _userService.GetTeams();
            var teamLookup = allTeams.ToDictionary(t => t.Id, t => t.Name);

            // Filter only users with the role "Maker"
            var departmentUsers = new List<UserModel>();
            foreach (var user in teamUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var filteredRoles = roles.Where(role => !role.Equals("admin", StringComparison.OrdinalIgnoreCase)).ToList();
                if (roles.Contains("Maker"))
                {
                    departmentUsers.Add(new UserModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = filteredRoles,
                        Phone = user.PhoneNumber,
                        Teams = teamLookup.TryGetValue(user.TeamId, out var teamName) ? teamName : null,
                        TeamId = user.TeamId,
                        LockoutEnd = user.LockoutEnd,
                        IsActive = user.IsActive
                    });
                }
            }

            response.Data = departmentUsers;
            response.Count = departmentUsers.Count;

            return Ok(response);
        }

        [HttpPost("AssignServiceRequestMaker")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
        public async Task<IActionResult> AssignServiceRequestMaker(int requestId, string newOwnerId)
        {
            if (requestId == 0 || requestId == null || string.IsNullOrEmpty(newOwnerId) | newOwnerId == null)
                return BadRequest(new { message = "Invalid request. Request ID and User ID are required." });

            var user = await _userManager.FindByIdAsync(newOwnerId);
            if (user == null)
                return NotFound(new { message = $"User with ID {newOwnerId} not found." });

            bool result = _serviceRequestService.AssignServiceRequestMaker(requestId, user);

            if (!result)
                return BadRequest(new { message = "Failed to assign the service request to the user." });

            return Ok(new { message = "Service request successfully assigned to the Maker." });
        }


        public class RequestDTOComparer : IEqualityComparer<ServiceRequestDTO>
        {
            public bool Equals(ServiceRequestDTO x, ServiceRequestDTO y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;
                return x.id == y.id;
            }

            public int GetHashCode(ServiceRequestDTO obj)
            {
                if (Object.ReferenceEquals(obj, null)) return 0;
                return obj.id.GetHashCode();
            }
        }


    }
}
