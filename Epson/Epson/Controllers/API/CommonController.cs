using Epson.Services.Interface.Products;
using Epson.Services.Services.Products;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
    [Route("api/[controller]")]
    public class CommonController : BaseApiController
    {
        private readonly IProductService _productService;

        public CommonController(
            IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var product = _productService.GetProduct();
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


    }
}
