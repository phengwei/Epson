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
using Microsoft.AspNetCore.Identity;

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
        public RequestModel PrepareRequestModel(RequestDTO request)
        {
            if (request != null)
            {
                var requestModel = new RequestModel();

                requestModel.Id = request.Id;
                requestModel.ApprovedBy = request.ApprovedBy;
                requestModel.ApprovedTime = request.ApprovedTime;
                requestModel.CreatedById = request.CreatedById;
                requestModel.CreatedOnUTC = request.CreatedOnUTC;
                requestModel.UpdatedById = request.UpdatedById;
                requestModel.UpdatedOnUTC = request.UpdatedOnUTC;
                requestModel.Segment = request.Segment;
                requestModel.TotalBudget = request.TotalBudget;
                requestModel.ApprovalState = request.ApprovalState;
                requestModel.TotalPrice = request.TotalPrice;
                requestModel.TimeToResolution = request.TimeToResolution;
                requestModel.RequestProducts = _mapper.Map<List<RequestProduct>>(request.RequestProducts);

                return requestModel;
            }

            return new RequestModel();
        }

        public List<RequestProductModel> PrepareRequestProductModel(List<RequestProductDTO> requestProducts)
        {
            if (requestProducts?.Count == 0 || requestProducts == null)
                return new List<RequestProductModel>();

            List<RequestProductModel> requestProductModels = new List<RequestProductModel>();
            foreach (var requestProduct in requestProducts)
            {
                var request = _requestService.GetRequestById(requestProduct.RequestId);

                var overallRequestStatusStr = "Pending";

                if (request.ApprovalState == (int)ApprovalStateEnum.Approved)
                {
                    overallRequestStatusStr = "Successful";
                }
                else if ((request.ApprovalState >= (int)ApprovalStateEnum.RejectedByFulfiller) && (request.ApprovalState <= (int)ApprovalStateEnum.DealExited))
                {
                    overallRequestStatusStr = "Failed";
                }

                var requestProductModel = new RequestProductModel
                {
                    Id = requestProduct.Id,
                    RequestedBy = _userManager.FindByIdAsync(_requestService.GetRequestById(requestProduct.RequestId).CreatedById).Result.UserName,
                    ProductId = requestProduct.ProductId,
                    ProductName = _productService.GetProductById(requestProduct.ProductId).Name,
                    RequestId = requestProduct.RequestId,
                    Quantity = requestProduct.Quantity,
                    DistyPrice = requestProduct.DistyPrice,
                    DealerPrice = requestProduct.DealerPrice,
                    EndUserPrice = requestProduct.EndUserPrice,
                    FulfillerId = requestProduct.FulfillerId,
                    FulfillerName = requestProduct.FulfillerId != null ? _userManager.FindByIdAsync(requestProduct.FulfillerId).Result.UserName : null,
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
        public List<RequestModel> PrepareRequestModels(List<RequestDTO> requests)
        {
            if (requests == null || requests.Count == 0)
                return new List<RequestModel>();

            List<RequestModel> requestModels = new List<RequestModel>();

            try
            {
                foreach (var request in requests)
                {
                    var createdByUser = request.CreatedById != null ? _userManager.FindByIdAsync(request.CreatedById).Result : null;
                    var approvedByUser = request.ApprovedBy != null ? _userManager.FindByIdAsync(request.ApprovedBy).Result : null;
                    var createdTeam = createdByUser != null ? _teamRepository.GetById(createdByUser.TeamId) : null;

                    var requestProductsModel = request.RequestProducts?.Select(rp =>
                    {
                        var fulfiller = rp.FulfillerId != null ? _userManager.FindByIdAsync(rp.FulfillerId).Result : null;
                        var product = _productService.GetProductById(rp.ProductId);
                        var productCategories = _productService.GetProductCategoriesByProductId(rp.ProductId);

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
                            ProductCategory = productCategories.Select(pc => new ProductCategoryModel
                            {
                                ProductId = pc.ProductId,
                                CategoryId = pc.CategoryId,
                                CategoryName = _categoryService.GetCategoryById(pc.CategoryId).Name
                            }).FirstOrDefault()
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
                        CreatedTeam = createdTeam?.Name,
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
                            PreparedBy = request.RequestSubmissionDetail.CreatedBy != null ? _userManager.FindByIdAsync(request.RequestSubmissionDetail.CreatedBy).Result?.UserName : null,
                        } : null,
                        ProjectInformationModel = request.ProjectInformation,
                    };

                    requestModels.Add(requestModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return requestModels;
        }


    }
}
