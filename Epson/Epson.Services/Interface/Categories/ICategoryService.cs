using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Categories;
using Epson.Services.DTO.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Categories
{
    public interface ICategoryService
    {
        public CategoryDTO GetCategoryById(int id);
        public List<CategoryDTO> GetCategories();
        public bool InsertCategory(Category category, string userId);
        public bool UpdateCategory(Category category, string userId);
        public bool DeleteCategory(Category category, string userId);
    }
}
