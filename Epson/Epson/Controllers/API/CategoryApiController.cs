using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Epson.Factories;
using AutoMapper;
using Epson.Services.Interface.Categories;
using Epson.Model.Categories;
using Epson.Core.Domain.Categories;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Sales,Product,Admin,Coverplus,Sales Section Head")]
    [Route("api/category")]
    public class CategoryApiController : BaseApiController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;


        public CategoryApiController(
            ICategoryService categoryService,
            ICategoryModelFactory categoryModelFactory,
            IWorkContext workContext,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _categoryModelFactory = categoryModelFactory;
            _workContext = workContext;
            _mapper = mapper;
        }

        [HttpGet("getcategorybyid")]
        public async Task<IActionResult> CategoryById(int id)
        {
            var response = new GenericResponseModel<CategoryModel>();

            if (id == null || id == 0)
                return BadRequest("Id must not be empty");

            var category = _categoryService.GetCategoryById(id);

            var categoryModel = _categoryModelFactory.PrepareCategoryModel(category);

            response.Data = categoryModel;
            return Ok(response);
        }

        [HttpGet("getcategories")]
        public async Task<IActionResult> GetCategories()
        {
            var response = new GenericResponseModel<List<CategoryModel>>();

            var categories = _categoryService.GetCategories();

            var categoryModels = _categoryModelFactory.PrepareCategoryModels(categories);

            response.Data = categoryModels;

            return Ok(response);
        }
        

        [HttpPost("adcategory")]
        public async Task<IActionResult> AddCategory([FromBody] BaseQueryModel<CategoryModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var model = queryModel.Data;

            var user = _workContext.CurrentUser;

            var category = new Category
            {
                Name = model.Name
            };

            if (_categoryService.InsertCategory(category, user.Id))
                return Ok();
            else
                return BadRequest("Failed to insert category");
        }

        [HttpPost("editcategory")]
        public async Task<IActionResult> EditCategory([FromBody] BaseQueryModel<CategoryModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            if (model.Id == 0 || model.Id == null)
                return BadRequest("Id must not be empty!");
            
            var category = _categoryService.GetCategoryById(model.Id);

            if (category == null)
                return NotFound("Category not found!");

            var user = _workContext.CurrentUser;

            var updatedCategory = new Category
            {
                Id = category.Id,
                Name = model.Name
            };

            if (_categoryService.UpdateCategory(updatedCategory, user.Id))
                return Ok();
            else
                return BadRequest("Failed to update category");
        }

        [HttpPost("deletecategory")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = _categoryService.GetCategoryById(id);

            var categoryToDelete = _mapper.Map<Category>(category);

            var user = _workContext.CurrentUser;

            if (_categoryService.DeleteCategory(categoryToDelete, user.Id))
                return Ok();
            else
                return BadRequest("Failed to delete category");
        }
    }
}
