using Epson.Core.Domain.Users;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Epson.Services.Interface.Products;
using Epson.Model.Products;
using Epson.Factories;
using Epson.Services.DTO.Products;
using Epson.Core.Domain.Products;
using AutoMapper;
using Epson.Services.Interface.Email;

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

        [HttpGet("getproducts")]
        public async Task<IActionResult> GetProducts()
        {
            var response = new GenericResponseModel<List<ProductModel>>();

            var products = _productService.GetProduct();

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

            if (_productService.InsertProduct(product))
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
            var user = _workContext.CurrentUser;

            var updatedProduct = new Product
            {
                Id = product.Id,
                Name = model.Name,
                Price = model.Price,
                UpdatedOnUTC = DateTime.UtcNow,
                UpdatedById = user.Id
            };

            if (_productService.UpdateProduct(updatedProduct))
                return Ok();
            else
                return BadRequest("Failed to update product");
        }

        [HttpPost("deleteproduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _productService.GetProductById(id);

            var productToDelete = _mapper.Map<Product>(product);

            if (_productService.DeleteProduct(productToDelete))
                return Ok();
            else
                return BadRequest("Failed to delete product");
        }
    }
}
