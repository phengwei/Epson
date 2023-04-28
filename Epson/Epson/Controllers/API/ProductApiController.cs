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
        private readonly IWorkContext _workContext;


        public ProductApiController(
            UserManager<ApplicationUser> userManager,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IWorkContext workContext)
        {
            _userManager = userManager;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _workContext = workContext;
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

            _productService.InsertProduct(product);

            return Ok();
        }
    }
}
