using AutoMapper;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Model.Categories;
using Epson.Services.DTO.Categories;
using Epson.Services.Interface.Products;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Epson.Factories
{
    public class CategoryModelFactory : ICategoryModelFactory
    {
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly IRepository<ProductCategory> _ProductCategoryRepository;
        private readonly IMemoryCache _memoryCache;

        public CategoryModelFactory
            (IMapper mapper,
            IProductService productService,
            IRepository<ProductCategory> productCategoryRepository,
            IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _productService = productService;
            _ProductCategoryRepository = productCategoryRepository;
            _memoryCache = memoryCache;
        }
        public CategoryModel PrepareCategoryModel(CategoryDTO category)
        {
            if (category != null)
            {
                var categoryModel = new CategoryModel();
                categoryModel.Id = category.Id;
                categoryModel.Name = category.Name;
                categoryModel.Type = category.Type;

                return categoryModel;
            }

            return new CategoryModel();
        }

        public List<CategoryModel> GetValidCategories(List<CategoryDTO> categories)
        {
            const string cacheKey = "validCategories";
            if (!_memoryCache.TryGetValue(cacheKey, out List<CategoryModel> categoryModels))
            {
                categoryModels = PrepareCategoryModels(categories);

                var cacheExpirationOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(24), 
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromMinutes(6)
                };

                _memoryCache.Set(cacheKey, categoryModels, cacheExpirationOptions);
            }

            return categoryModels;
        }

        public List<CategoryModel> PrepareCategoryModels(List<CategoryDTO> categories)
        {
            if (categories?.Count == 0 || categories == null)
                return new List<CategoryModel>();

            var allProducts = _productService.GetProducts().Where(p => p.IsActive);
            var productCategories = _ProductCategoryRepository.GetAll();

            var productsByCategory = allProducts
                .Join(productCategories, p => p.Id, pc => pc.ProductId, (product, pc) => new { product, pc.CategoryId })
                .GroupBy(p => p.CategoryId)
                .ToDictionary(g => g.Key, g => g.Select(x => _mapper.Map<Product>(x.product)).ToList());

            List<CategoryModel> categoryModels = new List<CategoryModel>();
            foreach (var category in categories)
            {
                categoryModels.Add(new CategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Products = productsByCategory.TryGetValue(category.Id, out var products) ? products : new List<Product>(),
                    BackupFulfiller1 = category.BackupFulfiller1,
                    BackupFulfiller2 = category.BackupFulfiller2,
                    EscalationFulfiller = category.EscalationFulfiller,
                    Type = category.Type
                });
            }

            return categoryModels.ToList();
        }

    }
}
