using AutoMapper;
using Epson.Core.Domain.Categories;
using Epson.Core.Domain.Email;
using Epson.Core.Domain.Enum;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.SLA;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Data.Context;
using Epson.Services.DTO.Report;
using Epson.Services.DTO.Requests;
using Epson.Services.DTO.SLA;
using Epson.Services.Extensions;
using Epson.Services.Interface.AuditTrails;
using Epson.Services.Interface.Email;
using Epson.Services.Interface.Products;
using Epson.Services.Interface.Requests;
using Epson.Services.Interface.SLA;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using Serilog;
using System.Globalization;
using System.Web.Mvc;

namespace Epson.Services.Services.Requests
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IMapper _mapper;
        private readonly EpsonDbContext _context;
        private readonly IRepository<Request> _RequestRepository;
        private readonly IRepository<RequestProduct> _RequestProductRepository;
        private readonly IRepository<CompetitorInformation> _CompetitorInformationRepository;
        private readonly IRepository<RequestSubmissionDetail> _RequestSubmissionDetailRepository;
        private readonly IRepository<ProjectInformation> _ProjectInformationRepository;
        private readonly IRepository<ProjectInformationReason> _ProjectInformationReasonRepository;
        private readonly IRepository<ServiceRequest> _ServiceRequestRepository;
        private readonly IRepository<ProductCategory> _ProductCategoryRepository;
        private readonly IRepository<Category> _CategoryRepository;
        private readonly IProductService _productService;
        private readonly IAuditTrailService _auditTrailService;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly ISLAService _slaService;
        private readonly IOptions<SLASetting> _slaSetting;
        private readonly ScopedTaskRunner _scopedTaskRunner;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestService
            (IMapper mapper,
            EpsonDbContext dbContext,
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            IRepository<CompetitorInformation> competitorInformationRepository,
            IRepository<RequestSubmissionDetail> requestSubmissionDetailRepository,
            IRepository<ProjectInformation> projectInformationRepository,
            IRepository<ProjectInformationReason> projectInformationReasonRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ServiceRequest> serviceRequestRepository,
            IRepository<Category> categoryRepository,
            IProductService productService,
            IAuditTrailService auditTrailService,
            IEmailService emailService,
            ILogger logger,
            ISLAService slaService,
            IOptions<SLASetting> slaSetting,
            ScopedTaskRunner scopedTaskRunner,
            UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _context = dbContext;
            _RequestRepository = requestRepository;
            _RequestProductRepository = requestProductRepository;
            _CompetitorInformationRepository = competitorInformationRepository;
            _RequestSubmissionDetailRepository = requestSubmissionDetailRepository;
            _ProjectInformationRepository = projectInformationRepository;
            _ProjectInformationReasonRepository = projectInformationReasonRepository;
            _ServiceRequestRepository = serviceRequestRepository;
            _ProductCategoryRepository = productCategoryRepository;
            _CategoryRepository = categoryRepository;
            _productService = productService;
            _auditTrailService = auditTrailService;
            _emailService = emailService;
            _logger = logger;
            _slaService = slaService;
            _slaSetting = slaSetting;
            _scopedTaskRunner = scopedTaskRunner;
            _userManager = userManager;
        }

        public const string Entity = "ServiceRequest";
        public ServiceRequestDTO GetRequestById(int id)
        {
            if (id == 0)
                return new ServiceRequestDTO();

            var request = _context.ServiceRequest
                .FirstOrDefault(r => r.Id == id);

            if (request == null)
                return new ServiceRequestDTO();

            var requestDTO = new ServiceRequestDTO
            {
                id = request.Id,
                serviceRequestNo = request.serviceRequestNo,
                owner = request.owner,
                ownerGroup = request.ownerGroup,
                status = request.status,
                statusDate = request.statusDate,
                fixStatus = request.fixStatus,
                fixStatusDate = request.fixStatusDate,
                paymentStatus = request.paymentStatus,
                paymentStatusDate = request.paymentStatusDate,
                requester = request.requester,
                requesterName = request.requesterName,
                requesterPhone = request.requesterPhone,
                requesterEmail = request.requesterEmail,
                reportedBy = request.reportedBy,
                reportedByPhone = request.reportedByPhone,
                reportedByEmail = request.reportedByEmail,
                reportedByName = request.reportedByName,
                classificationPath = request.classificationPath,
                services = request.services,
                category = request.category,
                section = request.section,
                timeTracking = request.timeTracking,
                summary = request.summary,
                details = request.details,
                reportedDate = request.reportedDate,
                requesterAffectedDate = request.requesterAffectedDate,
                targetFinishDate = request.targetFinishDate,
                standardTAT = request.standardTAT,
                actualStartDate = request.actualStartDate,
                actualFinishDate = request.actualFinishDate,
                workNotes = request.workNotes,
                verificationNotes = request.verificationNotes,
                approvedBy = request.approvedBy,
                approvedByName = request.approvedByName,
                checkedBy = request.checkedBy,
                checkedByName = request.checkedByName,
                serviceRequestStatus = request.serviceRequestStatus,
                createdByID = request.CreatedById,
                createdOnUTC = request.CreatedOnUTC,
                updatedByID = request.UpdatedById,
                updatedOnUTC = request.UpdatedOnUTC,
                timeToResolution = request.timeToResolution,
                isBreached = request.isBreached
            };

            return requestDTO;
        }


        public List<ServiceRequestDTO> GetServiceRequests(string search = null, Func<ServiceRequest, bool> filter = null, int? page = null, int? itemsPerPage = null)
        {
            int totalItems;
            return GetServiceRequests(out totalItems, filter, search, page, itemsPerPage);
        }

        public List<ServiceRequestDTO> GetServiceRequests(out int totalItems, Func<ServiceRequest, bool> filter = null, string search = null, int? page = null, int? itemsPerPage = null)
        {
            var query = _context.ServiceRequest
                .AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter).AsQueryable();
            }

            totalItems = query.Count();

            if (page.HasValue && itemsPerPage.HasValue && itemsPerPage.Value != -1)
            {
                query = query
                    .OrderByDescending(x => x.CreatedOnUTC)
                    .Skip((page.Value - 1) * itemsPerPage.Value)
                    .Take(itemsPerPage.Value);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUTC);
            }

            var requests = query.ToList();

            var requestDTOs = requests.Select(request =>
            {
                return new ServiceRequestDTO
                {
                    id = request.Id,
                    serviceRequestNo = request.serviceRequestNo,
                    owner = request.owner,
                    ownerGroup = request.ownerGroup,
                    status = request.status,
                    statusDate = request.statusDate,
                    fixStatus = request.fixStatus,
                    fixStatusDate = request.fixStatusDate,
                    paymentStatus = request.paymentStatus,
                    paymentStatusDate = request.paymentStatusDate,
                    requester = request.requester,
                    requesterName = request.requesterName,
                    requesterPhone = request.requesterPhone,
                    requesterEmail = request.requesterEmail,
                    reportedBy = request.reportedBy,
                    reportedByPhone = request.reportedByPhone,
                    reportedByEmail = request.reportedByEmail,
                    reportedByName = request.reportedByName,
                    classificationPath = request.classificationPath,
                    services = request.services,
                    category = request.category,
                    section = request.section,
                    timeTracking = request.timeTracking,
                    summary = request.summary,
                    details = request.details,
                    reportedDate = request.reportedDate,
                    requesterAffectedDate = request.requesterAffectedDate,
                    targetFinishDate = request.targetFinishDate,
                    standardTAT = request.standardTAT,
                    actualStartDate = request.actualStartDate,
                    actualFinishDate = request.actualFinishDate,
                    workNotes = request.workNotes,
                    verificationNotes = request.verificationNotes,
                    approvedBy = request.approvedBy,
                    approvedByName = request.approvedByName,
                    createdByID = request.CreatedById,
                    createdOnUTC = request.CreatedOnUTC,
                    updatedByID = request.UpdatedById,
                    updatedOnUTC = request.UpdatedOnUTC,
                    checkedBy = request.checkedBy,
                    checkedByName = request.checkedByName,
                    serviceRequestStatus = request.serviceRequestStatus,
                    timeToResolution = request.timeToResolution,
                    isBreached = request.isBreached
                };
            }).ToList();

            return requestDTOs;
        }

        public bool AssignServiceRequestMaker(int requestId, ApplicationUser user)
        {
            if (requestId == 0 || user == null)
                throw new ArgumentException("Request ID and User ID must be provided.");

            try
            {
                var serviceRequest = _ServiceRequestRepository.GetById(requestId);
                if (serviceRequest == null)
                {
                    _logger.Warning("Service request with ID {requestId} not found.", requestId);
                    return false;
                }

                serviceRequest.status = "INPROG";
                serviceRequest.approvedBy = user.Id;
                serviceRequest.approvedByName = user.UserName;
                serviceRequest.serviceRequestStatus = (int)ServiceRequestStatusEnum.PendingMakerDecision;
                
                // Update entity
                _ServiceRequestRepository.Update(serviceRequest);

                // Log audit trail
                string actionDetails = $"Assigned {user.UserName} (ID: {user.Id}) as Maker for Service Request {requestId}.";
                _auditTrailService.CreateAuditTrail(
                    requestId,
                    "ServiceRequest",
                    DateTime.UtcNow,
                    serviceRequest.UpdatedById,
                    actionDetails,
                    "Assign"
                );

                _logger.Information("Successfully assigned Maker {userId} to Service Request {requestId}.", user.Id, requestId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error assigning Maker {userId} to Service Request {requestId}.", user.Id, requestId);
                return false;
            }
        }


        public bool InsertServiceRequest(ServiceRequestDTO serviceRequestDTO)
        {
            if (serviceRequestDTO == null)
                throw new ArgumentNullException(nameof(serviceRequestDTO));

            try
            {
                var serviceRequestEntity = _mapper.Map<ServiceRequest>(serviceRequestDTO);

                var newId = _ServiceRequestRepository.Add(serviceRequestEntity);

                _logger.Information("Creating service request with Id: {id}", newId);

                string actionDetails = $"{serviceRequestDTO.createdByStr} created service request {newId}";
                _auditTrailService.CreateAuditTrail(
                    newId,
                    "ServiceRequest",
                    DateTime.UtcNow,
                    serviceRequestDTO.createdByID,
                    actionDetails,
                    "Insert"
                );

                _logger.Information("Successfully created service request with Id: {id}", newId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating service request for {serviceRequestNo}", serviceRequestDTO.serviceRequestNo);
                return false;
            }
        }

        public bool MakerServiceRequest(ServiceRequestDTO serviceRequestDTO)
        {
            if (serviceRequestDTO == null)
                throw new ArgumentNullException(nameof(serviceRequestDTO));

            try
            {
                var serviceRequestEntity = _mapper.Map<ServiceRequest>(serviceRequestDTO);

                _ServiceRequestRepository.Update(serviceRequestEntity);

                _logger.Information("Updating service request with Id: {id}", serviceRequestEntity.Id);

                string actionDetails = $"{serviceRequestDTO.updatedByStr} acted on service request {serviceRequestEntity.Id}";
                _auditTrailService.CreateAuditTrail(
                    serviceRequestEntity.Id,
                    "ServiceRequest",
                    DateTime.UtcNow,
                    serviceRequestDTO.updatedByID,
                    actionDetails,
                    "Maker"
                );

                _logger.Information("Maker Successfully acted on service request with Id: {id}", serviceRequestEntity.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating service request for {serviceRequestNo}", serviceRequestDTO.serviceRequestNo);
                return false;
            }
        }

        public bool CheckerServiceRequest(ServiceRequestDTO serviceRequestDTO)
        {
            if (serviceRequestDTO == null)
                throw new ArgumentNullException(nameof(serviceRequestDTO));

            try
            {
                var serviceRequestEntity = _mapper.Map<ServiceRequest>(serviceRequestDTO);

                _ServiceRequestRepository.Update(serviceRequestEntity);

                _logger.Information("Updating service request with Id: {id}", serviceRequestEntity.Id);

                string actionDetails = $"{serviceRequestDTO.updatedByStr} acted on service request {serviceRequestEntity.Id}";
                _auditTrailService.CreateAuditTrail(
                    serviceRequestEntity.Id,
                    "ServiceRequest",
                    DateTime.UtcNow,
                    serviceRequestDTO.updatedByID,
                    actionDetails,
                    "Checker"
                );

                _logger.Information("Checker Successfully acted on service request with Id: {id}", serviceRequestEntity.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating service request for {serviceRequestNo}", serviceRequestDTO.serviceRequestNo);
                return false;
            }
        }

        // 1. Get Average Time to Resolution in Hours for a specific month
        public decimal GetAverageTimeToResolutionInHours(int month)
        {
            var requests = _ServiceRequestRepository.GetAll()
                .Where(x => x.timeToResolution.HasValue &&
                            (month == 0 || x.CreatedOnUTC.Month == month))
                .ToList();

            if (requests.Count == 0)
                return 0; // Avoid division by zero

            return Math.Round((decimal)requests.Average(x => x.timeToResolution.Value.TotalHours), 2);
        }


        // 2. Get Total Ticket Count for a specific month
        public int GetTotalTicketCount(int month)
        {
            return _ServiceRequestRepository.GetAll()
                .Count(x => month == 0 || (x.CreatedOnUTC.Month == month));
        }

        // 3. Get Breached Ticket Count for a specific month
        public int GetBreachedTicketCount(int month)
        {
            return _ServiceRequestRepository.GetAll()
                .Count(x => x.isBreached == true &&
                            (month == 0 || x.CreatedOnUTC.Month == month));
        }


        // 4. Get Success Rate of Tickets for a specific month
        public decimal GetSuccessRateOfTickets(int month)
        {
            int totalTickets = GetTotalTicketCount(month);
            int successfulTickets = _ServiceRequestRepository.GetAll()
                .Count(x => x.serviceRequestStatus == (int)ServiceRequestStatusEnum.Closed &&
                            (month == 0 || x.CreatedOnUTC.Month == month));

            return totalTickets > 0 ? Math.Round((decimal)successfulTickets / totalTickets * 100, 2) : 0;
        }

        public async Task<List<RequesterSales>> GetMonthlySalesByRequester(string requesterId, int month = 0, bool allRequester = false)
        {
            var last12Months = new List<DateTime>();
            for (int i = 0; i < 12; i++)
            {
                last12Months.Add(DateTime.UtcNow.AddMonths(-i));
            }
            last12Months = last12Months.Select(d => new DateTime(d.Year, d.Month, 1)).OrderBy(d => d).ToList();

            var query = _ServiceRequestRepository.Table.AsQueryable();

            if (month != 0)
            {
                query = query.Where(r => r.CreatedOnUTC.Month == month);
            }

            if (!allRequester)
            {
                query = query.Where(r => r.CreatedById == requesterId);
            }

            var salesData = query
                .GroupBy(r => new
                {
                    YearMonth = new DateTime(r.CreatedOnUTC.Year, r.CreatedOnUTC.Month, 1),
                    Requester = r.CreatedById
                })
                .Select(g => new RequesterSales
                {
                    Date = g.Key.YearMonth,
                    RequesterName = _userManager.FindByIdAsync(g.Key.Requester).Result.UserName,
                    MonthlySales = 50,
                    TotalNumberOfSales = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var result = last12Months.Select(m => new RequesterSales
            {
                Date = m,
                RequesterName = salesData.FirstOrDefault(s => s.Date == m)?.RequesterName ?? "",
                MonthlySales = salesData.FirstOrDefault(s => s.Date == m)?.MonthlySales ?? 0,
                TotalNumberOfSales = salesData.FirstOrDefault(s => s.Date == m)?.TotalNumberOfSales ?? 0
            }).ToList();

            return await Task.FromResult(result);

        }

        public async Task<List<RequesterSales>> GetMonthlySalesByRequesterByDonut(DateTime fromMonth, DateTime toMonth, bool allRequester = false)
        {

            var query = _ServiceRequestRepository.Table
                    .Where(r => r.CreatedOnUTC >= fromMonth && r.CreatedOnUTC <= toMonth);

            var groupedData = query
                .GroupBy(r => r.CreatedById)
                .Select(g => new
                {
                    RequesterId = g.Key,
                    MonthlySales = 50,
                    TotalNumberOfSales = g.Count()
                })
                .OrderByDescending(x => x.TotalNumberOfSales)
                .ToList(); // Synchronously fetch the grouped data

            var requesterSalesList = new List<RequesterSales>();

            foreach (var data in groupedData)
            {
                var user = await _userManager.FindByIdAsync(data.RequesterId);
                requesterSalesList.Add(new RequesterSales
                {
                    RequesterId = data.RequesterId,
                    RequesterName = user?.UserName ?? "Unknown",
                    MonthlySales = data.MonthlySales,
                    TotalNumberOfSales = data.TotalNumberOfSales
                });
            }

            return requesterSalesList;
        }

    }
}
