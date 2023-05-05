using AutoMapper;
using Epson.Core.Domain.AuditTrail;
using Epson.Core.Domain.Users;
using Epson.Factories;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Request;
using Epson.Services.Interface.AuditTrails;
using Epson.Services.Interface.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Epson.Controllers.API
{
    [Route("api/audittrail")]
    public class AuditTrailApiController : BaseApiController
    {
        private readonly IAuditTrailService _auditTrailService;
        private readonly IWorkContext _workContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;


        public AuditTrailApiController(
            IAuditTrailService auditTrailService,
            IWorkContext workContext,
            IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _auditTrailService = auditTrailService;
            _workContext = workContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet("getproductaudittrail")]
        public async Task<IActionResult> GetProductAuditTrails()
        {
            var response = new GenericResponseModel<List<AuditTrail>>();

            var auditTrails = _auditTrailService.GetProductAuditTrails();

            response.Data = auditTrails;
            return Ok(response);
        }
    }
}
