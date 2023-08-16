using AutoMapper;
using Epson.Core.Domain.Products;
using Epson.Data;
using Epson.Model.Categories;
using Epson.Model.Products;
using Epson.Services.DTO.Products;
using Epson.Services.Interface.Categories;
using Epson.Services.Interface.Products;

namespace Epson.Factories
{
    public class ProductModelFactory : IProductModelFactory
    {
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductModelFactory
            (IMapper mapper,
            IProductService productService,
            ICategoryService categoryService)
        {
            _mapper = mapper;
            _productService = productService;
            _categoryService = categoryService;
        }
        public ProductModel PrepareProductModel(ProductDTO product)
        {
            if (product != null)
            {
                var productModel = new ProductModel();
                productModel.Id = product.Id;
                productModel.Name = product.Name;
                productModel.Price = product.Price;
                productModel.UpdatedById = product.UpdatedById;
                productModel.CreatedById = product.CreatedById;
                productModel.CreatedOnUTC = product.CreatedOnUTC;
                productModel.UpdatedOnUTC = product.UpdatedOnUTC;
                productModel.ProductCategoriess = _productService.GetProductCategoriesByProductId(product.Id).Select(pc => new ProductCategoryModel
                {
                    ProductId = pc.ProductId,
                    CategoryId = pc.CategoryId,
                    CategoryName = _categoryService.GetCategoryById(pc.CategoryId).Name
                }).ToList();

                return productModel;
            }

            return new ProductModel();
        }

        public List<ProductModel> PrepareProductModels(List<ProductDTO> products)
        {
            if (products?.Count == 0 || products == null)
                return new List<ProductModel>();

            List<ProductModel> productModels = new List<ProductModel>();
            foreach (var product in products)
            {
                var productModel = new ProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    IsActive = product.IsActive,
                    Status = product.IsActive ? "Active" : "Inactive",
                    UpdatedById = product.UpdatedById,
                    CreatedById = product.CreatedById,
                    CreatedOnUTC = product.CreatedOnUTC,
                    UpdatedOnUTC = product.UpdatedOnUTC,
                    ProductCategoriess = _productService.GetProductCategoriesByProductId(product.Id).Select(pc => new ProductCategoryModel
                    {
                        ProductId = pc.ProductId,
                        CategoryId = pc.CategoryId,
                        CategoryName = _categoryService.GetCategoryById(pc.CategoryId).Name
                    }).ToList()
            };
                productModels.Add(productModel);
            }

            return productModels;
        }
    }
}
