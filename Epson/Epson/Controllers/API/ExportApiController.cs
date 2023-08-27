using Epson.Infrastructure;
using Epson.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Epson.Factories;
using Epson.Core.Domain.Requests;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Epson.Services.Interface.Requests;
using Epson.Model.Request;
using Microsoft.AspNetCore.Identity;
using Epson.Core.Domain.Users;
using Epson.Core.Domain.Enum;
using Epson.Services.Interface.Products;
using Epson.Core.Domain.Products;
using Epson.Services.DTO.Requests;
using System.Globalization;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Users;
using Epson.Services.DTO.Products;
using Epson.Model.Products;
using OfficeOpenXml;

namespace Epson.Controllers.API
{
    [Route("api/export")]
    public class ExportApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ExportApiController(
            IRequestService requestService,
            IProductService productService,
            IUserService userService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _requestService = requestService;
            _productService = productService;
            _userService = userService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }
        
        //[HttpPost("export")]
        //public async Task<IActionResult> Export([FromBody] BaseQueryModel<ProductModel> queryModel)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var model = queryModel.Data;

        //    var user = _workContext.CurrentUser;

        //    var request = new Request
        //    {
        //        CreatedOnUTC = DateTime.UtcNow,
        //        UpdatedOnUTC = DateTime.UtcNow,
        //        CreatedById = user.Id,
        //        UpdatedById = user.Id,
        //        Segment = model.Segment,
        //        ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction,
        //    };

        //    if (_requestService.InsertRequest(request, model.RequestProducts, model.CompetitorInformations, model.RequestSubmissionDetail, model.ProjectInformationModel))
        //        return Ok();
        //    else
        //        return BadRequest("Failed to create request");
        //}

        [HttpPost("toExcel")]
        public async Task<IActionResult> ExportToExcel(ProductModel product)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Product Data");

            ws.Cells["A1"].Value = "Name";
            ws.Cells["B1"].Value = product.Name;

            ws.Cells["A2"].Value = "Price";
            ws.Cells["B2"].Value = product.Price;


            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);

            string fileName = "product.xlsx";
            string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            stream.Position = 0;
            return File(stream, fileType, fileName);
        }
    }
}
