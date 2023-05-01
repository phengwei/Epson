using AutoMapper;
using Epson.Core.Domain.Users;
using Epson.Factories;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Request;
using Epson.Services.Interface.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
    [Route("api/report")]
    public class ReportApiController : BaseApiController
    {
        private readonly IRequestService _requestService;
        private readonly IRequestModelFactory _requestModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;


        public ReportApiController(
            IRequestService requestService,
            IRequestModelFactory requestModelFactory,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _requestService = requestService;
            _requestModelFactory = requestModelFactory;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("getmonthlysalesbyrequester")]
        public async Task<IActionResult> MonthlySalesByRequester(string requesterId)
        {
            var response = new GenericResponseModel<List<RequestModel>>();

            if (string.IsNullOrEmpty(requesterId))
                return BadRequest("Id must not be empty");

            var requests = _requestService.GetRequests().Where(x => x.CreatedById == requesterId).ToList();

            var requestModel = _requestModelFactory.PrepareRequestModels(requests);

            response.Data = requestModel;
            return Ok(response);
        }
    }
}
