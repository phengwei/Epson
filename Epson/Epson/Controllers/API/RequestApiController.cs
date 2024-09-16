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
        private readonly IRepository<Team> _teamRepository;
        private readonly IConfiguration _configuration;
        private readonly IDraftService _draftService;

        public RequestApiController(
            IRequestService requestService,
            IProductService productService,
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
            _productService = productService;
            _userService = userService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
            _teamRepository = teamRepository;
            _configuration = configuration;
            _draftService = draftService;
        }
        [HttpGet("getrequestbyid")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Product,Admin,Director,Sales Operation,Coverplus,Sales Section Head")]
        public async Task<IActionResult> RequestById(int id)
        {
            var response = new GenericResponseModel<RequestDTO>();

            if (id == null || id == 0)
                return BadRequest("Id must not be empty");

            RequestDTO request = _requestService.GetRequestById(id);
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);
            var approver = await _userManager.FindByIdAsync(request.ApprovedBy);

            if (approver != null)
            {
                request.ApprovedByName = approver.UserName;
            }


            string divisionHeadUserEmail = _userService.GetCoverplusTeamHierarchyEmail();

            bool isDivisionHeadUser = false;
            if (user.Email == divisionHeadUserEmail)
            {
                isDivisionHeadUser = true;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isCoverplusUser = roles.Contains(RoleEnum.Coverplus.ToString());
            var isProductUser = roles.Contains(RoleEnum.Product.ToString());
            var isAdminUser = roles.Contains(RoleEnum.Admin.ToString()) || roles.Contains(RoleEnum.Director.ToString());

            request = _requestService.GetUnfulfilledRequestProducts(request, user, isDivisionHeadUser, isCoverplusUser, isProductUser, isAdminUser);

            //var requestModel = _requestModelFactory.PrepareRequestModel(request);

            response.Data = request;
            return Ok(response);
        }

        [HttpGet("getrequests")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> GetRequests(string search = null, int? page = null, int? itemsPerPage = null, bool breached = false, int month = 0)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();
            var currentUser = _workContext.CurrentUser;
            var currentUserDetail = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            Func<Request, bool> filter = null;
            int totalItems;

            HashSet<RequestDTO> requestSet = new HashSet<RequestDTO>(new RequestDTOComparer());

            Func<Request, bool> monthFilter = x => month == 0 || (x.CreatedOnUTC.Month == month);
            Func<Request, bool> breachedFilter = x => !breached || x.RequestProducts.Any(rp => rp.Breached);

            if (currentUser.Roles.Contains("Admin") || currentUser.Roles.Contains("Director"))
            {
                Func<Request, bool> adminFilter = x => true;
                requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => adminFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
            }
            else
            {
                if (currentUser.Roles.Contains("Sales Operation"))
                {
                    Func<Request, bool> salesOperationFilter = x => x.ApprovalState == (int)ApprovalStateEnum.Approved;
                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => salesOperationFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }

                if (currentUser.Roles.Contains("Sales Section Head"))
                {
                    bool multiRoles = currentUser.Roles.Count > 1;

                    var teamHierarchy = _userService.InitializeTeamHierarchy(true, multiRoles);

                    var relevantTeamIds = _userService.GetChildTeamIds(teamHierarchy, currentUserDetail.TeamId, _teamRepository);
                    relevantTeamIds.Add(currentUserDetail.TeamId);

                    var usersInRelevantTeams = _userManager.Users
                                                            .Where(u => relevantTeamIds.Contains(u.TeamId))
                                                            .Select(u => u.Id)
                                                            .ToList();

                    Func<Request, bool> salesSectionHeadFilter = x => usersInRelevantTeams.Contains(x.CreatedById) &&
                                                                x.CreatedById != currentUser.Id;

                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => salesSectionHeadFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }

                if (currentUser.Roles.Contains("Product") || currentUser.Roles.Contains("Coverplus"))
                {
                    Func<Request, bool> fulfillerFilter = x => x.RequestProducts.Any(rp => rp.FulfillerId == currentUser.Id);

                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => fulfillerFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }

                if (currentUser.Roles.Contains("Sales"))
                {
                    Func<Request, bool> salesFilter = x => x.CreatedById == currentUser.Id;

                    requestSet.UnionWith(_requestService.GetRequests(out totalItems, x => salesFilter(x) && monthFilter(x) && breachedFilter(x), search, page: null, itemsPerPage: null));
                }
            }

            var uniqueRequests = requestSet.ToList().Distinct(new RequestDTOComparer()).ToList();
            int actualTotalItems = uniqueRequests.Count;

            if (page.HasValue && itemsPerPage.HasValue && itemsPerPage.Value != -1)
            {
                uniqueRequests = uniqueRequests
                                    .Skip((page.Value - 1) * itemsPerPage.Value)
                                    .Take(itemsPerPage.Value)
                                    .ToList();
            }

            //var requestModels = await _requestModelFactory.PrepareRequestModelsAsync(uniqueRequests.OrderByDescending(x => x.CreatedOnUTC).ToList());

            response.Data = uniqueRequests;
            response.Count = actualTotalItems;

            return Ok(response);
        }



        public class RequestDTOComparer : IEqualityComparer<RequestDTO>
        {
            public bool Equals(RequestDTO x, RequestDTO y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(RequestDTO obj)
            {
                if (Object.ReferenceEquals(obj, null)) return 0;
                return obj.Id.GetHashCode();
            }
        }

        [HttpGet("getdrafts")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> GetDrafts(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<DraftDTO>>();
            var user = _workContext.CurrentUser;
            int totalItems;

            var drafts = _draftService.GetDrafts(out totalItems, x => x.UserId == user.Id, search, page, itemsPerPage);

            response.Data = drafts;
            response.Count = totalItems;

            return Ok(response);
        }

        [HttpGet("getdraftbyid/{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> GetDraftById(int id)
        {
            if (id == 0)
                return BadRequest("Invalid draft ID!");

            var draft = _draftService.GetDraftById(id);
            if (draft == null)
                return NotFound("Draft not found!");

            return Ok(draft);
        }

        [HttpPost("editdraft")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> EditDraft([FromBody] BaseQueryModel<DraftDTO> queryModel)
        {
            if (queryModel == null || queryModel.Data == null)
            {
                return BadRequest("Invalid draft request.");
            }

            var model = queryModel.Data;

            if (model.Id == 0 || model.Id == null)
                return BadRequest("Draft ID must not be empty!");

            var draft = _draftService.GetDraftById(model.Id);
            if (draft == null)
                return NotFound("Draft not found!");

            var user = _workContext.CurrentUser;
            draft.UpdatedOnUTC = DateTime.UtcNow;
            draft.SelectedCategories = model.SelectedCategories;
            draft.ProductsToShow = model.ProductsToShow;
            draft.CompetitorsToShow = model.CompetitorsToShow;
            draft.CoverplusesToShow = model.CoverplusesToShow;
            draft.SubmissionDetail = model.SubmissionDetail;
            draft.ProjectInformation = model.ProjectInformation;
            draft.Reasons = model.Reasons;
            draft.Priority = model.Priority;
            draft.Comments = model.Comments;
            draft.CustomerName = model.CustomerName;
            draft.DealJustification = model.DealJustification;
            draft.Deadline = model.Deadline;
            draft.SLA = model.SLA;

            if (_draftService.UpdateDraft(draft))
                return Ok();
            else
                return BadRequest("Failed to update draft.");
        }

        [HttpPost("createdraft")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> CreateDraft([FromBody] BaseQueryModel<DraftDTO> queryModel)
        {
            if (queryModel == null || queryModel.Data == null)
            {
                return BadRequest("Invalid draft request.");
            }

            var model = queryModel.Data;
            var user = _workContext.CurrentUser;

            var draft = new DraftDTO
            {
                CreatedOnUTC = DateTime.UtcNow,
                UpdatedOnUTC = DateTime.UtcNow,
                UserId = user.Id,
                SelectedCategories = model.SelectedCategories,
                ProductsToShow = model.ProductsToShow,
                CompetitorsToShow = model.CompetitorsToShow,
                CoverplusesToShow = model.CoverplusesToShow,
                SubmissionDetail = model.SubmissionDetail,
                ProjectInformation = model.ProjectInformation,
                Reasons = model.Reasons,
                Priority = model.Priority,
                Comments = model.Comments,
                CustomerName = model.CustomerName,
                DealJustification = model.DealJustification,
                Deadline = model.Deadline,
                SLA = model.SLA
            };

            if (_draftService.InsertDraft(draft))
                return Ok();
            else
                return BadRequest("Failed to create draft.");
        }

        [HttpPost("savedraft")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> SaveDraft([FromBody] BaseQueryModel<DraftDTO> queryModel)
        {
            if (queryModel == null || queryModel.Data == null)
            {
                return BadRequest("Invalid draft request.");
            }

            var model = queryModel.Data;
            var user = _workContext.CurrentUser;

            var existingDraft = _draftService.GetDraftByUserId(user.Id);

            if (existingDraft != null)
            {
                existingDraft.UpdatedOnUTC = DateTime.UtcNow;
                existingDraft.SelectedCategories = model.SelectedCategories;
                existingDraft.ProductsToShow = model.ProductsToShow;
                existingDraft.CompetitorsToShow = model.CompetitorsToShow;
                existingDraft.CoverplusesToShow = model.CoverplusesToShow;
                existingDraft.SubmissionDetail = model.SubmissionDetail;
                existingDraft.ProjectInformation = model.ProjectInformation;
                existingDraft.Reasons = model.Reasons;
                existingDraft.Priority = model.Priority;
                existingDraft.Comments = model.Comments;
                existingDraft.CustomerName = model.CustomerName;
                existingDraft.DealJustification = model.DealJustification;
                existingDraft.Deadline = model.Deadline;
                existingDraft.SLA = model.SLA;

                if (_draftService.UpdateDraft(existingDraft))
                {
                    return Ok(new { message = "Draft updated successfully" });
                }
                else
                {
                    return BadRequest("Failed to update draft.");
                }
            }
            else
            {
                var newDraft = new DraftDTO
                {
                    UserId = user.Id,
                    CreatedOnUTC = DateTime.UtcNow,
                    UpdatedOnUTC = DateTime.UtcNow,
                    SelectedCategories = model.SelectedCategories,
                    ProductsToShow = model.ProductsToShow,
                    CompetitorsToShow = model.CompetitorsToShow,
                    CoverplusesToShow = model.CoverplusesToShow,
                    SubmissionDetail = model.SubmissionDetail,
                    ProjectInformation = model.ProjectInformation,
                    Reasons = model.Reasons,
                    Priority = model.Priority,
                    Comments = model.Comments,
                    CustomerName = model.CustomerName,
                    DealJustification = model.DealJustification,
                    Deadline = model.Deadline,
                    SLA = model.SLA
                };

                if (_draftService.InsertDraft(newDraft))
                {
                    return Ok(new { message = "Draft created successfully", id = newDraft.Id });
                }
                else
                {
                    return BadRequest("Failed to create draft.");
                }
            }
        }

        [HttpGet("loaddraft")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Admin,Product,Sales Section Head,Coverplus,Sales Operation,Director")]
        public async Task<IActionResult> LoadDraft()
        {
            var user = _workContext.CurrentUser;

            var draft = _draftService.GetDraftByUserId(user.Id);

            if (draft != null)
            {
                return Ok(draft); 
            }
            else
            {
                return NotFound("No draft found for the current user.");
            }
        }


        [HttpPost("createrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Sales Section Head")]
        public async Task<IActionResult> CreateRequest([FromBody] BaseQueryModel<RequestDTO> queryModel)
        {
            if (queryModel == null || queryModel.Data == null)
            {
                return BadRequest("Invalid request.");
            }

            var model = queryModel.Data;

            var user = _workContext.CurrentUser;
            var dbUser = await _userManager.FindByIdAsync(user.Id);

            var request = new RequestDTO
            {
                CreatedOnUTC = DateTime.UtcNow,
                UpdatedOnUTC = DateTime.UtcNow,
                CreatedById = user.Id,
                CreatedByStr = user.Name,
                UpdatedById = user.Id,
                Segment = model.Segment,
                ApprovalState = (int)ApprovalStateEnum.PendingSalesSectionHeadAction,
                TeamId = dbUser.TeamId,
                TeamName = _teamRepository.GetById(dbUser.TeamId).Name,
                sla = model.sla
            };

            if (_requestService.InsertRequest(request, model.RequestProducts, model.CompetitorInformations, model.RequestSubmissionDetail, model.ProjectInformation))
                return Ok();
            else
                return BadRequest("Failed to create request");
        }

        [HttpPost("editrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> EditRequest([FromBody] BaseQueryModel<RequestDTO> queryModel)
        {
            if (queryModel == null || queryModel.Data == null)
            {
                return BadRequest("Invalid request.");
            }
            var model = queryModel.Data;

            if (model.Id == 0 || model.Id == null)
                return BadRequest("Id must not be empty!");

            var request = _requestService.GetRequestById(model.Id);
            var user = _workContext.CurrentUser;

            if (request == null)
                return NotFound("Request not found!");

            if (request.ApprovalState != (int)ApprovalStateEnum.AmendQuotation)
                return BadRequest("Request is not in the state of approval!");

            var updatedRequest = new RequestDTO
            {
                Id = request.Id,
                ApprovedBy = request.ApprovedBy,
                ApprovedTime = request.ApprovedTime,
                AmendQuotationTime = request.AmendQuotationTime,
                CreatedOnUTC = request.CreatedOnUTC,
                UpdatedOnUTC = DateTime.UtcNow,
                CreatedById = user.Id,
                CreatedByStr = user.Name,
                UpdatedById = user.Id,
                Segment = request.Segment,
                ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction,
                Comments = request.Comments,
            };

            if (_requestService.UpdateRequest(updatedRequest, model.RequestProducts, model.CompetitorInformations, model.RequestSubmissionDetail, model.ProjectInformation))
                return Ok();
            else
                return BadRequest("Failed to update request");
        }
           
        [HttpPost("closedeal")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> CloseDeal(int id, string comments, bool isAccept)
        {
            var request = _requestService.GetRequestById(id);
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Request not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            bool isSuccess = false;

            if (isAccept)
                isSuccess = _requestService.AcceptDeal(user, _mapper.Map<Request>(request), comments);
            else
                isSuccess = _requestService.RejectDeal(user, _mapper.Map<Request>(request), comments);

            if (isSuccess)
                return Ok("Deal has been closed");
            else
                return BadRequest("Failed to close deal");
        }
        [HttpPost("exitdeal")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales")]
        public async Task<IActionResult> ExitDeal(int id, string comments)
        {
            var request = _requestService.GetRequestById(id);
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Request not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            bool isSuccess = false;

            isSuccess = _requestService.ExitDeal(user, _mapper.Map<Request>(request), comments);

            if (isSuccess)
                return Ok("Deal has been exited");
            else
                return BadRequest("Failed to exit deal");
        }

        [HttpPost("fulfilldivisioncoverplusrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product,Coverplus")]
        public async Task<IActionResult> FulfillDivisionCoverplusRequest(int id, int productId, string remarks)
        {
            if (id == 0 || productId == 0)
                return NotFound("Resources not found!");

            var requestProduct = _requestService.GetRequestProducts().Where(x => x.Id == id).FirstOrDefault();
            var product = _productService.GetProductById(productId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (requestProduct == null || product == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.FulfillDivisionCoverplusRequest(user, _mapper.Map<RequestProduct>(requestProduct), _mapper.Map<Product>(product), remarks))
                return Ok("Request has been fulfilled");
            else
                return BadRequest("Failed to fulfill request");
        }

        [HttpPost("fulfillrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product,Coverplus,Sales Section Head")]
        public async Task<IActionResult> FulfillRequest(int id, int productId, decimal fulfilledPrice, string remarks)
        {
            if (id == 0 || productId == 0)
                return NotFound("Resources not found!");

            var requestProduct = _requestService.GetRequestProducts().Where(x => x.Id == id).FirstOrDefault();
            var product = _productService.GetProductById(productId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (requestProduct == null || product == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.FulfillRequest(user, _mapper.Map<RequestProduct>(requestProduct), _mapper.Map<Product>(product), fulfilledPrice, remarks))
                return Ok("Request has been fulfilled");
            else
                return BadRequest("Failed to fulfill request");
        }

        [HttpPost("fulfillrequests")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product,Coverplus,Sales Section Head")]
        public async Task<IActionResult> FulfillRequests([FromBody] List<int> ids)
        {
            if (ids.Count == 0)
                return NotFound("Resources not found!");

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.FulfillRequests(ids, user))
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

            if (_requestService.SetRequestToAmendQuotation(_workContext.CurrentUser?.Id, _workContext.CurrentUser.Name, _mapper.Map<Request>(request)))
                return Ok("Request has been set to amend quotation");
            else
                return BadRequest("Failed to set amend quotation for request");
        }

        [HttpPost("approvefirstlevelrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head,Director")]
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

            if (await _requestService.ApproveFirstLevelRequest(_workContext.CurrentUser?.Id, _workContext.CurrentUser?.Name, _mapper.Map<Request>(request)))
                return Ok("Request has been approved to proceed");
            else
                return BadRequest("Failed to set complete first level approval for request");
        }

        [HttpPost("rejectfirstlevelrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head,Director")]
        public async Task<IActionResult> RejectFirstLevelRequest(int requestId)
        {
            if (requestId == 0)
                return NotFound("Resources not found!");

            var request = _requestService.GetRequestById(requestId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.RejectFirstLevelRequest(_workContext.CurrentUser?.Id, _workContext.CurrentUser?.Name, _mapper.Map<Request>(request)))
                return Ok("Request has been rejected");
            else
                return BadRequest("Failed to set complete first level approval for request");
        }

        [HttpPost("approvefinalrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Director")]
        public async Task<IActionResult> ApproveFinalRequest(int requestId, bool isAccept)
        {
            if (requestId == 0)
                return NotFound("Resources not found!");

            var request = _requestService.GetRequestById(requestId);

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            if (request == null)
                return NotFound("Resources not found!");

            if (user == null)
                return Unauthorized("User not authorized to perform this operation");

            if (_requestService.ApproveFinalLevelRequest(_mapper.Map<Request>(request), isAccept))
                return Ok("Request has been approved");
            else
                return BadRequest("Failed to set approved request");
        }

        [HttpPost("cancelrequest")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales, Admin, Director")]
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
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product, Coverplus")]
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
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales, Admin, Director")]
        public async Task<IActionResult> GetPendingRequesterItem(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);
            var isAdminUser = await _userManager.IsInRoleAsync(user, RoleEnum.Admin.ToString()) || await _userManager.IsInRoleAsync(user, RoleEnum.Director.ToString());

            List<RequestDTO> requests = new List<RequestDTO>();
            int totalItems;
            Func<Request, bool> filter;

            if (isAdminUser)
            {
                filter = x => x.ApprovalState == (int)ApprovalStateEnum.Approved ||
                              x.ApprovalState == (int)ApprovalStateEnum.RejectedByFulfiller ||
                              x.ApprovalState == (int)ApprovalStateEnum.RejectedBySalesSectionHead;
            }
            else
            {
                filter = x => x.CreatedById == user.Id &&
                             (x.ApprovalState == (int)ApprovalStateEnum.Approved ||
                              x.ApprovalState == (int)ApprovalStateEnum.RejectedByFulfiller ||
                              x.ApprovalState == (int)ApprovalStateEnum.RejectedBySalesSectionHead);

            }

            requests = _requestService.GetRequests(out totalItems, filter, search, page, itemsPerPage);

            //var requestModels = await _requestModelFactory.PrepareRequestModelsAsync(requests);

            response.Data = requests;
            response.Count = totalItems;

            return Ok(response);
        }


        [HttpGet("getfulfilledrequestasfulfiller")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product, Coverplus, Admin, Director")]
        public async Task<IActionResult> GetFulfilledRequestAsFulfiller(int? page = null, int? itemsPerPage = null, string? search = null)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);
            var isAdminUser = await _userManager.IsInRoleAsync(user, RoleEnum.Admin.ToString()) || await _userManager.IsInRoleAsync(user, RoleEnum.Director.ToString());

            List<RequestDTO> requests;
            int totalItems;

            if (search == null)
            {
                if (isAdminUser)
                {
                    requests = _requestService.GetRequests(
                        out totalItems,
                        r => r.RequestProducts.Any(rp => rp.HasFulfilled),
                        search,
                        page,
                        itemsPerPage
                    );
                }
                else
                {
                    requests = _requestService.GetRequests(
                        out totalItems,
                        r => r.RequestProducts.Any(rp => rp.FulfillerId == user.Id && rp.HasFulfilled == true),
                        search,
                        page,
                        itemsPerPage
                    );
                }

                //requestModels = await _requestModelFactory.PrepareRequestProductModelAsync(requestProducts);
            }
            else
            {
                if (isAdminUser)
                {
                    requests = _requestService.GetRequests(
                        out totalItems,
                        r => r.RequestProducts.Any(rp => rp.HasFulfilled),
                        search,
                        page,
                        itemsPerPage
                    );
                }
                else
                {
                    requests = _requestService.GetRequests(
                        out totalItems,
                        r => r.RequestProducts.Any(rp => rp.HasFulfilled) && r.RequestProducts.Any(rp => rp.FulfillerId == user.Id),
                        search,
                        page,
                        itemsPerPage
                    );
                }

                //var requestProductModels = await _requestModelFactory.PrepareRequestProductModelAsync(requestProducts);
                var requestQueryable = requests.AsQueryable();

                if (page.HasValue && itemsPerPage.HasValue )
                {
                    requestQueryable = requestQueryable.ToList().Where(x =>
                        (x.ProjectInformation.ProjectName != null && x.ProjectInformation.ProjectName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        x.Id.ToString().Contains(search)).AsQueryable();

                    totalItems = requestQueryable.Count();

                    if (itemsPerPage.Value != -1)
                    {
                        requests = requests
                            .OrderByDescending(rp => rp.CreatedOnUTC)
                            .Skip((page.Value - 1) * itemsPerPage.Value)
                            .Take(itemsPerPage.Value).ToList();
                    }
                }
            }


            response.Data = requests;
            response.Count = totalItems;


            return Ok(response);
        }

        [HttpGet("getpendingfulfillmentasrequester")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales, Director")]
        public async Task<IActionResult> GetPendingFulfillmentAsRequester(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);
            var isAdminUser = await _userManager.IsInRoleAsync(user, RoleEnum.Admin.ToString()) || await _userManager.IsInRoleAsync(user, RoleEnum.Director.ToString());

            Func<Request, bool> filter;

            if (isAdminUser)
            {
                filter = x => x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction ||
                              x.ApprovalState == (int)ApprovalStateEnum.AmendQuotation;
            }
            else
            {
                filter = x => (x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction ||
                               x.ApprovalState == (int)ApprovalStateEnum.PendingSalesSectionHeadAction ||
                               x.ApprovalState == (int)ApprovalStateEnum.AmendQuotation) &&
                              x.CreatedById == user.Id;
            }

            int totalItems;
            var requests = _requestService.GetRequests(out totalItems, filter, search, page, itemsPerPage);
            //var requestModels = await _requestModelFactory.PrepareRequestModelsAsync(requests);

            response.Data = requests;
            response.Count = totalItems; 

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
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product, Coverplus")]
        public async Task<IActionResult> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<FulfillmentSummary>>();

            var user = _workContext.CurrentUser;

            var fulfillmentSummary = _requestService.GetFulfillmentSummary(startDate, endDate, granularity, user.Id);

            response.Data = fulfillmentSummary;

            return Ok(response);
        }

        [HttpGet("getpendingfulfilleritem")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product, Coverplus, Admin, Director")]
        public async Task<IActionResult> GetPendingFulfillerItem(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<PagedResult<RequestDTO>>();

            string divisionHeadUserEmail = _userService.GetCoverplusTeamHierarchyEmail();
            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            bool isDivisionHeadUser = false;
            if (user.Email == divisionHeadUserEmail)
            {
                isDivisionHeadUser = true;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isCoverplusUser = roles.Contains(RoleEnum.Coverplus.ToString());
            var isProductUser = roles.Contains(RoleEnum.Product.ToString());
            var isAdminUser = roles.Contains(RoleEnum.Admin.ToString()) || roles.Contains(RoleEnum.Director.ToString());

            var requests = _requestService.GetUnfulfilledRequests(user, isDivisionHeadUser, isCoverplusUser, isProductUser, isAdminUser, search, page, itemsPerPage);

            //var requestModels = await _requestModelFactory.PrepareRequestModelsAsync(requests.Items);

            response.Data = new PagedResult<RequestDTO>
            {
                Items = requests.Items,
                Total = requests.Total
            };

            return Ok(response);
        }



        [HttpGet("getpendingsalessectionheaditem")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin,Director")]
        public async Task<IActionResult> GetPendingSalesSectionHeadItem(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();

            var currentUser = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            bool multiRoles = false;
            if (userRoles.Count > 1)
                multiRoles = true;

            var teamHierarchy = _userService.InitializeTeamHierarchy(true, multiRoles);

            var relevantTeamIds = _userService.GetChildTeamIds(teamHierarchy, currentUser.TeamId, _teamRepository);
            relevantTeamIds.Add(currentUser.TeamId);

            var usersInRelevantTeams = _userManager.Users
                                                   .Where(u => relevantTeamIds.Contains(u.TeamId))
                                                   .Select(u => u.Id)
                                                   .ToList();

            List<RequestDTO> filteredRequestsQuery = new List<RequestDTO>();

            Func<Request, bool> filter;
            int totalItems;

            if (userRoles.Contains("Admin") || userRoles.Contains("Director"))
            {
                filter = x => x.ApprovalState == (int)ApprovalStateEnum.PendingSalesSectionHeadAction;
            }
            else
            {
                filter = x => x.ApprovalState == (int)ApprovalStateEnum.PendingSalesSectionHeadAction &&
                              usersInRelevantTeams.Contains(x.CreatedById) &&
                              x.CreatedById != currentUser.Id;
            }

            filteredRequestsQuery = _requestService.GetRequests(out totalItems, filter, search, page, itemsPerPage)
                                .ToList();

            //var requestModels = await _requestModelFactory.PrepareRequestModelsAsync(filteredRequestsQuery);

            response.Data = filteredRequestsQuery;
            response.Count = totalItems;

            return Ok(response);
        }

        [HttpGet("getpendingsalessectionheaddepartmentrequests")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin,Director")]
        public async Task<IActionResult> GetPendingSalesSectionHeadDepartmentItems(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<RequestDTO>>();

            var currentUser = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var roles = await _userManager.GetRolesAsync(currentUser);
            var isAdminUser = roles.Contains(RoleEnum.Admin.ToString()) || roles.Contains(RoleEnum.Director.ToString());

            var teamHierarchy = _userService.InitializeTeamHierarchy();

            var relevantTeamIds = _userService.GetChildTeamIds(teamHierarchy, currentUser.TeamId, _teamRepository);
            relevantTeamIds.Add(currentUser.TeamId); 

            var usersInRelevantTeams = _userManager.Users
                                                   .Where(u => relevantTeamIds.Contains(u.TeamId))
                                                   .Select(u => u.Id)
                                                   .ToList();

            Func<Request, bool> filter = null;
            int totalItems;

            if (!isAdminUser)
            {
                filter = x => x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction &&
                          (x.RequestProducts.Any(rp => usersInRelevantTeams.Contains(rp.FulfillerId)) ||
                          usersInRelevantTeams.Contains(x.CreatedById));
            }
            else
            {
                filter = x => x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction;
            }

            var requests = _requestService.GetRequests(out totalItems, filter, search, page, itemsPerPage)
                                .ToList();

            //var requestModels = await _requestModelFactory.PrepareRequestModelsAsync(requests);

            response.Data = requests;
            response.Count = totalItems;

            return Ok(response);
        }


        [HttpGet("getcompletedrequests")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin,Director")]
        public async Task<IActionResult> GetCompletedRequests()
        {
            var response = new GenericResponseModel<List<RequestDTO>>();

            var user = await _userManager.FindByIdAsync(_workContext.CurrentUser?.Id);

            var isAdminUser = await _userManager.IsInRoleAsync(user, RoleEnum.Admin.ToString());

            var requests = _requestService.GetRequests().Where(x => x.ApprovalState == (int)ApprovalStateEnum.Approved)
                                                                    .ToList();

            //var requestModels = await _requestModelFactory.PrepareRequestProductModelAsync(requests);

            response.Data = requests;

            return Ok(response);
        }

        [HttpGet("getnumberofrequestssummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin, Director")]
        public async Task<IActionResult> GetNumberOfRequestsSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<NoOfRequestSummary>>();

            var user = _workContext.CurrentUser;

            var requestSummary = _requestService.GetTotalRequestSummary(startDate, endDate, granularity, user.Id);

            response.Data = requestSummary;

            return Ok(response);
        }

        [HttpGet("getnumberofpendingrequestssummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin, Director")]
        public async Task<IActionResult> GetNumberOfPendingRequestsSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<NoOfPendingRequestSummary>>();

            var user = _workContext.CurrentUser;

            var requestSummary = _requestService.GetTotalPendingRequestSummary(startDate, endDate, granularity, user.Id);

            response.Data = requestSummary;

            return Ok(response);
        }

        [HttpGet("getnumberofcompletedrequestssummary")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales Section Head, Admin, Director")]
        public async Task<IActionResult> GetNumberOfCompletedRequestsSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var response = new GenericResponseModel<List<NoOfCompletedRequestSummary>>();

            var user = _workContext.CurrentUser;

            var requestSummary = _requestService.GetTotalCompletedRequestSummary(startDate, endDate, granularity, user.Id);

            response.Data = requestSummary;

            return Ok(response);
        }
    }
}
