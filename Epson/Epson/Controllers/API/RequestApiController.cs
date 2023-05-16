using Epson.Infrastructure;
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
using Epson.Core.Domain.Enum;
using Epson.Services.Interface.Products;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Requests;
using System.Globalization;
using Epson.Services.DTO.Report;

namespace Epson.Controllers.API
{
    [Route("api/request")]
    public class RequestApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IProductService _productService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public RequestApiController(
            IRequestService requestService,
            IProductService productService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _requestService = requestService;
            _productService = productService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpGet("getrequestbyid")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Product,Admin")]
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
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product")]
        public async Task<IActionResult> GetRequests()
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            var requests = _requestService.GetRequests();

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }

        [HttpPost("createrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> CreateRequest([FromBody] BaseQueryModel<RequestModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            var user = _workContext.CurrentUser;
            var deadlineInHours = _configuration.GetValue<int>("SLA:DeadlineInHours");

            var request = new Request
            {
                CreatedOnUTC = DateTime.UtcNow,
                UpdatedOnUTC = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedById = user.Id,
                Segment = model.Segment,
                ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction,
                Priority = model.Priority,
                Deadline = DateTime.UtcNow.AddHours(deadlineInHours)
        };

            if (_requestService.InsertRequest(request, model.RequestProducts))
                return Ok();
            else
                return BadRequest("Failed to create request");
        }

        [HttpPost("editrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> EditRequest([FromBody] BaseQueryModel<RequestModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            if (model.Id == 0 || model.Id == null)
                return BadRequest("Id must not be empty!");

            var request = _requestService.GetRequestById(model.Id);
            var user = _workContext.CurrentUser;

            if (request == null)
                return NotFound("Request not found!");

            if (request.ApprovalState == (int)ApprovalStateEnum.AmendQuotation)
                return BadRequest("Request is not in the state of approval!");

            var updatedRequest = new Request
            {
                Id = request.Id,
                UpdatedOnUTC = DateTime.UtcNow,
                UpdatedById = user.Id,
                Segment = model.Segment,
                TotalBudget = model.TotalBudget,
                ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction,
                Priority = model.Priority,
                Deadline = (DateTime)model.Deadline
            };

            if (_requestService.UpdateRequest(updatedRequest, model.RequestProducts))
                return Ok();
            else
                return BadRequest("Failed to update request");
        }

        [HttpPost("approverequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
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

        [HttpPost("fulfillrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> FulfillRequest(int requestId, int productId, decimal fulfilledPrice)
        {
            if (requestId == 0 || productId == 0)
                return NotFound("Resources not found!");

            var request = _requestService.GetRequestById(requestId);
            var product = _productService.GetProductById(productId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null || product == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.FulfillRequest(user, _mapper.Map<Request>(request), _mapper.Map<Product>(product), fulfilledPrice))
                return Ok("Request has been fulfilled");
            else
                return BadRequest("Failed to fulfill request");
        }

        [HttpPost("setrequesttoamendquotation")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales, Admin")]
        public async Task<IActionResult> SetRequestToAmendQuotation(int requestId)
        {
            if (requestId == 0)
                return NotFound("Resources not found!");

            var request = _requestService.GetRequestById(requestId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.SetRequestToAmendQuotation(_mapper.Map<Request>(request)))
                return Ok("Request has been set to amend quotation");
            else
                return BadRequest("Failed to set amend quotation for request");
        }


        [HttpGet("getpendingrequesteritem")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> GetPendingRequesterItem()
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var requests = _requestService.GetRequests().Where(x => x.CreatedById == user.Id 
                                                                && x.ApprovalState == (int)ApprovalStateEnum.PendingRequesterAction)
                                                                    .ToList();

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }

        [HttpGet("getpendingfulfillmentasrequester")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> GetPendingFulfillmentAsRequester()
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var requests = _requestService.GetRequests().Where(x => x.CreatedById == user.Id
                                                                && x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction)
                                                                    .ToList();

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }

        [HttpGet("getrequestsummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            try
            {
                var response = new GenericResponseModel<List<SalesSummary>>();
                var summary = _requestService.GetRequestSummary(startDate, endDate, granularity);
                response.Data = summary;
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getfulfillmentsummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<FulfillmentSummary>>();

            var fulfillmentSummary = _requestService.GetFulfillmentSummary(startDate, endDate, granularity);

            response.Data = fulfillmentSummary;

            return Ok(response);
        }

        [HttpGet("getpendingfulfilleritem")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> GetPendingFulfillerItem()
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var requests = _requestService.GetRequests()
                .Select(x => new RequestDTO
                {
                    Id = x.Id,
                    ApprovedBy = x.ApprovedBy,
                    ApprovedTime = x.ApprovedTime,
                    CreatedById = x.CreatedById,
                    CreatedOnUTC = x.CreatedOnUTC,
                    UpdatedById = x.UpdatedById,
                    UpdatedOnUTC = x.UpdatedOnUTC,
                    Segment = x.Segment,
                    TotalBudget = x.TotalBudget,
                    ApprovalState = x.ApprovalState,
                    Priority = x.Priority,
                    Deadline = x.Deadline,
                    TotalPrice = x.TotalPrice,
                    TimeToResolution = x.TimeToResolution,
                    RequestProducts = x.RequestProducts
                        .Where(rp => rp.FulfillerId == user.Id && rp.HasFulfilled == false)
                        .ToList()
                })
                .Where(x => x.RequestProducts.Any())
                .Where(x => x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction)
                .ToList();

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }
    }
}
