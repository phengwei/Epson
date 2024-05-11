using AutoMapper;
using Epson.Core.Domain.Users;
using Epson.Factories;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Request;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Report;
using Epson.Services.Interface.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
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
        public async Task<IActionResult> MonthlySalesByRequester(string requesterId, bool allRequester = false)
        {
            var response = new GenericResponseModel<List<RequesterSales>>();

            var monthlySalesByRequester = await _reportService.GetMonthlySalesByRequester(requesterId, allRequester);

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
        
        [HttpGet("gettopproductsbyrevenue")]
        public async Task<IActionResult> TopProductsByRevenue(int month)
        {
            var response = new GenericResponseModel<List<ProductRevenue>>();

            var topProductsByRevenue = await _reportService.GetTopProductsByRevenue(month);

            response.Data = topProductsByRevenue;
            return Ok(response);
        }


    }
}
