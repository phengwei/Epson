﻿using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Epson.Factories;
using Epson.Core.Domain.Requests;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Epson.Services.Interface.Requests;
using Epson.Model.Request;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin")]
    [Route("api/request")]
    public class RequestApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;


        public RequestApiController(
            IRequestService requestService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _requestService = requestService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
        }
        [HttpGet("getrequestbyid")]
        public async Task<IActionResult> RequestById(int id)
        {
            var response = new GenericResponseModel<RequestModel>();

            if (id == null || id == 0)
                return BadRequest("Id must not be empty");

            var request = _requestService.GetRequestById(id);

            var requestModel = _requestModelFactory.PrepareRequestModel(request);

            response.Data = requestModel;
            return Ok(response);
        }

        [HttpGet("getrequests")]
        public async Task<IActionResult> GetRequests()
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            var requests = _requestService.GetRequests();

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }

        [HttpPost("createrequest")]
        public async Task<IActionResult> CreateRequest([FromBody] BaseQueryModel<RequestModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            var manager = await _userManager.FindByIdAsync(model.ManagerId);
            var user = _workContext.CurrentUser;

            var request = new Request
            {
                CreatedOnUTC = DateTime.UtcNow,
                UpdatedOnUTC = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedById = user.Id,
                Segment = model.Segment,
                ManagerId = manager?.Id,
                ManagerName = manager?.UserName,
                Quantity = model.Quantity,
                Priority = model.Priority,
                Deadline = model.Deadline,
                TotalPrice = model.TotalPrice
            };

            if (_requestService.InsertRequest(request, model.RequestProducts))
                return Ok();
            else
                return BadRequest("Failed to create request");
        }

        [HttpPost("editrequest")]
        public async Task<IActionResult> EditRequest([FromBody] BaseQueryModel<RequestModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            if (model.Id == 0 || model.Id == null)
                return BadRequest("Id must not be empty!");

            var request = _requestService.GetRequestById(model.Id);
            var manager = await _userManager.FindByIdAsync(model.ManagerId);
            var user = _workContext.CurrentUser;

            if (request == null)
                return NotFound("Request not found!");

            var updatedRequest = new Request
            {
                Id = request.Id,
                UpdatedOnUTC = DateTime.UtcNow,
                UpdatedById = user.Id,
                Segment = model.Segment,
                ManagerId = manager?.Id,
                ManagerName = manager?.UserName,
                Quantity = model.Quantity,
                Priority = model.Priority,
                Deadline = model.Deadline,
                TotalPrice = model.TotalPrice
            };

            if (_requestService.UpdateRequest(updatedRequest, model.RequestProducts))
                return Ok();
            else
                return BadRequest("Failed to update request");
        }

        [HttpPost("approverequest")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var request = _requestService.GetRequestById(id);
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Request not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.ApproveRequest(user, _mapper.Map<Request>(request)))
                return Ok("Request has been approved");
            else
                return BadRequest("Failed to approve request");

        }
    }
}
