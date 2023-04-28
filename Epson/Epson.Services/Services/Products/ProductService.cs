using Epson.Data;
using Epson.Core.Domain.Products;
using Epson.Services.Interface.Products;
using AutoMapper;
using Epson.Services.DTO.Products;
using Serilog;

namespace Epson.Services.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Product> _ProductRepository;
        private readonly ILogger _logger;

        public ProductService
            (IMapper mapper,
            IRepository<Product> productRepository,
            ILogger logger)
        {
            _mapper = mapper;
            _ProductRepository = productRepository;
            _logger = logger;
        }

        public ProductDTO GetProductById(int id)
        {
            if (id == 0 || id == null)
                return new ProductDTO();

            return _mapper.Map<ProductDTO>(_ProductRepository.GetById(id));
        }

        public List<ProductDTO> GetProduct()
        {
            var products = _ProductRepository.GetAll();

            var productDTOs = products.Select(x => new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                CreatedById = x.CreatedById,
                CreatedOnUTC = x.CreatedOnUTC, 
                UpdatedById = x.UpdatedById,
                UpdatedOnUTC = x.UpdatedOnUTC
            })
            .OrderBy(x => x.Name)
            .ToList();

            return productDTOs;
        }

        public void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                _ProductRepository.Add(product);
                _logger.Information("Inserting product {ProductName}", product.Name);
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error inserting product {ProductName}", product.Name);
            }
            
        }


    }
}
