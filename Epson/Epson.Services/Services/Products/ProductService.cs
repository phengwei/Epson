using Epson.Data;
using Epson.Core.Domain.Products;
using Epson.Services.Interface.Products;
using AutoMapper;
using Epson.Services.DTO.Products;

namespace Epson.Services.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Product> _ProductRepository;

        public ProductService
            (IMapper mapper,
            IRepository<Product> productRepository)
        {
            _mapper = mapper;
            _ProductRepository = productRepository;
        }

        public List<Product> GetProduct()
        {
            var products = _ProductRepository.GetAll();
            var singleProduct = products.FirstOrDefault();

            var r = _mapper.Map<ProductDTO>(singleProduct);

            return products.ToList();
        }
    }
}
