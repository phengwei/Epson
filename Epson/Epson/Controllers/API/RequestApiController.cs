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

namespace Epson.Controllers.API
{
    [Route("api/request")]
    public class RequestApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public RequestApiController(
            IRequestService requestService,
            IProductService productService,
            IUserService userService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _requestService = requestService;
            _productService = productService;
            _userService = userService;
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
            var currentUser = _workContext.CurrentUser;

            List<RequestDTO> requests = new List<RequestDTO>();

            if (currentUser.Roles.Contains("Admin"))
                requests = _requestService.GetRequests();
            else
                requests = _requestService.GetRequests().Where(x => x.CreatedById == currentUser.Id).ToList();

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
                ApprovalState = (int)ApprovalStateEnum.PendingSalesSectionHeadAction,
                Priority = model.Priority,
                Deadline = model.Deadline,
                DealJustification = model.DealJustification,
                CustomerName = model.CustomerName, 
            };

            if (_requestService.InsertRequest(request, model.RequestProducts, model.CompetitorInformations, model.RequestSubmissionDetail, model.ProjectInformationModel))
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

            if (request.ApprovalState != (int)ApprovalStateEnum.AmendQuotation)
                return BadRequest("Request is not in the state of approval!");

            var updatedRequest = new Request
            {
                Id = request.Id,
                CustomerName = model.CustomerName,
                CreatedOnUTC = request.CreatedOnUTC,
                UpdatedOnUTC = DateTime.UtcNow,
                DealJustification = model.DealJustification,
                Deadline = model.Deadline,
                CreatedById = user.Id,
                UpdatedById = user.Id,
                Segment = request.Segment,
                ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction,
                Priority = model.Priority
            };

            if (_requestService.UpdateRequest(updatedRequest, model.RequestProducts, model.CompetitorInformations))
                return Ok();
            else
                return BadRequest("Failed to update request");
        }
           
        [HttpPost("approverequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> ApproveRequest(int id, string comments)
        {
            var request = _requestService.GetRequestById(id);
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Request not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.ApproveRequest(user, _mapper.Map<Request>(request), comments))
                return Ok("Request has been approved");
            else
                return BadRequest("Failed to approve request");
        }

        [HttpPost("fulfillrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> FulfillRequest(int requestId, int productId, decimal fulfilledPrice, DateTime deliveryDate, string remarks)
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

            if (_requestService.FulfillRequest(user, _mapper.Map<Request>(request), _mapper.Map<Product>(product), fulfilledPrice, deliveryDate, remarks))
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

        [HttpPost("approvefirstlevelrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin")]
        public async Task<IActionResult> ApproveFirstLevelRequest(int requestId)
        {
            if (requestId == 0)
                return NotFound("Resources not found!");

            var request = _requestService.GetRequestById(requestId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.ApproveFirstLevelRequest(_mapper.Map<Request>(request)))
                return Ok("Request has been set to amend quotation");
            else
                return BadRequest("Failed to set amend quotation for request");
        }

        [HttpPost("cancelrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales, Admin")]
        public async Task<IActionResult> CancelRequest(int requestId, string remarks)
        {
            if (requestId == 0)
                return NotFound("Resources not found!");

            var request = _requestService.GetRequestById(requestId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.CancelRequest(_mapper.Map<Request>(request), remarks))
                return Ok("Request has been cancelled");
            else
                return BadRequest("Failed to cancel request!");
        }

        [HttpPost("rejectrequestproduct")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales, Admin")]
        public async Task<IActionResult> RejectRequest(int requestProductId, string remarks)
        {
            if (requestProductId == 0)
                return NotFound("Resources not found!");

            var requestProduct = _requestService.GetRequestProducts().Where(x => x.Id == requestProductId).First();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (requestProduct == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.RejectRequest(user, _mapper.Map<RequestProduct>(requestProduct), remarks))
                return Ok("Request has been rejected");
            else
                return BadRequest("Failed to reject request!");
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

        [HttpGet("getfulfilledrequestasfulfiller")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> GetFulfilledRequestAsFulfiller()
        {
            var response = new GenericResponseModel<List<RequestProductModel>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var requestProducts = _requestService.GetRequestProducts()
                                                .Where(x => x.FulfillerId == user.Id)
                                                .ToList();

            var requestModels = _requestModelFactory.PrepareRequestProductModel(requestProducts);

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
                                                                && x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction 
                                                                || x.ApprovalState == (int)ApprovalStateEnum.AmendQuotation)
                                                                    .ToList();

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }

        [HttpGet("getrequestsummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<SalesSummary>>();

            var user = _workContext.CurrentUser;

            var summary = _requestService.GetRequestSummary(startDate, endDate, granularity, user.Id);

            response.Data = summary;

            return Ok(response);
        }

        [HttpGet("getfulfillmentsummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<FulfillmentSummary>>();

            var user = _workContext.CurrentUser;

            var fulfillmentSummary = _requestService.GetFulfillmentSummary(startDate, endDate, granularity, user.Id);

            response.Data = fulfillmentSummary;

            return Ok(response);
        }

        [HttpGet("getpendingfulfilleritem")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product")]
        public async Task<IActionResult> GetPendingFulfillerItem()
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var govtUsers = _userService.GetGovtUsersWithProductRole();

            if (!govtUsers.Any(u => u.Id == user.Id))
                return Unauthorized();

            var coverplusUsers = await _userManager.GetUsersInRoleAsync(RoleEnum.Coverplus.ToString());
            var isCoverplusUser = coverplusUsers.Any(x => x.Id == user.Id);

            var requests = _requestService.GetUnfulfilledRequests(isCoverplusUser);

            var requestModels = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModels;

            return Ok(response);
        }
    }
}
