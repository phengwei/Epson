using AutoMapper;
using Epson.Core.Domain.AuditTrail;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Users;
using Epson.Factories;
using Epson.Infrastructure;
using Epson.Model.Common;
using Epson.Model.Request;
using Epson.Services.DTO.Requests;
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

            var auditTrails = _auditTrailService.GetProductAuditTrails().OrderByDescending(x => x.CreatedOnUTC).ToList();

            response.Data = auditTrails;
            return Ok(response);
        }

        [HttpGet("GetServiceRequestAuditTrail")]
        public async Task<IActionResult> GetServiceRequestAuditTrail(int id)
        {
            var response = new GenericResponseModel<List<AuditTrailDTO>>();

            // Fetch audit trails from the service
            var auditTrails = _auditTrailService
                                .GetServiceRequestAuditTrails()
                                .Where(x => x.EntityId == id)
                                .ToList();

            var auditDtos = new List<AuditTrailDTO>();

            foreach (var audit in auditTrails)
            {
                // Manually fetch user details for actorStr
                var user = _userManager.FindByIdAsync(audit.Actor);
                string actorStr = user != null ? user.Result.UserName : "Unknown User"; // Default if user not found

                // Manually map the AuditTrailDTO
                var auditDto = new AuditTrailDTO
                {
                    Id = audit.Id,
                    EntityId = audit.EntityId,
                    Entity = audit.Entity,
                    Action = audit.Action,
                    ActionTime = audit.ActionTime.AddHours(8),
                    Actor = audit.Actor,
                    ActorStr = actorStr, // Assign fetched user name
                    ActionDetails = audit.ActionDetails,
                    CreatedOnUTC = audit.CreatedOnUTC.AddHours(8)
                };

                auditDtos.Add(auditDto);
            }

            response.Data = auditDtos;
            return Ok(response);
        }

        [HttpGet("getrequestaudittrails")]
        public async Task<IActionResult> GetRequestAuditTrails(string search = null, int? page = null, int? itemsPerPage = null)
        {
            var response = new GenericResponseModel<List<AuditTrail>>();

            int totalItems;

            var auditTrails = _auditTrailService.GetRequestAuditTrails(out totalItems, null, search, page, itemsPerPage).Where(x => x.Entity == "Request").OrderByDescending(x => x.CreatedOnUTC).ToList();

            response.Data = auditTrails;
            response.Count = totalItems;

            return Ok(response);
        }
    }
}
