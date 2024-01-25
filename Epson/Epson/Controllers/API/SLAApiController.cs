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

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin, Sales, Product")]
    [Route("api/sla")]
    public class SLAApiController : BaseApiController
    {
        private readonly ISLAService _slaService;
        private readonly ISLAModelFactory _slaModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;


        public SLAApiController(
            ISLAService slaService,
            ISLAModelFactory slaModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            IConfiguration configuration,
            IWebHostEnvironment hostEnvironment)
        {
            _slaService = slaService;
            _slaModelFactory = slaModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
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
            var currentUserId = _workContext.CurrentUser.Id;
            var response = new GenericResponseModel<SLAMetricsModel>();

            response.Data.AverageTimeToResolutionInHours = _slaService.GetAverageTimeToResolutionInHours(currentUserId);
            response.Data.TotalTickets = _slaService.GetTotalTicketCount(currentUserId);
            response.Data.BreachedTickets = _slaService.GetBreachedTicketCount(currentUserId);
            response.Data.SuccessRate = _slaService.GetSuccessRateOfTickets(currentUserId);

            return Ok(response);
        }

        private void ReloadConfiguration()
        {
            var configurationRoot = (IConfigurationRoot)_configuration;
            configurationRoot.Reload();
        }
    }
}
