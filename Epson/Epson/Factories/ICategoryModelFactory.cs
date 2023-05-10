using Epson.Model.Categories;
using Epson.Services.DTO.Categories;
using Epson.Services.DTO.Products;

namespace Epson.Factories
{
    public interface ICategoryModelFactory
    {
        public CategoryModel PrepareCategoryModel(CategoryDTO category);
        public List<CategoryModel> PrepareCategoryModels(List<CategoryDTO> categories);
    }
}
