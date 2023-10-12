using AutoMapper;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Model.Categories;
using Epson.Services.DTO.Categories;
using Epson.Services.Interface.Products;
using Microsoft.AspNetCore.Identity;

namespace Epson.Factories
{
    public class CategoryModelFactory : ICategoryModelFactory
    {
        private readonly IMapper _mapper;
        private readonly IProductService _productService;

        public CategoryModelFactory
            (IMapper mapper,
            IProductService productService)
        {
            _mapper = mapper;
            _productService = productService;
        }
        public CategoryModel PrepareCategoryModel(CategoryDTO category)
        {
            if (category != null)
            {
                var categoryModel = new CategoryModel();
                categoryModel.Id = category.Id;
                categoryModel.Name = category.Name;

                return categoryModel;
            }

            return new CategoryModel();
        }

        public List<CategoryModel> PrepareCategoryModels(List<CategoryDTO> categories)
        {
            if (categories?.Count == 0 || categories == null)
                return new List<CategoryModel>();

            List<CategoryModel> categoryModels = new List<CategoryModel>();
            foreach (var category in categories)
            {
                var categoryModel = new CategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Products = _mapper.Map<List<Product>>(_productService.GetProductsByCategory(category.Id).ToList()),
                    BackupFulfiller1 = category.BackupFulfiller1,
                    BackupFulfiller2 = category.BackupFulfiller2
                };
                categoryModels.Add(categoryModel);
            }

            return categoryModels;
        }
    }
}
