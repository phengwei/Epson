using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Requests;
using Epson.Model.Categories;

namespace Epson.Model.Request
{
    public class RequestProductModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Budget { get; set; }
        public string FulfillerId { get; set; }
        public string FulfillerName { get; set; }
        public decimal FulfilledPrice { get; set; }
        public bool HasFulfilled { get; set; }
        public DateTime FulfilledDate { get; set; }
        public ProductCategoryModel ProductCategory { get; set; } = new ProductCategoryModel();
    }
}
