using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Epson.Services.Interface.Products;
using Epson.Model.Products;
using Epson.Factories;
using Epson.Core.Domain.Products;
using AutoMapper;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product,Admin")]
    [Route("api/product")]
    public class ProductApiController : BaseApiController
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;


        public ProductApiController(
            IProductService productService,
            IProductModelFactory productModelFactory,
            IWorkContext workContext,
            IMapper mapper)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _workContext = workContext;
            _mapper = mapper;
        }

        [HttpGet("getproductbyid")]
        public async Task<IActionResult> ProductById(int id)
        {
            var response = new GenericResponseModel<ProductModel>();

            if (id == null || id == 0)
                return BadRequest("Id must not be empty");

            var product = _productService.GetProductById(id);

            var productModel = _productModelFactory.PrepareProductModel(product);

            response.Data = productModel;
            return Ok(response);
        }

        [HttpGet("getproductbycategory")]
        public async Task<IActionResult> GetProductByCategory(int categoryId)
        {
            var response = new GenericResponseModel<List<ProductModel>>();

            if (categoryId == null || categoryId == 0)
                return BadRequest("Id must not be empty");

            var products = _productService.GetProductsByCategory(categoryId);

            var productModel = _productModelFactory.PrepareProductModels(products);

            response.Data = productModel;
            return Ok(response);
        }

        [HttpGet("getproducts")]
        public async Task<IActionResult> GetProducts()
        {
            var response = new GenericResponseModel<List<ProductModel>>();

            var products = _productService.GetProducts();

            var productModels = _productModelFactory.PrepareProductModels(products);

            response.Data = productModels;

            return Ok(response);
        }
        

        [HttpPost("addproduct")]
        public async Task<IActionResult> AddProduct([FromBody] BaseQueryModel<ProductModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var model = queryModel.Data;

            var user = _workContext.CurrentUser;

            var product = new Product
            {
                Name = model.Name,
                Price = model.Price,
                CreatedOnUTC = DateTime.UtcNow,
                UpdatedOnUTC = DateTime.UtcNow,
                CreatedById = user.Id,
                UpdatedById = user.Id
            };

            if (_productService.InsertProduct(product, model.ProductCategories, user.Id))
                return Ok();
            else
                return BadRequest("Failed to insert product");
        }

        [HttpPost("editproduct")]
        public async Task<IActionResult> EditProduct([FromBody] BaseQueryModel<ProductModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var model = queryModel.Data;

            if (model.Id == 0 || model.Id == null)
                return BadRequest("Id must not be empty!");
            
            var product = _productService.GetProductById(model.Id);

            if (product == null)
                return NotFound("Product not found!");

            var user = _workContext.CurrentUser;

            var updatedProduct = new Product
            {
                Id = product.Id,
                Name = model.Name,
                Price = model.Price,
                UpdatedOnUTC = DateTime.UtcNow,
                UpdatedById = user.Id
            };

            if (_productService.UpdateProduct(updatedProduct, model.ProductCategories, user.Id))
                return Ok();
            else
                return BadRequest("Failed to update product");
        }

        [HttpPost("deleteproduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _productService.GetProductById(id);

            var productToDelete = _mapper.Map<Product>(product);

            var user = _workContext.CurrentUser;

            if (_productService.DeleteProduct(productToDelete, user.Id))
                return Ok();
            else
                return BadRequest("Failed to delete product");
        }
    }
}
