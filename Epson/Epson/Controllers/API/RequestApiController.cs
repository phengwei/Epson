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


        public RequestApiController(
            IRequestService requestService,
            IProductService productService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _requestService = requestService;
            _productService = productService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
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

            var request = new Request
            {
                CreatedOnUTC = DateTime.UtcNow,
                UpdatedOnUTC = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedById = user.Id,
                Segment = model.Segment,
                TotalBudget = model.TotalBudget,
                ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction,
                Priority = model.Priority,
                Deadline = (DateTime)model.Deadline
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
        public async Task<IActionResult> FulfillRequest(int requestId, int productId, decimal totalPrice)
        {
            var request = _requestService.GetRequestById(requestId);
            var product = _productService.GetProductById(productId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null || product == null)
                return NotFound("Request not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.FulfillRequest(user, _mapper.Map<Request>(request), _mapper.Map<Product>(product), totalPrice))
                return Ok("Request has been fulfilled");
            else
                return BadRequest("Failed to fulfill request");
        }
    }
}
