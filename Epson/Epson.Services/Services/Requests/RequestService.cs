using AutoMapper;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Services.DTO.Requests;
using Epson.Services.DTO.SLA;
using Epson.Services.Interface.Requests;
using Epson.Services.Interface.SLA;
using Microsoft.Extensions.Options;
using Serilog;

namespace Epson.Services.Services.Requests
{
    public class RequestService : IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<Request> _RequestRepository;
        private readonly IRepository<RequestProduct> _RequestProductRepository;
        private readonly ILogger _logger;
        private readonly ISLAService _slaService;
        private readonly IOptions<SLASetting> _slaSetting;

        public RequestService
            (IMapper mapper,
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            ILogger logger,
            ISLAService slaService,
            IOptions<SLASetting> slaSetting)
        {
            _mapper = mapper;
            _RequestRepository = requestRepository;
            _RequestProductRepository = requestProductRepository;
            _logger = logger;
            _slaService = slaService;
            _slaSetting = slaSetting;
        }

        public RequestDTO GetRequestById(int id)
        {
            if (id == 0 || id == null)
                return new RequestDTO();

            return _mapper.Map<RequestDTO>(_RequestRepository.GetById(id));
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
                ManagerId = x.ManagerId,
                ManagerName = x.ManagerName,
                Quantity = x.Quantity,
                Priority = x.Priority,
                Deadline = x.Deadline,
                TotalPrice = x.TotalPrice,
                TimeToResolution = x.TimeToResolution
            })
            .OrderBy(x => x.CreatedOnUTC)
            .ToList();

            //var t = CalculateTimeToResolution(requests.FirstOrDefault());

            return requestDTOs;
        }

        public bool InsertRequest(Request request, List<RequestProduct> requestProducts)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                request.Id = _RequestRepository.Add(request);
                _logger.Information("Creating request {id}", request.Id);

                foreach (var requestProduct in requestProducts)
                {
                    requestProduct.RequestId = request.Id;

                    InsertRequestProduct(requestProduct);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating request {id}", request.Id);

                return false;
            }
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
                _logger.Error(ex, "Error deleting request products of request{requestid}", requestId);

                return false;
            }
        }

        public bool ApproveRequest(ApplicationUser user, Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (GetRequestById(request.Id) == null)
                throw new ArgumentNullException(nameof(request));

            request.ApprovedBy = user.Id;
            request.ApprovedTime = DateTime.UtcNow;

            request.TimeToResolution = CalculateResolutionTime(request.ApprovedTime, request.CreatedOnUTC, _slaService.GetSLAStaffLeavesByStaffId(user.Id), _slaService.GetSLAHolidays());

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

        //todo: add deadline configuration?
        public TimeSpan CalculateResolutionTime(DateTime approvedTime, DateTime ticketCreateTime, List<SLAStaffLeaveDTO> staffLeaves, List<SLAHolidayDTO> holidays)
        {
            //calculate resolution time
            TimeSpan resolutionTime = approvedTime - ticketCreateTime;

            //calculate resolution time if working hours is included 
            TimeSpan workingHoursResolutionTime = TimeSpan.Zero;
            if (_slaSetting.Value.IncludeWorkingHours == true)
            {
                DateTime startTime = new DateTime(ticketCreateTime.Year, ticketCreateTime.Month, ticketCreateTime.Day, _slaSetting.Value.WorkingStartHour, _slaSetting.Value.WorkingStartMinute, 0);
                DateTime endTime = new DateTime(approvedTime.Year, approvedTime.Month, approvedTime.Day, _slaSetting.Value.WorkingEndHour, _slaSetting.Value.WorkingEndMinute, 0);

                //calculate the number of full working days between the start and end dates
                int fullDays = 0;
                if (startTime < endTime)
                {
                    fullDays = (int)(endTime - startTime).TotalDays;
                    if (startTime.DayOfWeek > endTime.DayOfWeek)
                    {
                        fullDays -= 2; //adjust for weekends between start and end dates
                    }
                    else if (endTime.DayOfWeek == DayOfWeek.Saturday)
                    {
                        fullDays -= 1; //adjust for end date on a weekend
                    }
                }

                //calculate the working hours for the full days
                workingHoursResolutionTime = TimeSpan.FromHours(fullDays * 8);

                //calculate the working hours for the partial day (if any)
                if (fullDays == 0)
                {
                    //if the start and end dates are on the same day, calculate the working hours between them
                    workingHoursResolutionTime = TimeSpan.FromTicks(Math.Min(endTime.Ticks, approvedTime.Ticks) - Math.Max(startTime.Ticks, ticketCreateTime.Ticks));
                }
                else
                {
                    //if there are full working days between the start and end dates, calculate the working hours for the partial day(s) at the start and end
                    DateTime partialDayStart = startTime.AddDays(fullDays);
                    DateTime partialDayEnd = endTime;
                    if (partialDayStart.DayOfWeek != DayOfWeek.Saturday && partialDayStart.DayOfWeek != DayOfWeek.Sunday) // if partial day is a weekday
                    {
                        workingHoursResolutionTime += TimeSpan.FromTicks(Math.Min(partialDayEnd.Ticks, approvedTime.Ticks) - Math.Max(partialDayStart.Ticks, ticketCreateTime.Ticks));
                    }
                }
            }

            //adjust resolution time based on SLA settings
            if (_slaSetting.Value.IncludeHoliday == true)
            {
                resolutionTime -= GetTimeSpanOfHolidayDates(ticketCreateTime, approvedTime, holidays.Select(x => x.Date).ToList());
            }
            if (_slaSetting.Value.IncludeStaffLeaves == true)
            {
                resolutionTime -= GetTimeSpanOfStaffLeaves(ticketCreateTime, approvedTime, staffLeaves.Select(l => (l.StartDate, l.EndDate)).ToList());
            }

            //choose the appropriate resolution time based on SLA settings
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
