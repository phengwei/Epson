using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Requests;
using Epson.Model.Categories;
using FluentMigrator.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Epson.Model.Products
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }
        public DateTime? CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public List<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public List<ProductCategoryModel> ProductCategoriess { get; set; } = new List<ProductCategoryModel>();
    }
}
