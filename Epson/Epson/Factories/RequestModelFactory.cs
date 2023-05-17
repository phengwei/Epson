using AutoMapper;
using Epson.Core.Domain.Enum;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Model.Categories;
using Epson.Model.Request;
using Epson.Services.DTO.Products;
using Epson.Services.DTO.Requests;
using Epson.Services.Interface.Categories;
using Epson.Services.Interface.Products;
using Microsoft.AspNetCore.Identity;

namespace Epson.Factories
{
    public class RequestModelFactory : IRequestModelFactory
    {
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        public RequestModelFactory
            (IMapper mapper,
            IProductService productService,
            ICategoryService categoryService,
            UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _productService = productService;
            _categoryService = categoryService;
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
                requestModel.CustomerName = request.CustomerName;
                requestModel.CreatedById = request.CreatedById;
                requestModel.CreatedOnUTC = request.CreatedOnUTC;
                requestModel.DealJustification = request.DealJustification;
                requestModel.UpdatedById = request.UpdatedById;
                requestModel.UpdatedOnUTC = request.UpdatedOnUTC;
                requestModel.Segment = request.Segment;
                requestModel.TotalBudget = request.TotalBudget;
                requestModel.ApprovalState = request.ApprovalState;
                requestModel.Priority = request.Priority;
                requestModel.Deadline = request.Deadline;
                requestModel.TotalPrice = request.TotalPrice;
                requestModel.TimeToResolution = request.TimeToResolution;
                requestModel.RequestProducts = request.RequestProducts;

                return requestModel;
            }

            return new RequestModel();
        }

        public List<RequestModel> PrepareRequestModels(List<RequestDTO> requests)
        {
            if (requests?.Count == 0 || requests == null)
                return new List<RequestModel>();

            List<RequestModel> requestModels = new List<RequestModel>();
            foreach (var request in requests)
            {
                var requestModel = new RequestModel
                {
                    Id = request.Id,
                    ApprovedBy = request.ApprovedBy,
                    ApprovedTime = request.ApprovedTime,
                    CustomerName = request.CustomerName,
                    CreatedBy = _userManager.FindByIdAsync(request.CreatedById).Result.UserName,
                    CreatedById = request.CreatedById,
                    CreatedOnUTC = request.CreatedOnUTC,
                    DealJustification = request.DealJustification,
                    UpdatedById = request.UpdatedById,
                    UpdatedOnUTC = request.UpdatedOnUTC,
                    Segment = request.Segment,
                    TotalBudget = request.TotalBudget,
                    ApprovalState = request.ApprovalState,
                    ApprovalStateStr = ((ApprovalStateEnum)request.ApprovalState).ToString(),
                    Priority = request.Priority,
                    Deadline = request.Deadline,
                    TotalPrice = request.TotalPrice,
                    TimeToResolution = request.TimeToResolution,
                    RequestProductsModel = request.RequestProducts.Select(rp => new RequestProductModel
                    {
                        Id = rp.Id,
                        Budget = rp.Budget,
                        RequestId = rp.RequestId,
                        ProductId = rp.ProductId,
                        Quantity = rp.Quantity,
                        ProductName = _productService.GetProductById(rp.ProductId).Name,
                        FulfilledDate = rp.FulfilledDate,
                        FulfillerId = rp.FulfillerId,
                        FulfillerName = rp.FulfillerId != null ? _userManager.FindByIdAsync(rp.FulfillerId).Result.UserName : null,
                        FulfilledPrice = rp.FulfilledPrice,
                        ProductCategory = _productService.GetProductCategoriesByProductId(rp.ProductId).Select(pc => new ProductCategoryModel
                        {
                            ProductId = pc.ProductId,
                            CategoryId = pc.CategoryId,
                            CategoryName = _categoryService.GetCategoryById(pc.CategoryId).Name
                        }).FirstOrDefault()
                    }).ToList(),
                };
                requestModels.Add(requestModel);
            }

            return requestModels;
        }
    }
}
