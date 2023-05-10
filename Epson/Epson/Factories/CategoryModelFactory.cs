using AutoMapper;
using Epson.Core.Domain.Categories;
using Epson.Data;
using Epson.Model.Categories;
using Epson.Services.DTO.Categories;

namespace Epson.Factories
{
    public class CategoryModelFactory : ICategoryModelFactory
    {
        private readonly IMapper _mapper;

        public CategoryModelFactory
            (IMapper mapper)
        {
            _mapper = mapper;
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
                };
                categoryModels.Add(categoryModel);
            }

            return categoryModels;
        }
    }
}
