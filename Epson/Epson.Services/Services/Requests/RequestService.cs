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
        private readonly IProductService _productService;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly ISLAService _slaService;
        private readonly IOptions<SLASetting> _slaSetting;

        public RequestService
            (IMapper mapper,
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            IProductService productService,
            IEmailService emailService,
            ILogger logger,
            ISLAService slaService,
            IOptions<SLASetting> slaSetting)
        {
            _mapper = mapper;
            _RequestRepository = requestRepository;
            _RequestProductRepository = requestProductRepository;
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

            requestDTO.RequestProducts = _RequestProductRepository.GetAll().Where(x => x.RequestId == id).ToList();

            return requestDTO;
        }

        public List<RequestDTO> GetRequests()
        {
            var requests = _RequestRepository.GetAll();

            var requestDTOs = requests.Select(x => new RequestDTO
            {
                Id = x.Id,
                ApprovedBy = x.ApprovedBy,
                ApprovedTime = x.ApprovedTime,
                CreatedById = x.CreatedById,
                CreatedOnUTC = x.CreatedOnUTC,
                UpdatedById = x.UpdatedById,
                UpdatedOnUTC = x.UpdatedOnUTC,
                Segment = x.Segment,
                TotalBudget = x.TotalBudget,
                ApprovalState = x.ApprovalState,
                Priority = x.Priority,
                Deadline = x.Deadline,
                TotalPrice = x.TotalPrice,
                TimeToResolution = x.TimeToResolution,
                RequestProducts = _RequestProductRepository.Table.Where(y => y.RequestId == x.Id).ToList()
            })
            .OrderBy(x => x.CreatedOnUTC)
            .ToList();

            return requestDTOs;
        }

        public bool InsertRequest(Request request, List<RequestProduct> requestProducts)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                request.TotalBudget = GetTotalBudgetOfRequestProducts(requestProducts);
                request.Id = _RequestRepository.Add(request);

                _logger.Information("Creating request {id}", request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    var product = _productService.GetProductById(requestProduct.ProductId);

                    requestProduct.RequestId = request.Id;
                    requestProduct.FulfillerId = product.CreatedById;
                    InsertRequestProduct(requestProduct);
                }

                var requestQueue = _emailService.CreateRequestEmailQueue(request, requestProducts);
                _emailService.InsertEmailQueue(requestQueue);

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

        public bool UpdateRequest(Request request, List<RequestProduct> requestProducts)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));
            try
            {
                _RequestRepository.Update(request);
                _logger.Information("Updating request {id}", request.Id);

                DeleteRequestProductOfRequest(request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    requestProduct.RequestId = request.Id;

                    InsertRequestProduct(requestProduct);
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

        public bool ApproveRequest(ApplicationUser user, Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ApprovalState != (int)ApprovalStateEnum.PendingRequesterAction)
                return false;
            
            request.ApprovalState = (int)ApprovalStateEnum.Approved;
            request.ApprovedBy = user.Id;
            request.ApprovedTime = DateTime.UtcNow;
            request.UpdatedOnUTC = DateTime.UtcNow;
            request.UpdatedById = user.Id;
            request.TimeToResolution = CalculateResolutionTime(request.ApprovedTime, request.CreatedOnUTC, _slaService.GetSLAStaffLeavesByStaffId(user.Id), _slaService.GetSLAHolidays());

            if (DateTime.UtcNow > request.Deadline)
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

        public bool FulfillRequest(ApplicationUser user, Request request, Product product, decimal totalPrice)
        {
            var existingRequest = GetRequestById(request.Id);
            var existingProduct = _productService.GetProductById(product.Id);

            if (existingRequest == null || existingProduct == null || request.ApprovalState != (int)ApprovalStateEnum.PendingFulfillerAction)
                return false;

            var requestProducts = _RequestProductRepository.GetAll().Where(x => x.RequestId == request.Id).ToList();
            var requestProductToFulfill = requestProducts.FirstOrDefault(x => x.ProductId == product.Id);

            if (requestProductToFulfill == null)
                return false;

            requestProductToFulfill.FulfilledPrice = totalPrice;
            requestProductToFulfill.HasFulfilled = true;
            requestProductToFulfill.FulfilledDate = DateTime.UtcNow;

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

                var fulfillRequestQueue = _emailService.CreateFulfillEmailQueue(request, requestProductToFulfill, allProductsFulfilled);
                _emailService.InsertEmailQueue(fulfillRequestQueue);

                _RequestRepository.Update(request);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fulfilling request {id}", requestProductToFulfill.Id);
                return false;
            }
        }

        public List<FulfillmentSummary> GetFulfillmentSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var requestProducts = _RequestProductRepository.Table.Where(rp => rp.FulfilledDate != DateTime.MinValue && rp.FulfilledDate >= startDate && rp.FulfilledDate <= endDate);

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

        public List<SalesSummary> GetRequestSummary(DateTime startDate, DateTime endDate, string granularity)
        {
            var requests = GetRequests();
            var filteredRequests = requests.Where(r => r.CreatedOnUTC >= startDate && r.CreatedOnUTC <= endDate);

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
