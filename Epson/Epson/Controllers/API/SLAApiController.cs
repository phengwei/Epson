﻿using Epson.Infrastructure;
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

namespace Epson.Controllers.API
{
    //[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    [Route("api/sla")]
    public class SLAApiController : BaseApiController
    {
        private readonly ISLAService _slaService;
        private readonly ISLAModelFactory _slaModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly IRequestService _requestService;


        public SLAApiController(
            ISLAService slaService,
            ISLAModelFactory slaModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            IRequestService requestService)
        {
            _slaService = slaService;
            _slaModelFactory = slaModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _requestService = requestService;
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
    }
}