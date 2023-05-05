using Epson.Data;
using Epson.Core.Domain.Products;
using Epson.Services.Interface.Products;
using AutoMapper;
using Epson.Services.DTO.Products;
using Serilog;
using Epson.Services.Interface.AuditTrails;

namespace Epson.Services.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Product> _ProductRepository;
        private readonly ILogger _logger;
        private readonly IAuditTrailService _auditTrailService;

        public ProductService
            (IMapper mapper,
            IRepository<Product> productRepository,
            ILogger logger,
            IAuditTrailService auditTrailService)
        {
            _mapper = mapper;
            _ProductRepository = productRepository;
            _logger = logger;
            _auditTrailService = auditTrailService;
        }

        public const string Entity = "Product";

        public ProductDTO GetProductById(int id)
        {
            if (id == 0 || id == null)
                return new ProductDTO();

            return _mapper.Map<ProductDTO>(_ProductRepository.GetById(id));
        }

        public List<ProductDTO> GetProducts()
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

        public bool InsertProduct(Product product, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                _ProductRepository.Add(product);
                _logger.Information("Inserting product {ProductName}", product.Name);

                var actionDetails = $"Inserted product {product.Id} of {product.Name} for a price of {product.Price}";
                _auditTrailService.CreateAuditTrail(product.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Insert");

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error inserting product {ProductName}", product.Name);

                return false;
            }
        }

        public bool UpdateProduct(Product product, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var oldProduct = GetProductById(product.Id);

            try
            {
                _ProductRepository.Update(product);
                _logger.Information("Updating product {ProductName}", product.Name);

                var actionDetails = $"Changed the following product details : ";

                if (product.Name != oldProduct.Name)
                    actionDetails += $"[Name] from '{oldProduct.Name}' to '{product.Name}' ";

                if (product.Price != oldProduct.Price)
                    actionDetails += $"[Price] from '{oldProduct.Price}' to '{product.Price}' ";

                _auditTrailService.CreateAuditTrail(product.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Update");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating product {ProductName}", product.Name);

                return false;
            }
        }

        public bool DeleteProduct(Product product, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (GetProductById(product.Id) == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                _ProductRepository.Delete(product.Id);
                _logger.Information("Deleting product {ProductName}", product.Name);

                var actionDetails = $"Deleted the product {product.Id} ({product.Name})";
                _auditTrailService.CreateAuditTrail(product.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Delete");


                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting product {ProductName}", product.Name);

                return false;
            }
        }
    }
}
