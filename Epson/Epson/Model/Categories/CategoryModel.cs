using Epson.Core.Domain.Products;
using Epson.Model.Products;

namespace Epson.Model.Categories
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; }
    }
}
