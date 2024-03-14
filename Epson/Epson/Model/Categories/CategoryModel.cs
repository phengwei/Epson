using Epson.Core.Domain.Products;
using Epson.Model.Products;

namespace Epson.Model.Categories
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
        public string? BackupFulfiller1 { get; set; }
        public string? BackupFulfiller2 { get; set; }
        public string? EscalationFulfiller { get; set; }
    }
}
