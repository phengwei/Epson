﻿using AutoMapper;
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
        private readonly UserManager<ApplicationUser> _userManager;
        public RequestModelFactory
            (IMapper mapper,
            IProductService productService,
            ICategoryService categoryService,
            IRequestService requestService,
            UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _productService = productService;
            _categoryService = categoryService;
            _requestService = requestService;
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
                };

                requestProductModels.Add(requestProductModel);
            }


            return requestProductModels;
        }

        public List<RequestModel> PrepareRequestModels(List<RequestDTO> requests)
        {
            if (requests?.Count == 0 || requests == null)
                return new List<RequestModel>();

            
            List<RequestModel> requestModels = new List<RequestModel>();

            try
            {

                foreach (var request in requests)
                {
                    var requestModel = new RequestModel
                    {
                        Id = request.Id,
                        ApprovedBy = request.ApprovedBy,
                        ApprovedTime = request.ApprovedTime,
                        CreatedBy = _userManager.FindByIdAsync(request.CreatedById).Result.UserName,
                        CreatedById = request.CreatedById,
                        CreatedOnUTC = request.CreatedOnUTC,
                        UpdatedById = request.UpdatedById,
                        UpdatedOnUTC = request.UpdatedOnUTC,
                        Segment = request.Segment,
                        TotalBudget = request.TotalBudget,
                        ApprovalState = request.ApprovalState,
                        ApprovalStateStr = ((ApprovalStateEnum)request.ApprovalState).GetDescription(),
                        TotalPrice = request.TotalPrice,
                        TimeToResolution = request.TimeToResolution,
                        Comments = request.Comments,
                        RequestProductsModel = request.RequestProducts.Select(rp => new RequestProductModel
                        {
                            Id = rp.Id,
                            CreatedOnUTC = rp.CreatedOnUTC,
                            DistyPrice = rp.DistyPrice,
                            DealerPrice = rp.DealerPrice,
                            EndUserPrice = rp.EndUserPrice,
                            RequestId = rp.RequestId,
                            ProductId = rp.ProductId,
                            Quantity = rp.Quantity,
                            ProductName = _productService.GetProductById(rp.ProductId).Name,
                            HasFulfilled = rp.HasFulfilled,
                            FulfilledDate = rp.FulfilledDate,
                            FulfillerId = rp.FulfillerId,
                            FulfillerName = rp.FulfillerId != null ? _userManager.FindByIdAsync(rp.FulfillerId).Result.UserName : null,
                            FulfilledPrice = rp.FulfilledPrice,
                            IsCoverplus = rp.IsCoverplus,
                            TimeToResolution = rp.TimeToResolution,
                            Status = rp.Status,
                            StatusStr = ((RequestProductStatusEnum)rp.Status).GetDescription(),
                            Remarks = rp.Remarks,
                            AuthorizedToFulfill = rp.AuthorizedToFulfill,
                            ProductCategory = _productService.GetProductCategoriesByProductId(rp.ProductId).Select(pc => new ProductCategoryModel
                            {
                                ProductId = pc.ProductId,
                                CategoryId = pc.CategoryId,
                                CategoryName = _categoryService.GetCategoryById(pc.CategoryId).Name
                            }).FirstOrDefault()
                        }).ToList(),
                        CompetitorInformationModel = request.CompetitorInformations.Select(x => new CompetitorInformationModel
                        {
                            Id = x.Id,
                            RequestId = x.RequestId,
                            Model = x.Model,
                            Brand = x.Brand,
                            DistyPrice = x.DistyPrice,
                            DealerPrice = x.DealerPrice,
                            EndUserPrice = x.EndUserPrice,
                        }).ToList(),
                        RequestSubmissionDetailModel = new RequestSubmissionDetailModel
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
                            PreparedBy = _userManager.FindByIdAsync(request.RequestSubmissionDetail.CreatedBy).Result.UserName,
                        },
                        ProjectInformationModel = request.ProjectInformation,
                    };
                    requestModels.Add(requestModel);
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return requestModels;
        }
    }
}
