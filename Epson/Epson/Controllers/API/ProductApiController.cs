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

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Product,Admin")]
    [Route("api/product")]
    public class ProductApiController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductApiController(
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("getproductbyid")]
        public async Task<IActionResult> ProductById(int id)
        {
            var response = new GenericResponseModel<ProductModel>();

            if (String.IsNullOrEmpty(id.ToString()))
                return BadRequest();

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
        public async Task<IActionResult> AddProduct(BaseQueryModel<ProductModel> queryModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var model = queryModel.Data;

            var productCopy = new Product();

            productCopy.Id = model.Id;
            productCopy.Name = model.Name;
            productCopy.Price = model.Price;
            productCopy.CreatedOnUTC = DateTime.UtcNow;
            productCopy.UpdatedOnUTC = DateTime.UtcNow;

            _productService.InsertProduct(productCopy);

            return Ok();
        }
    }
}
