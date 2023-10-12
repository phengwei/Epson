using Epson.Data;
using Epson.Core.Domain.Products;
using Epson.Services.Interface.Products;
using AutoMapper;
using Epson.Services.DTO.Products;
using Serilog;
using Epson.Services.Interface.AuditTrails;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Requests;
using Epson.Services.Interface.Categories;

namespace Epson.Services.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Product> _ProductRepository;
        private readonly IRepository<ProductCategory> _ProductCategoryRepository;
        private readonly ILogger _logger;
        private readonly IAuditTrailService _auditTrailService;
        private readonly ICategoryService _categoryService;

        public ProductService
            (IMapper mapper,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository,
            ILogger logger,
            IAuditTrailService auditTrailService,
            ICategoryService categoryService)
        {
            _mapper = mapper;
            _ProductRepository = productRepository;
            _ProductCategoryRepository = productCategoryRepository;
            _logger = logger;
            _auditTrailService = auditTrailService;
            _categoryService = categoryService;
        }

        public const string Entity = "Product";

        public ProductDTO GetProductById(int id)
        {
            if (id == 0 || id == null)
                return new ProductDTO();

            return _mapper.Map<ProductDTO>(_ProductRepository.GetById(id));
        }

        public List<ProductCategory> GetCategoryIdsByProductId(int id)
        {
            if (id == 0 || id == null)
                return new List<ProductCategory>();

            return _ProductCategoryRepository.GetAll().Where(x => x.ProductId == id).ToList();
        }
        public List<ProductDTO> GetProducts()
        {
            var products = _ProductRepository.GetAll();

            var productDTOs = products.Select(x => new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                IsActive = x.IsActive,
                CreatedById = x.CreatedById,
                CreatedOnUTC = x.CreatedOnUTC, 
                UpdatedById = x.UpdatedById,
                UpdatedOnUTC = x.UpdatedOnUTC
            })
            .OrderBy(x => x.Name)
            .ToList();

            return productDTOs;
        }

        public List<ProductDTO> GetProductsByCategory(int categoryId)
        {
            var productCategories = _ProductCategoryRepository.GetAll().Where(x => x.CategoryId == categoryId).ToList();
            var products = _ProductRepository.GetAll();

            var productDTOs = products.Select(x => new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                IsActive = x.IsActive,
                CreatedById = x.CreatedById,
                CreatedOnUTC = x.CreatedOnUTC,
                UpdatedById = x.UpdatedById,
                UpdatedOnUTC = x.UpdatedOnUTC
            })
           .Where(p => productCategories.Any(pc => pc.ProductId == p.Id) && p.IsActive)
            .OrderBy(x => x.Name)
            .ToList();

            return productDTOs;
        }

        public List<ProductCategory> GetProductCategoriesByProductId(int productId)
        {
            return _ProductCategoryRepository.GetAll().Where(x => x.ProductId == productId).ToList();
        }

        public bool InsertProduct(Product product, List<ProductCategory> productCategories, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                product.Id = _ProductRepository.Add(product);
                _logger.Information("Inserting product {ProductName}", product.Name);

                var actionDetails = $"Inserted product {product.Id} of {product.Name} for a price of {product.Price}";
                _auditTrailService.CreateAuditTrail(product.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Insert");

                foreach (var productCategory in productCategories)
                {
                    var category = _categoryService.GetCategoryById(productCategory.CategoryId);

                    if (category == null)
                        return false;

                    productCategory.ProductId = product.Id;
                    productCategory.CategoryId = category.Id;
                    InsertProductCategory(productCategory);
                }

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error inserting product {ProductName}", product.Name);

                return false;
            }
        }

        private bool InsertProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException(nameof(productCategory));

            try
            {
                _ProductCategoryRepository.Add(productCategory);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating product category {id}", productCategory.Id);
                return false;
            }
        }

        public bool UpdateProduct(Product product, List<ProductCategory> productCategories, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var oldProduct = GetProductById(product.Id);

            try
            {
                _ProductRepository.Update(product);
                _logger.Information("Updating product {ProductName}", product.Name);

                DeleteProductCategoriesOfProduct(product.Id);

                foreach (var productCategory in productCategories)
                {
                    productCategory.ProductId = product.Id;

                    InsertProductCategory(productCategory);
                }

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

        private bool DeleteProductCategoriesOfProduct(int productId)
        {
            if (productId == 0 || productId == null)
                return false;

            if (GetProductById(productId) == null)
                return false;

            var productCategories = _ProductCategoryRepository.GetAll().Where(x => x.ProductId == productId).ToList();
            try
            {
                foreach (var productCategory in productCategories)
                    _ProductCategoryRepository.Delete(productCategory.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting product categories of product {productId}", productId);

                return false;
            }
        }

        public bool DeactivateProduct(Product product, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (GetProductById(product.Id) == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                //_ProductRepository.Delete(product.Id);
                product.IsActive = false;
                _ProductRepository.Update(product);
                _logger.Information("Deactivating product {ProductName}", product.Name);

                var actionDetails = $"Deactivated the product {product.Id} ({product.Name})";
                _auditTrailService.CreateAuditTrail(product.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Deactivate");


                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deactivating product {ProductName}", product.Name);

                return false;
            }
        }

        public bool ReactivateProduct(Product product, string userId)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (GetProductById(product.Id) == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                //_ProductRepository.Delete(product.Id);
                product.IsActive = true;
                _ProductRepository.Update(product);
                _logger.Information("Reactivating product {ProductName}", product.Name);

                var actionDetails = $"Reactivated the product {product.Id} ({product.Name})";
                _auditTrailService.CreateAuditTrail(product.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Reactivate");


                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reactivating product {ProductName}", product.Name);

                return false;
            }
        }
    }
}
