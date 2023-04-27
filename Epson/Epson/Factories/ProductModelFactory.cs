using AutoMapper;
using Epson.Core.Domain.Products;
using Epson.Data;
using Epson.Model.Products;
using Epson.Services.DTO.Products;

namespace Epson.Factories
{
    public class ProductModelFactory : IProductModelFactory
    {
        private readonly IMapper _mapper;

        public ProductModelFactory
            (IMapper mapper)
        {
            _mapper = mapper;
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
                    UpdatedById = product.UpdatedById,
                    CreatedById = product.CreatedById,
                    CreatedOnUTC = product.CreatedOnUTC,
                    UpdatedOnUTC = product.UpdatedOnUTC
                };
                productModels.Add(productModel);
            }

            return productModels;
        }
    }
}
