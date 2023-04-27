using Epson.Core.Domain.Products;
using Epson.Model.Products;
using Epson.Services.DTO.Products;

namespace Epson.Factories
{
    public interface IProductModelFactory
    {
        public ProductModel PrepareProductModel(ProductDTO product);
        public List<ProductModel> PrepareProductModels(List<ProductDTO> products);
    }
}
