using AutoMapper;
using Epson.Core.Domain.Users;
using Epson.Factories;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Request;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Report;
using Epson.Services.Interface.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/report")]
    public class ReportApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IReportService _reportService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;


        public ReportApiController(
            IRequestService requestService,
            IReportService reportService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _requestService = requestService;
            _reportService = reportService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("getmonthlysalesbyrequester")]
        public async Task<IActionResult> MonthlySalesByRequester(string requesterId, int month = 0, bool allRequester = false)
        {
            var response = new GenericResponseModel<List<RequesterSales>>();

            var monthlySalesByRequester = await _reportService.GetMonthlySalesByRequester(requesterId, month, allRequester);

            response.Data = monthlySalesByRequester;
            return Ok(response);
        }

        [HttpGet("getmonthlysalesbyproduct")]
        public async Task<IActionResult> GetMonthlySalesByProduct(int productId, int month = 0)
        {
            var response = new GenericResponseModel<List<ProductRevenue>>();

            var monthlySalesByProduct = await _reportService.GetMonthlySalesByProduct(productId, month);

            response.Data = monthlySalesByProduct;
            return Ok(response);
        }


        [HttpGet("getmonthlysalesbyrequesterbydonut")]
        public async Task<IActionResult> MonthlySalesByRequesterByDonut(DateTime fromMonth, DateTime toMonth)
        {
            var response = new GenericResponseModel<List<RequesterSales>>();

            var monthlySalesByRequester = await _reportService.GetMonthlySalesByRequesterByDonut(fromMonth, toMonth);

            response.Data = monthlySalesByRequester;
            return Ok(response);
        }


        [HttpGet("gettoprequestersbysales")]
        public async Task<IActionResult> TopRequestersBySales(int month)
        {
            var response = new GenericResponseModel<List<RequesterSales>>();

            var monthlySalesByRequester = await _reportService.GetTopRequestersBySales(month);

            response.Data = monthlySalesByRequester;
            return Ok(response);
        }

        [HttpGet("getmonthlysalesbyproductbydonut")]
        public async Task<IActionResult> MonthlySalesByProductByDonut(DateTime fromMonth, DateTime toMonth)
        {
            var response = new GenericResponseModel<List<ProductRevenue>>();

            var monthlySalesByProduct = await _reportService.GetMonthlySalesByProductByDonut(fromMonth, toMonth);

            response.Data = monthlySalesByProduct;
            return Ok(response);
        }



    }
}
