using Epson.Data;
using Epson.Core.Domain.Categories;
using Epson.Services.Interface.Categories;
using AutoMapper;
using Serilog;
using Epson.Services.Interface.AuditTrails;
using Epson.Services.DTO.Categories;

namespace Epson.Services.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Category> _CategoryRepository;
        private readonly ILogger _logger;
        private readonly IAuditTrailService _auditTrailService;

        public CategoryService
            (IMapper mapper,
            IRepository<Category> categoryRepository,
            ILogger logger,
            IAuditTrailService auditTrailService)
        {
            _mapper = mapper;
            _CategoryRepository = categoryRepository;
            _logger = logger;
            _auditTrailService = auditTrailService;
        }

        public const string Entity = "Category";

        public CategoryDTO GetCategoryById(int id)
        {
            if (id == 0 || id == null)
                return new CategoryDTO();

            return _mapper.Map<CategoryDTO>(_CategoryRepository.GetById(id));
        }

        public List<CategoryDTO> GetCategories()
        {
            var categories = _CategoryRepository.GetAll();

            var categoryDTOs = categories.Select(x => new CategoryDTO
            {
                Id = x.Id,
                Name = x.Name,
                BackupFulfiller1 = x.BackupFulfiller1,
                BackupFulfiller2 = x.BackupFulfiller2
            })
            .OrderBy(x => x.Name)
            .ToList();

            return categoryDTOs;
        }

        public bool InsertCategory(Category category, string userId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            try
            {
                _CategoryRepository.Add(category);
                _logger.Information("Inserting category {CategoryName}", category.Name);

                var actionDetails = $"Inserted category {category.Id} of {category.Name}";
                _auditTrailService.CreateAuditTrail(category.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Insert");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inserting category {CategoryName}", category.Name);

                return false;
            }
        }

        public bool UpdateCategory(Category category, string userId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var oldCategory = GetCategoryById(category.Id);

            try
            {
                _CategoryRepository.Update(category);
                _logger.Information("Updating category {CategoryName}", category.Name);

                var actionDetails = $"Changed the following category details : ";

                if (category.Name != oldCategory.Name)
                    actionDetails += $"[Name] from '{oldCategory.Name}' to '{category.Name}' ";

                _auditTrailService.CreateAuditTrail(category.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Update");

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating category {CategoryName}", category.Name);

                return false;
            }
        }

        public bool DeleteCategory(Category category, string userId)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (GetCategoryById(category.Id) == null)
                throw new ArgumentNullException(nameof(category));

            try
            {
                _CategoryRepository.Delete(category.Id);
                _logger.Information("Deleting category {CategoryName}", category.Name);

                var actionDetails = $"Deleted the category {category.Id} ({category.Name})";
                _auditTrailService.CreateAuditTrail(category.Id, Entity, DateTime.UtcNow, userId, actionDetails, "Delete");


                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting category {CategoryName}", category.Name);

                return false;
            }
        }
    }
}
