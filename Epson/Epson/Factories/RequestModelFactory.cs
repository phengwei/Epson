using AutoMapper;
using Epson.Core.Domain.Enum;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Infrastructure;
using Epson.Model.Categories;
using Epson.Model.Request;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;
using Epson.Services.Interface.Categories;
using Epson.Services.Interface.Products;
using Epson.Services.Interface.Requests;
using LinqToDB;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Epson.Factories
{
    public class RequestModelFactory : IRequestModelFactory
    {
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IRequestService _requestService;
        private readonly IRepository<Team> _teamRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public RequestModelFactory
            (IMapper mapper,
            IProductService productService,
            ICategoryService categoryService,
            IRequestService requestService,
            IRepository<Team> teamRepository,
            UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _productService = productService;
            _categoryService = categoryService;
            _requestService = requestService;
            _teamRepository = teamRepository;
            _userManager = userManager;
        }

        private async Task<List<ApplicationUser>> GetUsersByIdsAsync(List<string> userIds)
        {
            return await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<List<RequestProductModel>> PrepareRequestProductModelAsync(List<RequestProductDTO> requestProducts)
        {
            if (requestProducts == null || requestProducts.Count == 0)
                return new List<RequestProductModel>();

            var requestIds = requestProducts.Select(rp => rp.RequestId).Distinct().ToList();
            var productIds = requestProducts.Select(rp => rp.ProductId).Distinct().ToList();
            var fulfillerIds = requestProducts.Where(rp => rp.FulfillerId != null).Select(rp => rp.FulfillerId).Distinct().ToList();

            var requests = await _requestService.GetRequestsByIdsAsync(requestIds); 
            var products = _productService.GetProductsByIds(productIds);
            var fulfillers = await GetUsersByIdsAsync(fulfillerIds);

            var requestDict = requests.ToDictionary(r => r.Id);
            var productDict = products.ToDictionary(p => p.Id);
            var fulfillerDict = fulfillers.ToDictionary(f => f.Id);

            var requestProductModels = new List<RequestProductModel>();
            foreach (var requestProduct in requestProducts)
            {
                var request = requestDict[requestProduct.RequestId];
                var product = productDict[requestProduct.ProductId];
                var fulfiller = requestProduct.FulfillerId != null ? fulfillerDict[requestProduct.FulfillerId] : null;

                var overallRequestStatusStr = GetOverallRequestStatus(request.ApprovalState);

                var requestProductModel = new RequestProductModel
                {
                    Id = requestProduct.Id,
                    RequestedBy = await GetUserNameAsync(request.CreatedById),
                    ProductId = requestProduct.ProductId,
                    ProductName = product.Name,
                    RequestId = requestProduct.RequestId,
                    Quantity = requestProduct.Quantity,
                    DistyPrice = requestProduct.DistyPrice,
                    DealerPrice = requestProduct.DealerPrice,
                    EndUserPrice = requestProduct.EndUserPrice,
                    FulfillerId = requestProduct.FulfillerId,
                    FulfillerName = fulfiller?.UserName,
                    FulfilledPrice = requestProduct.FulfilledPrice,
                    FulfilledDate = requestProduct.FulfilledDate,
                    HasFulfilled = requestProduct.HasFulfilled,
                    Breached = requestProduct.Breached,
                    IsCoverplus = requestProduct.IsCoverplus,
                    TimeToResolution = requestProduct.TimeToResolution,
                    Status = requestProduct.Status,
                    StatusStr = ((RequestProductStatusEnum)requestProduct.Status).GetDescription(),
                    Remarks = requestProduct.Remarks,
                    OverallRequestStatusStr = overallRequestStatusStr,
                    ProjectName = request.ProjectInformation.ProjectName
                };

                requestProductModels.Add(requestProductModel);
            }

            return requestProductModels;
        }

        private async Task<string> GetUserNameAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.UserName;
        }

        private string GetOverallRequestStatus(int approvalState)
        {
            return approvalState switch
            {
                (int)ApprovalStateEnum.Approved => "Successful",
                >= (int)ApprovalStateEnum.RejectedByFulfiller and <= (int)ApprovalStateEnum.DealExited => "Failed",
                (int)ApprovalStateEnum.AmendQuotation => "Pending amendment",
                _ => "Pending amendment"
            };
        }

        public RequestModel PrepareRequestModel(RequestDTO request)
        {
            if (request == null)
                return null;

            var userIds = new List<string> { request.CreatedById };
            var approverIds = new List<string> { request.ApprovedBy };
            var fulfillerIds = request.RequestProducts.Select(rp => rp.FulfillerId).Where(id => id != null).Distinct().ToList();

            var adminUsers = _userManager.GetUsersInRoleAsync(RoleEnum.Admin.GetDescription()).Result;
            var adminUserIds = adminUsers.Select(u => u.Id).Distinct().ToList();
            var allUserIds = userIds.Concat(approverIds).Concat(fulfillerIds).Concat(adminUserIds).Distinct().ToList();

            var users = _userManager.Users.Where(u => allUserIds.Contains(u.Id)).ToList();
            var userDictionary = users.ToDictionary(u => u.Id, u => u);

            var teamId = request.TeamId;
            var team = _teamRepository.Table.FirstOrDefault(t => t.Id == teamId);
            var teamDictionary = team != null ? new Dictionary<int, string> { { team.Id, team.Name } } : new Dictionary<int, string>();

            var productIds = request.RequestProducts.Select(rp => rp.ProductId).Distinct().ToList();
            var products = _productService.GetProductsByIds(productIds);
            var productCategories = _productService.GetProductCategoriesByProductIds(productIds);

            var productDictionary = products.ToDictionary(p => p.Id, p => p);
            var productCategoryDictionary = productCategories.GroupBy(pc => pc.ProductId)
                                                             .ToDictionary(g => g.Key, g => g.ToList());

            var createdByUser = request.CreatedById != null && userDictionary.ContainsKey(request.CreatedById) ? userDictionary[request.CreatedById] : null;
            var approvedByUser = request.ApprovedBy != null && userDictionary.ContainsKey(request.ApprovedBy) ? userDictionary[request.ApprovedBy] : null;
            var createdTeamName = request.TeamId != null && teamDictionary.ContainsKey(request.TeamId) ? teamDictionary[request.TeamId] : null;

            var requestProductsModel = request.RequestProducts?.Select(rp =>
            {
                var fulfiller = rp.FulfillerId != null && userDictionary.ContainsKey(rp.FulfillerId) ? userDictionary[rp.FulfillerId] : null;
                var product = productDictionary.ContainsKey(rp.ProductId) ? productDictionary[rp.ProductId] : null;
                var productCategoryModels = productCategoryDictionary.ContainsKey(rp.ProductId)
                    ? productCategoryDictionary[rp.ProductId].Select(pc => new ProductCategoryModel
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        CategoryName = _categoryService.GetCategoryById(pc.CategoryId).Name
                    }).ToList()
                    : new List<ProductCategoryModel>();

                return new RequestProductModel
                {
                    Id = rp.Id,
                    Breached = rp.Breached,
                    CreatedOnUTC = rp.CreatedOnUTC,
                    DistyPrice = rp.DistyPrice,
                    DealerPrice = rp.DealerPrice,
                    EndUserPrice = rp.EndUserPrice,
                    RequestId = rp.RequestId,
                    ProductId = rp.ProductId,
                    Quantity = rp.Quantity,
                    ProductName = product?.Name ?? "Unknown Product",
                    HasFulfilled = rp.HasFulfilled,
                    FulfilledDate = rp.FulfilledDate,
                    FulfillerId = rp.FulfillerId,
                    FulfillerName = fulfiller?.UserName,
                    FulfilledPrice = rp.FulfilledPrice,
                    IsCoverplus = rp.IsCoverplus,
                    TimeToResolution = rp.TimeToResolution,
                    Status = rp.Status,
                    StatusStr = ((RequestProductStatusEnum)rp.Status).GetDescription(),
                    Remarks = rp.Remarks,
                    AuthorizedToFulfill = rp.AuthorizedToFulfill,
                    WarrantyRequest = rp.WarrantyRequest,
                    WarrantyRequestPeriod = rp.WarrantyRequestPeriod,
                    ProductCategory = productCategoryModels.FirstOrDefault()
                };
            }).ToList();

            var requestModel = new RequestModel
            {
                Id = request.Id,
                ApprovedBy = request.ApprovedBy,
                ApprovedByName = approvedByUser?.UserName,
                ApprovedTime = request.ApprovedTime,
                AmendQuotationTime = request.AmendQuotationTime,
                CreatedBy = createdByUser?.UserName,
                CreatedById = request.CreatedById,
                CreatedOnUTC = request.CreatedOnUTC,
                CreatedTeam = createdTeamName,
                UpdatedById = request.UpdatedById,
                UpdatedOnUTC = request.UpdatedOnUTC,
                Segment = request.Segment,
                TotalBudget = request.TotalBudget,
                ApprovalState = request.ApprovalState,
                ApprovalStateStr = ((ApprovalStateEnum)request.ApprovalState).GetDescription(),
                TotalPrice = request.TotalPrice,
                TimeToResolution = request.TimeToResolution,
                Comments = request.Comments,
                RequestProductsModel = requestProductsModel ?? new List<RequestProductModel>(),
                CompetitorInformationModel = request.CompetitorInformations?.Select(x => new CompetitorInformationModel
                {
                    Id = x.Id,
                    RequestId = x.RequestId,
                    Model = x.Model,
                    Brand = x.Brand,
                    DistyPrice = x.DistyPrice,
                    DealerPrice = x.DealerPrice,
                    EndUserPrice = x.EndUserPrice,
                }).ToList() ?? new List<CompetitorInformationModel>(),
                RequestSubmissionDetailModel = request.RequestSubmissionDetail != null ? new RequestSubmissionDetailModel
                {
                    Id = request.RequestSubmissionDetail.Id,
                    RequestId = request.Id,
                    DistributorName = request.RequestSubmissionDetail.DistributorName,
                    ResellerName = request.RequestSubmissionDetail.ResellerName,
                    ContactPersonName = request.RequestSubmissionDetail.ContactPersonName,
                    TelephoneNo = request.RequestSubmissionDetail.TelephoneNo,
                    FaxNo = request.RequestSubmissionDetail.FaxNo,
                    Email = request.RequestSubmissionDetail.Email,
                    CreatedOnUTC = request.RequestSubmissionDetail.CreatedOnUTC,
                    CreatedBy = request.RequestSubmissionDetail.CreatedBy,
                    PreparedBy = request.RequestSubmissionDetail.CreatedBy != null ? userDictionary[request.RequestSubmissionDetail.CreatedBy]?.UserName : null,
                } : null,
                ProjectInformationModel = request.ProjectInformation,
            };

            return requestModel;
        }


    }
}
