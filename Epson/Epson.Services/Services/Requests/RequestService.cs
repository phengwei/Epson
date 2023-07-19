using AutoMapper;
using Epson.Core.Domain.Enum;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.SLA;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Services.DTO.Report;
using Epson.Services.DTO.Requests;
using Epson.Services.DTO.SLA;
using Epson.Services.Interface.Email;
using Epson.Services.Interface.Products;
using Epson.Services.Interface.Requests;
using Epson.Services.Interface.SLA;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System.Globalization;

namespace Epson.Services.Services.Requests
{
    public class RequestService : IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Request> _RequestRepository;
        private readonly IRepository<RequestProduct> _RequestProductRepository;
        private readonly IRepository<CompetitorInformation> _CompetitorInformationRepository;
        private readonly IRepository<RequestSubmissionDetail> _RequestSubmissionDetailRepository;
        private readonly IRepository<ProjectInformation> _ProjectInformationRepository;
        private readonly IRepository<ProjectInformationReason> _ProjectInformationReasonRepository;
        private readonly IProductService _productService;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly ISLAService _slaService;
        private readonly IOptions<SLASetting> _slaSetting;

        public RequestService
            (IMapper mapper,
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            IRepository<CompetitorInformation> competitorInformationRepository,
            IRepository<RequestSubmissionDetail> requestSubmissionDetailRepository,
            IRepository<ProjectInformation> projectInformationRepository,
            IRepository<ProjectInformationReason> projectInformationReasonRepository,
            IProductService productService,
            IEmailService emailService,
            ILogger logger,
            ISLAService slaService,
            IOptions<SLASetting> slaSetting)
        {
            _mapper = mapper;
            _RequestRepository = requestRepository;
            _RequestProductRepository = requestProductRepository;
            _CompetitorInformationRepository = competitorInformationRepository;
            _RequestSubmissionDetailRepository = requestSubmissionDetailRepository;
            _ProjectInformationRepository = projectInformationRepository;
            _ProjectInformationReasonRepository = projectInformationReasonRepository;
            _productService = productService;
            _emailService = emailService;
            _logger = logger;
            _slaService = slaService;
            _slaSetting = slaSetting;
        }

        public RequestDTO GetRequestById(int id)
        {
            if (id == 0 || id == null)
                return new RequestDTO();

            var requestDTO = _mapper.Map<RequestDTO>(_RequestRepository.GetById(id));

            requestDTO.RequestProducts = _mapper.Map<List<RequestProductDTO>>(_RequestProductRepository.GetAll().Where(x => x.RequestId == id).ToList());

            return requestDTO;
        }

        public List<RequestDTO> GetRequests()
        {
            var requests = _RequestRepository.GetAll();
            var requestDTOs = requests.Select(x =>
            {
                var projectInformation = _ProjectInformationRepository
                .Table
                .Where(a => a.RequestId == x.Id)
                .FirstOrDefault();

                return new RequestDTO
                {
                    Id = x.Id,
                    ApprovedBy = x.ApprovedBy,
                    ApprovedTime = x.ApprovedTime,
                    CompetitorInformations = _CompetitorInformationRepository.Table.Where(p => p.RequestId == x.Id).ToList(),
                    CreatedById = x.CreatedById,
                    CreatedOnUTC = x.CreatedOnUTC,
                    CustomerName = x.CustomerName,
                    DealJustification = x.DealJustification,
                    UpdatedById = x.UpdatedById,
                    UpdatedOnUTC = x.UpdatedOnUTC,
                    Segment = x.Segment,
                    TotalBudget = x.TotalBudget,
                    ApprovalState = x.ApprovalState,
                    Priority = x.Priority,
                    Deadline = x.Deadline,
                    TotalPrice = x.TotalPrice,
                    TimeToResolution = x.TimeToResolution,
                    Comments = x.Comments,
                    RequestProducts = _mapper.Map<List<RequestProductDTO>>(_RequestProductRepository.GetAll().Where(p => p.RequestId == x.Id).ToList()),
                    RequestSubmissionDetail = _RequestSubmissionDetailRepository.Table.Where(d => d.RequestId == x.Id).FirstOrDefault(),
                    ProjectInformation = new ProjectInformationDTO
                    {
                        Id = projectInformation.Id,
                        RequestId = x.Id,
                        ProjectName = projectInformation.ProjectName,
                        ProjectId = projectInformation.ProjectId,
                        Industry = projectInformation.Industry,
                        Type = projectInformation.Type,
                        ClosingDate = projectInformation.ClosingDate,
                        DeliveryDate = projectInformation.DeliveryDate,
                        CompanyAddress = projectInformation.CompanyAddress,
                        ContactPersonName = projectInformation.ContactPersonName,
                        TelephoneNo = projectInformation.TelephoneNo,
                        Email = projectInformation.Email,
                        Requirements = projectInformation.Requirements,
                        CustomerApplications = projectInformation.CustomerApplications,
                        Budget = projectInformation.Budget,
                        StaggeredDelivery = projectInformation.StaggeredDelivery,
                        OtherInformation = projectInformation.OtherInformation,
                        ProjectInformationReasons = _ProjectInformationReasonRepository
                                                    .Table
                                                    .Where(r => r.ProjectInformationId == projectInformation.Id)
                                                    .ToList(),
                    }
                };
            })
            .OrderBy(x => x.CreatedOnUTC)
            .ToList();

            return requestDTOs;
        }

        public List<RequestDTO> GetUnfulfilledRequests(ApplicationUser user, bool isCoverplusUser, bool isProductUser)
        {
            var requests = GetRequests();

            var unfulfilledRequests = requests
                .Select(x =>
                {
                    x.RequestProducts.ForEach(rp =>
                    {
                        rp.AuthorizedToFulfill = false;

                        if (isCoverplusUser && rp.IsCoverplus)
                        {
                            rp.AuthorizedToFulfill = true;
                        } 
                        else if (isProductUser && rp.FulfillerId == user.Id && !rp.IsCoverplus)
                        {
                            rp.AuthorizedToFulfill = true;
                        }
                    });

                    x.RequestProducts = x.RequestProducts
                        .Where(rp => rp.HasFulfilled == false)
                        .ToList();

                    return x;
                })
                .Where(x => x.RequestProducts.Any() && x.ApprovalState == (int)ApprovalStateEnum.PendingFulfillerAction)
                .ToList();

            return unfulfilledRequests;
        }


        public List<RequestProductDTO> GetRequestProducts()
        {
            var requestProducts = _RequestProductRepository.GetAll();

            var requestProductDTOs = requestProducts.Select(x => new RequestProductDTO
            {
                Id = x.Id,
                RequestId = x.RequestId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Budget = x.Budget,
                FulfillerId = x.FulfillerId,
                FulfilledPrice = x.FulfilledPrice,
                FulfilledDate = x.FulfilledDate,
                HasFulfilled = x.HasFulfilled,
                TenderDate = x.TenderDate,
                DeliveryDate = x.DeliveryDate,
                TimeToResolution = x.TimeToResolution,
                Status = x.Status,
                Remarks = x.Remarks,
                IsCoverplus = x.IsCoverplus
            })
            .ToList();

            return requestProductDTOs;
        }

        public bool InsertRequest(Request request,
            List<RequestProduct> requestProducts, 
            List<CompetitorInformation> competitorInformations, 
            RequestSubmissionDetail requestSubmissionDetail,
            ProjectInformationDTO projectInformationDTO)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (requestSubmissionDetail == null)
                throw new ArgumentNullException(nameof(requestSubmissionDetail));

            var projectInformation = _mapper.Map<ProjectInformation>(projectInformationDTO);

            try
            {
                request.TotalBudget = GetTotalBudgetOfRequestProducts(requestProducts);
                request.Id = _RequestRepository.Add(request);
                requestSubmissionDetail.RequestId = request.Id;
                projectInformation.RequestId = request.Id;
                
                _logger.Information("Creating request {id}", request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    var product = _productService.GetProductById(requestProduct.ProductId);
                    requestProduct.FulfillerId = product.CreatedById;
                    requestProduct.CreatedOnUTC = request.CreatedOnUTC;
                    requestProduct.UpdatedOnUTC = request.UpdatedOnUTC;
                    requestProduct.RequestId = request.Id;
                    InsertRequestProduct(requestProduct);
                }

                foreach (var competitorInformation in competitorInformations)
                {
                    competitorInformation.RequestId = request.Id;
                    InsertCompetitorInformation(competitorInformation);
                }
                
                var requestQueue = _emailService.CreateRequestEmailQueue(request, requestProducts);
                _emailService.InsertEmailQueue(requestQueue);

                var projectInformationId = _ProjectInformationRepository.Add(projectInformation);

                foreach (var projectInformationReason in projectInformationDTO.ProjectInformationReasons)
                {
                    projectInformationReason.ProjectInformationId = projectInformationId;
                    _ProjectInformationReasonRepository.Add(projectInformationReason);
                }

                requestSubmissionDetail.CreatedBy = request.CreatedById;
                _RequestSubmissionDetailRepository.Add(requestSubmissionDetail);
                _logger.Information("Successfully created request {id}", request.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating request {id}", request.Id);

                return false;
            }
        }

        private decimal GetTotalBudgetOfRequestProducts(List<RequestProduct> requestProducts)
        {
            decimal totalBudget = 0;

            foreach (var requestProduct in requestProducts)
                totalBudget += requestProduct.Budget;

            return totalBudget;
        }
        private bool InsertRequestProduct(RequestProduct requestProduct)
        {
            if (requestProduct == null)
                throw new ArgumentNullException(nameof(requestProduct));

            try
            {
                _RequestProductRepository.Add(requestProduct);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating product request {id}", requestProduct.Id);
                return false;
            }
        }


        private bool InsertCompetitorInformation(CompetitorInformation competitorInformation)
        {
            if (competitorInformation == null)
                throw new ArgumentNullException(nameof(competitorInformation));

            try
            {
                _CompetitorInformationRepository.Add(competitorInformation);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating competitor information {id}", competitorInformation.Id);
                return false;
            }
        }

        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts, List<CompetitorInformation> competitorInformations)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));
            try
            {
                request.TotalBudget = GetTotalBudgetOfRequestProducts(requestProducts);
                _RequestRepository.Update(request);
                _logger.Information("Updating request {id}", request.Id);

                DeleteRequestProductOfRequest(request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    var product = _productService.GetProductById(requestProduct.ProductId);

                    requestProduct.FulfillerId = null;
                    requestProduct.RequestId = request.Id;
                    requestProduct.CreatedOnUTC = request.CreatedOnUTC;
                    requestProduct.UpdatedOnUTC = request.UpdatedOnUTC;

                    InsertRequestProduct(requestProduct);
                }

                DeleteCompetitorInformationOfRequest(request.Id);

                foreach (var competitorInformation in competitorInformations)
                {
                    competitorInformation.RequestId = request.Id;

                    InsertCompetitorInformation(competitorInformation);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating request {id}", request.Id);

                return false;
            }
        }

        private bool DeleteRequestProductOfRequest(int requestId)
        {
            if (requestId == 0 || requestId == null)
                return false;

            if (GetRequestById(requestId) == null)
                return false;

            var requestProducts = _RequestProductRepository.GetAll().Where(x => x.RequestId == requestId).ToList();
            try
            {
                foreach (var requestProduct in requestProducts)
                    _RequestProductRepository.Delete(requestProduct.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting request products of request {requestid}", requestId);

                return false;
            }
        }

        private bool DeleteCompetitorInformationOfRequest(int requestId)
        {
            if (requestId == 0 || requestId == null)
                return false;

            if (GetRequestById(requestId) == null)
                return false;

            var competitorInformations = _CompetitorInformationRepository.GetAll().Where(x => x.RequestId == requestId).ToList();
            try
            {
                foreach (var competitorInformation in competitorInformations)
                    _CompetitorInformationRepository.Delete(competitorInformation.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting competitor information of request {requestid}", requestId);

                return false;
            }
        }

        public bool ApproveRequest(ApplicationUser user, Request request, string comments)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ApprovalState != (int)ApprovalStateEnum.PendingRequesterAction)
                return false;

            var projectInformation = _ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault();

            request.ApprovalState = (int)ApprovalStateEnum.Approved;
            request.ApprovedBy = user.Id;
            request.ApprovedTime = DateTime.UtcNow;
            request.UpdatedOnUTC = DateTime.UtcNow;
            request.UpdatedById = user.Id;
            request.TimeToResolution = CalculateResolutionTime(request.ApprovedTime, request.CreatedOnUTC, _slaService.GetSLAStaffLeavesByStaffId(user.Id), _slaService.GetSLAHolidays());
            request.Comments = comments;

            if (DateTime.UtcNow > projectInformation.ClosingDate)
                request.Breached = true;

            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Approving request {id}", request.Id);

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error approving request {requestid}", request.Id);
                return false;
            }
        }

        public bool FulfillRequest(ApplicationUser user, Request request, Product product, decimal totalPrice, DateTime deliveryDate, string remarks)
        {
            var existingRequest = GetRequestById(request.Id);
            var existingProduct = _productService.GetProductById(product.Id);

            if (existingRequest == null || existingProduct == null || request.ApprovalState != (int)ApprovalStateEnum.PendingFulfillerAction)
                return false;

            var requestProducts = _RequestProductRepository.GetAll().Where(x => x.RequestId == request.Id).ToList();
            var requestProductToFulfill = requestProducts.FirstOrDefault(x => x.ProductId == product.Id);

            if (requestProductToFulfill == null)
                return false;

            var projectInformation = _ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault();

            requestProductToFulfill.FulfilledPrice = totalPrice;
            requestProductToFulfill.FulfillerId = user.Id;
            requestProductToFulfill.HasFulfilled = true;
            requestProductToFulfill.FulfilledDate = DateTime.UtcNow;
            requestProductToFulfill.DeliveryDate = deliveryDate;
            requestProductToFulfill.UpdatedOnUTC = DateTime.UtcNow;
            requestProductToFulfill.TimeToResolution = CalculateResolutionTime(requestProductToFulfill.FulfilledDate, requestProductToFulfill.CreatedOnUTC, _slaService.GetSLAStaffLeavesByStaffId(user.Id), _slaService.GetSLAHolidays());
            requestProductToFulfill.Remarks = remarks;

            if (DateTime.UtcNow > projectInformation.ClosingDate)
                requestProductToFulfill.Breached = true;

            try
            {
                _RequestProductRepository.Update(requestProductToFulfill);
                _logger.Information("Fulfilling request product {id}", requestProductToFulfill.Id);

                //calculates total price for all requested products
                decimal totalUpdatedPrice = requestProducts.Sum(x => x.FulfilledPrice);
                request.TotalPrice = totalUpdatedPrice;

                //checks for all hasfulfilled field 
                bool allProductsFulfilled = requestProducts.All(x => x.HasFulfilled == true);

                if (allProductsFulfilled)
                    request.ApprovalState = (int)ApprovalStateEnum.PendingRequesterAction;

                _RequestRepository.Update(request);

                var updatedRequest = _mapper.Map<Request>(GetRequestById(request.Id));
                var fulfillRequestQueue = _emailService.CreateFulfillEmailQueue(updatedRequest, requestProductToFulfill, allProductsFulfilled);
                _emailService.InsertEmailQueue(fulfillRequestQueue);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fulfilling request {id}", requestProductToFulfill.Id);
                return false;
            }
        }

        public bool SetRequestToAmendQuotation(Request request)
        {
            var req = GetRequestById(request.Id);

            if (req == null)
                throw new Exception("Invalid request.");

            request.ApprovalState = (int)ApprovalStateEnum.AmendQuotation;

            List<RequestProduct> requestProducts = _RequestProductRepository.Table.Where(x => x.RequestId == req.Id).ToList();

            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Setting Approval state to amend quotation of request {id}", request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    if (requestProduct.HasFulfilled == true)
                    {
                        var amendQuotationEmailQueue = _emailService.CreateAmendQuotationEmailQueue(request, requestProduct);
                        _emailService.InsertEmailQueue(amendQuotationEmailQueue);
                        requestProduct.HasFulfilled = false;
                        requestProduct.FulfilledDate = DateTime.MinValue;
                        requestProduct.FulfillerId = string.Empty;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting approval state to amend quotation for request {id}", request.Id);
                return false;
            }
        }

        public bool ApproveFirstLevelRequest(Request request)
        {
            var req = GetRequestById(request.Id);

            if (req == null)
                throw new Exception("Invalid request.");

            request.ApprovalState = (int)ApprovalStateEnum.PendingFulfillerAction;

            List<RequestProduct> requestProducts = _RequestProductRepository.Table.Where(x => x.RequestId == req.Id).ToList();

            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Completing first level approval for request {id}", request.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error completing first level approval for request {id}", request.Id);
                return false;
            }
        }

        public bool CancelRequest(Request request, string remarks)
        {
            var req = GetRequestById(request.Id);

            if (req == null)
                throw new Exception("Invalid request.");

            request.ApprovalState = (int)ApprovalStateEnum.Cancelled;
            request.Comments = remarks;

            List<RequestProduct> requestProducts = _RequestProductRepository.Table.Where(x => x.RequestId == req.Id).ToList();

            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Cancelled request {id}", request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    if (requestProduct.HasFulfilled == true)
                    {
                        var cancellationEmailQueue = _emailService.CreateCancellationEmailQueue(request, requestProduct);
                        _emailService.InsertEmailQueue(cancellationEmailQueue);
                        requestProduct.HasFulfilled = false;
                        requestProduct.FulfilledDate = DateTime.MinValue;
                        requestProduct.FulfillerId = string.Empty;
                    }

                    requestProduct.Status = (int)RequestProductStatusEnum.Cancelled;
                    _RequestProductRepository.Update(requestProduct);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting cancelling for request {id}", request.Id);
                return false;
            }
        }

        public bool RejectRequest(ApplicationUser user, RequestProduct requestProduct, string remarks)
        {
            var reqProduct = GetRequestProducts().Where(x => x.Id == requestProduct.Id).First();

            if (reqProduct == null)
                throw new Exception("Invalid request.");

            
            var request = GetRequestById(requestProduct.RequestId);
            var projectInformation = _ProjectInformationRepository.GetAll().Where(x => x.RequestId == request.Id).FirstOrDefault();

            requestProduct.Status = (int)RequestProductStatusEnum.Rejected;
            requestProduct.Remarks = remarks;
            requestProduct.HasFulfilled = true;
            requestProduct.FulfillerId = user.Id;
            requestProduct.FulfilledDate = DateTime.UtcNow;
            requestProduct.UpdatedOnUTC = DateTime.UtcNow;
            requestProduct.TimeToResolution = CalculateResolutionTime(requestProduct.FulfilledDate, requestProduct.CreatedOnUTC, _slaService.GetSLAStaffLeavesByStaffId(user.Id), _slaService.GetSLAHolidays());

            List<RequestProduct> requestProducts = _RequestProductRepository.Table.Where(x => x.RequestId == requestProduct.RequestId).ToList();
            bool allRejected = requestProducts.All(rp => rp.Status == (int)RequestProductStatusEnum.Rejected);

            if (DateTime.UtcNow > projectInformation.ClosingDate)
                requestProduct.Breached = true;

            try
            {
                _RequestProductRepository.Update(requestProduct);
                _logger.Information("Rejected request product {id}", reqProduct.Id);
    
                //if all products in a request is rejected, set status of request to reject
                if (allRejected)
                {
                    request.ApprovalState = (int)ApprovalStateEnum.Rejected;
                    _RequestRepository.Update(_mapper.Map<Request>(request));
                }


                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reject request product {id}", requestProduct.Id);
                return false;
            }
        }

        public List<FulfillmentSummary> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity, string userId)
        {
            var requestProducts = _RequestProductRepository.Table.
                Where(rp => rp.FulfilledDate != DateTime.MinValue 
                && rp.FulfilledDate >= startDate 
                && rp.FulfilledDate <= endDate
                && rp.FulfillerId == userId);

            List<FulfillmentSummary> fulfillmentSummary;

            switch (granularity)
            {
                case "day":
                    fulfillmentSummary = requestProducts.GroupBy(rp => rp.FulfilledDate.Date)
                                .Select(group => new FulfillmentSummary
                                {
                                    Period = group.Key.ToString("MMMM dd, yyyy"),
                                    Fulfillments = group.Count()
                                }).ToList();
                    break;
                case "week":
                    fulfillmentSummary = requestProducts.GroupBy(rp => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(rp.FulfilledDate.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday))
                                .Select(group => new FulfillmentSummary
                                {
                                    Period = "Week " + group.Key.ToString(),
                                    Fulfillments = group.Count()
                                }).ToList();
                    break;
                case "month":
                    fulfillmentSummary = requestProducts.GroupBy(rp => rp.FulfilledDate.Date.Month)
                                .Select(group => new FulfillmentSummary
                                {
                                    Period = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key),
                                    Fulfillments = group.Count()
                                }).ToList();
                    break;
                default:
                    throw new Exception("Invalid granularity. Please specify 'day', 'week', or 'month'.");
            }

            return fulfillmentSummary;
        }

        public List<SalesSummary> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity, string userId)
        {
            var requests = GetRequests();
            var filteredRequests = requests.
                Where(r => r.CreatedOnUTC >= startDate 
                && r.CreatedOnUTC <= endDate
                && r.CreatedById == userId);

            List<SalesSummary> salesSummary;

            switch (granularity.ToLower())
            {
                case "day":
                    salesSummary = filteredRequests.GroupBy(r => r.CreatedOnUTC.Date)
                                .Select(group => new SalesSummary
                                {
                                    Period = group.Key.ToString("MMMM dd, yyyy"),
                                    Sales = group.Count()
                                }).ToList();
                    break;
                case "week":
                    salesSummary = filteredRequests.GroupBy(r => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(r.CreatedOnUTC.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday))
                                .Select(group => new SalesSummary
                                {
                                    Period = "Week " + group.Key.ToString(),
                                    Sales = group.Count()
                                }).ToList();
                    break;
                case "month":
                    salesSummary = filteredRequests.GroupBy(r => r.CreatedOnUTC.Date.Month)
                                .Select(group => new SalesSummary
                                {
                                    Period = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Key),
                                    Sales = group.Count()
                                }).ToList();
                    break;
                default:
                    throw new ArgumentException("Invalid granularity. Allowed values are 'day', 'week', 'month'");
            }

            return salesSummary;
        }

        public TimeSpan CalculateResolutionTime(DateTime approvedTime, DateTime ticketCreateTime, List<SLAStaffLeaveDTO> staffLeaves, List<SLAHolidayDTO> holidays)
        {
            TimeSpan resolutionTime = approvedTime - ticketCreateTime;

            TimeSpan workingHoursResolutionTime = TimeSpan.Zero;
            if (_slaSetting.Value.IncludeWorkingHours == true)
            {
                DateTime startTime = new DateTime(ticketCreateTime.Year, ticketCreateTime.Month, ticketCreateTime.Day, _slaSetting.Value.WorkingStartHour, _slaSetting.Value.WorkingStartMinute, 0);
                DateTime endTime = new DateTime(approvedTime.Year, approvedTime.Month, approvedTime.Day, _slaSetting.Value.WorkingEndHour, _slaSetting.Value.WorkingEndMinute, 0);

                startTime = startTime.ToUniversalTime();
                endTime = endTime.ToUniversalTime();

                if (ticketCreateTime >= startTime && approvedTime <= endTime)
                {
                    int fullDays = 0;
                    if (startTime < endTime)
                    {
                        fullDays = (int)(endTime - startTime).TotalDays;
                        if (startTime.DayOfWeek > endTime.DayOfWeek)
                        {
                            fullDays -= 2;
                        }
                        else if (endTime.DayOfWeek == DayOfWeek.Saturday)
                        {
                            fullDays -= 1;
                        }
                    }

                    workingHoursResolutionTime = TimeSpan.FromHours(fullDays * 8);

                    if (fullDays == 0)
                    {
                        workingHoursResolutionTime = TimeSpan.FromTicks(Math.Min(endTime.Ticks, approvedTime.Ticks) - Math.Max(startTime.Ticks, ticketCreateTime.Ticks));
                    }
                    else
                    {
                        DateTime partialDayStart = startTime.AddDays(fullDays);
                        DateTime partialDayEnd = endTime;
                        if (partialDayStart.DayOfWeek != DayOfWeek.Saturday && partialDayStart.DayOfWeek != DayOfWeek.Sunday) // if partial day is a weekday
                        {
                            workingHoursResolutionTime += TimeSpan.FromTicks(Math.Min(partialDayEnd.Ticks, approvedTime.Ticks) - Math.Max(partialDayStart.Ticks, ticketCreateTime.Ticks));
                        }
                    }
                }
            }

            if (_slaSetting.Value.IncludeHoliday == true)
            {
                resolutionTime -= GetTimeSpanOfHolidayDates(ticketCreateTime, approvedTime, holidays.Select(x => x.Date).ToList());
            }
            if (_slaSetting.Value.IncludeStaffLeaves == true)
            {
                resolutionTime -= GetTimeSpanOfStaffLeaves(ticketCreateTime, approvedTime, staffLeaves.Select(l => (l.StartDate, l.EndDate)).ToList());
            }

            if (_slaSetting.Value.IncludeWorkingHours == true && workingHoursResolutionTime < resolutionTime)
            {
                return workingHoursResolutionTime;
            }
            else
            {
                return resolutionTime;
            }
        }

        private TimeSpan GetTimeSpanOfHolidayDates(DateTime start, DateTime end, List<DateTime> dates)
        {
            TimeSpan overlappingTime = TimeSpan.Zero;
            foreach (DateTime date in dates)
            {
                if (date >= start && date < end)
                {
                    overlappingTime += TimeSpan.FromDays(1);
                }
            }
            return overlappingTime;
        }
        private TimeSpan GetTimeSpanOfStaffLeaves(DateTime start, DateTime end, List<(DateTime Start, DateTime End)> dateRanges)
        {
            TimeSpan overlappingTime = TimeSpan.Zero;
            foreach (var dateRange in dateRanges)
            {
                if (dateRange.End > start && dateRange.Start < end)
                {
                    DateTime overlappingStart = dateRange.Start < start ? start : dateRange.Start;
                    DateTime overlappingEnd = dateRange.End > end ? end : dateRange.End;
                    overlappingTime += overlappingEnd - overlappingStart;
                }
            }
            return overlappingTime;
        }
    }
}
