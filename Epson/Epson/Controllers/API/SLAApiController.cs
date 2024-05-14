using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Epson.Services.Interface.Products;
using Epson.Model.Products;
using Epson.Factories;
using Epson.Core.Domain.Products;
using AutoMapper;
using Epson.Services.Interface.SLA;
using Epson.Model.SLA;
using Epson.Core.Domain.SLA;
using Epson.Services.Interface.Requests;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Services.Interface.Users;
using Epson.Data;
using Epson.Core.Domain.Enum;
using Epson.Services.DTO.Requests;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Sales, Product, Sales Section Head")]
    [Route("api/sla")]
    public class SLAApiController : BaseApiController
    {
        private readonly ISLAService _slaService;
        private readonly ISLAModelFactory _slaModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IRequestService _requestService;
        private readonly IRepository<Team> _teamRepository;


        public SLAApiController(
            ISLAService slaService,
            ISLAModelFactory slaModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            IConfiguration configuration,
            IWebHostEnvironment hostEnvironment,
            UserManager<ApplicationUser> userManager,
            IUserService userService,
            IRequestService requestService,
            IRepository<Team> teamRepository)
        {
            _slaService = slaService;
            _slaModelFactory = slaModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _userService = userService;
            _requestService = requestService;
            _teamRepository = teamRepository;
        }

        [HttpGet("getslaholidays")]
        public async Task<IActionResult> GetSLAHolidays()
        {
            var response = new GenericResponseModel<List<SLAHolidayModel>>();

            var slaHolidays = _slaService.GetSLAHolidays();

            var slaHolidayModels = _slaModelFactory.PrepareSLAHolidayModels(slaHolidays);

            response.Data = slaHolidayModels;

            return Ok(response);
        }

        [HttpGet("getslastaffleaves")]
        public async Task<IActionResult> GetSLAStaffLeaves()
        {
            var response = new GenericResponseModel<List<SLAStaffLeaveModel>>();

            var slaStaffLeaves = _slaService.GetSLAStaffLeaves();

            var slaStaffLeaveModels = _slaModelFactory.PrepareSLAStaffLeaveModels(slaStaffLeaves);

            response.Data = slaStaffLeaveModels;

            return Ok(response);
        }

        [HttpGet("getslastaffleavesbystaff")]
        public async Task<IActionResult> GetSLAStaffLeavesByStaff(string staffId)
        {
            var response = new GenericResponseModel<List<SLAStaffLeaveModel>>();

            var slaStaffLeaves = _slaService.GetSLAStaffLeaves().Where(x => x.StaffId == staffId).ToList();

            var slaStaffLeaveModels = _slaModelFactory.PrepareSLAStaffLeaveModels(slaStaffLeaves);

            response.Data = slaStaffLeaveModels;

            return Ok(response);
        }


        [HttpPost("addslaholiday")]
        public async Task<IActionResult> AddSLAHoliday([FromBody] BaseQueryModel<SLAHolidayModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            var slaHoliday = new SLAHoliday
            {
                Date = model.Date,
                Description = model.Description,
                IsAdhoc = model.IsAdhoc
            };

            if (_slaService.InsertSLAHoliday(slaHoliday))
                return Ok();
            else
                return BadRequest("Failed to insert SLA holiday");
        }

        [HttpPost("addslastaffleave")]
        public async Task<IActionResult> AddSLAStaffLeave([FromBody] BaseQueryModel<SLAStaffLeave> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            var slaStaffLeave = new SLAStaffLeave
            {
                StaffId = model.StaffId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Reason = model.Reason
            };

            if (_slaService.InsertSLAStaffLeave(slaStaffLeave))
                return Ok();
            else
                return BadRequest("Failed to insert SLA Staff Leave");
        }

        [HttpPost("deleteslaholiday")]
        public async Task<IActionResult> DeleteSLAHoliday(int id)
        {
            var slaHoliday = _slaService.GetSLAHolidayById(id);

            var slaHolidayToDelete = _mapper.Map<SLAHoliday>(slaHoliday);

            if (_slaService.DeleteSLAHoliday(slaHolidayToDelete))
                return Ok();
            else
                return BadRequest("Failed to delete SLA Holiday");
        }

        [HttpPost("deleteslastaffleave")]
        public async Task<IActionResult> DeletSLAStaffLeave(int id)
        {
            var slaStaffLeave = _slaService.GetSLAStaffLeaveById(id);

            var slaStaffLeaveToDelete = _mapper.Map<SLAStaffLeave>(slaStaffLeave);

            if (_slaService.DeleteStaffLeaveHoliday(slaStaffLeaveToDelete))
                return Ok();
            else
                return BadRequest("Failed to delete SLA Staff Leave");
        }

        [HttpGet("getslasettings")]
        public async Task<IActionResult> GetSLASettings()
        {
            var response = new GenericResponseModel<SLASettingModel>();

            var slaSettings = _slaService.GetSLASettings();

            var slaSettingsModel = _slaModelFactory.PrepareSLASettingModel(slaSettings);

            response.Data = slaSettingsModel;

            return Ok(response);
        }

        // Changes will only take place after restarting application
        [HttpPost("updateslasettings")]
        public IActionResult UpdateSLASettings([FromBody] BaseQueryModel<SLASetting> queryModel)
        {
            var slaSetting = queryModel.Data;
            try
            {
                var appSettingsPath = Path.Combine(_hostEnvironment.ContentRootPath, "appsettings.json");

                var json = System.IO.File.ReadAllText(appSettingsPath);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                jsonObj["SLA"]["IncludeHoliday"] = slaSetting.IncludeHoliday;
                jsonObj["SLA"]["IncludeStaffLeaves"] = slaSetting.IncludeStaffLeaves;
                jsonObj["SLA"]["IncludeWorkingHours"] = slaSetting.IncludeWorkingHours;
                jsonObj["SLA"]["WorkingStartHour"] = slaSetting.WorkingStartHour;
                jsonObj["SLA"]["WorkingStartMinute"] = slaSetting.WorkingStartMinute;
                jsonObj["SLA"]["WorkingEndHour"] = slaSetting.WorkingEndHour;
                jsonObj["SLA"]["WorkingEndMinute"] = slaSetting.WorkingEndMinute;
                jsonObj["SLA"]["DeadlineInHours"] = slaSetting.DeadlineInHours;

                var updatedJsonString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

                System.IO.File.WriteAllText(appSettingsPath, updatedJsonString);
                ReloadConfiguration();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getslametrics")]
        public async Task<IActionResult> GetSLAMetricsModel()
        {
            var currentUser = await _userManager.FindByIdAsync(_workContext.CurrentUser.Id);
            var salesUsers = await _userManager.GetUsersInRoleAsync("Sales Section Head");

            bool isAdminUser = false;
            bool isSalesSectionHeadUser = false;
            if (salesUsers.Where(x => x.Equals(currentUser)).Count() > 0)
                isSalesSectionHeadUser = true;

            Dictionary<string, string> teamHierarchy = new Dictionary<string, string>();
            List<int> relevantTeamIds = new List<int>();
            List<string> usersInRelevantTeams = new List<string>();
            List<RequestDTO> requests = new List<RequestDTO>();


            if ((await _userManager.IsInRoleAsync(currentUser, RoleEnum.Admin.ToString())))
            {
                isAdminUser = true;
            }
            if (isSalesSectionHeadUser)
            {
                teamHierarchy = _userService.InitializeTeamHierarchy();
                relevantTeamIds = _userService.GetChildTeamIds(teamHierarchy, currentUser.TeamId, _teamRepository);
                relevantTeamIds.Add(currentUser.TeamId);

                usersInRelevantTeams = _userManager.Users
                                                       .Where(u => relevantTeamIds.Contains(u.TeamId))
                                                       .Select(u => u.Id)
                                                       .ToList();

                requests = _requestService.GetRequests()
                    .Where(x => x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction &&
                                x.RequestProducts.Any(rp => usersInRelevantTeams.Contains(rp.FulfillerId)))

                    .ToList();
            }

            var response = new GenericResponseModel<SLAMetricsModel>();

            response.Data.AverageTimeToResolutionInHours = _slaService.GetAverageTimeToResolutionInHours(currentUser, isSalesSectionHeadUser, usersInRelevantTeams, requests, isAdminUser);
            response.Data.TotalTickets = _slaService.GetTotalTicketCount(currentUser, isSalesSectionHeadUser, usersInRelevantTeams, requests, isAdminUser);
            response.Data.BreachedTickets = _slaService.GetBreachedTicketCount(currentUser, isSalesSectionHeadUser, usersInRelevantTeams, requests, isAdminUser);
            response.Data.SuccessRate = _slaService.GetSuccessRateOfTickets(currentUser, isSalesSectionHeadUser, usersInRelevantTeams, requests, isAdminUser);

            return Ok(response);
        }

        private void ReloadConfiguration()
        {
            var configurationRoot = (IConfigurationRoot)_configuration;
            configurationRoot.Reload();
        }
    }
}
