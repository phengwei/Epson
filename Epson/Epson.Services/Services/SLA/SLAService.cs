using Epson.Data;
using Epson.Core.Domain.Products;
using Epson.Services.Interface.Products;
using AutoMapper;
using Epson.Services.DTO.Products;
using Serilog;
using Epson.Services.Interface.SLA;
using Epson.Core.Domain.SLA;
using Epson.Services.DTO.SLA;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using Microsoft.Identity.Client;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using Epson.Services.Interface.Users;
using Epson.Core.Domain.Users;
using Epson.Services.Interface.Requests;
using Epson.Services.DTO.Requests;

namespace Epson.Services.Services.SLA
{
    public class SLAService : ISLAService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SLAHoliday> _SLAHolidayRepository;
        private readonly IRepository<SLAStaffLeave> _SLAStaffLeaveRepository;
        private readonly IRepository<Request> _requestRepository;
        private readonly IRepository<RequestProduct> _requestProductRepository;
        private readonly IRepository<Team> _teamRepository;
        private readonly ILogger _logger;
        private readonly IOptions<SLASetting> _slaSetting;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        public SLAService
            (IMapper mapper,
            IRepository<SLAHoliday> slaHolidayRepository,
            IRepository<SLAStaffLeave> slaStaffLeaveRepository,
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            IRepository<Team> teamRepository,
            ILogger logger,
            IOptions<SLASetting> slaSetting,
            IConfiguration configuration,
            IUserService userService,
            UserManager<ApplicationUser> userManager)
        {
            _mapper = mapper;
            _SLAHolidayRepository = slaHolidayRepository;
            _SLAStaffLeaveRepository = slaStaffLeaveRepository;
            _requestRepository = requestRepository;
            _requestProductRepository = requestProductRepository;
            _teamRepository = teamRepository;
            _logger = logger;
            _slaSetting = slaSetting;
            _configuration = configuration;
            _userService = userService;
            _userManager = userManager;
        }

        public SLAHolidayDTO GetSLAHolidayById(int id)
        {
            if (id == 0 || id == null)
                return new SLAHolidayDTO();

            return _mapper.Map<SLAHolidayDTO>(_SLAHolidayRepository.GetById(id));
        }
        public SLAStaffLeaveDTO GetSLAStaffLeaveById(int id)
        {
            if (id == 0 || id == null)
                return new SLAStaffLeaveDTO();

            return _mapper.Map<SLAStaffLeaveDTO>(_SLAStaffLeaveRepository.GetById(id));
        }

        public List<SLAHolidayDTO> GetSLAHolidays()
        {
            var slaHolidays = _SLAHolidayRepository.GetAll().DistinctBy(holiday => holiday.Date).ToList();

            return _mapper.Map<List<SLAHolidayDTO>>(slaHolidays);
        }

        public List<SLAStaffLeaveDTO> GetSLAStaffLeaves()
        {
            var slaStaffLeaves = _SLAStaffLeaveRepository.GetAll();

            return _mapper.Map<List<SLAStaffLeaveDTO>>(slaStaffLeaves);
        }

        public List<SLAStaffLeaveDTO> GetSLAStaffLeavesByStaffId(string staffId)
        {
            var slaStaffLeaves = _SLAStaffLeaveRepository.GetAll().Where(x => x.StaffId == staffId);

            return _mapper.Map<List<SLAStaffLeaveDTO>>(slaStaffLeaves);
        }

        public bool InsertSLAHoliday(SLAHoliday slaHoliday)
        {
            if (slaHoliday == null)
                throw new ArgumentNullException(nameof(slaHoliday));

            try
            {
                _SLAHolidayRepository.Add(slaHoliday);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inserting SLA Holiday of {holidayDate}", slaHoliday.Date);

                return false;
            }
        }

        public bool InsertSLAStaffLeave(SLAStaffLeave slaStaffLeave)
        {
            if (slaStaffLeave == null)
                throw new ArgumentNullException(nameof(slaStaffLeave));

            try
            {
                _SLAStaffLeaveRepository.Add(slaStaffLeave);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inserting SLA Holiday of {fromleave} to {toleave} for {userId}", slaStaffLeave.StartDate, slaStaffLeave.EndDate, slaStaffLeave.StaffId);

                return false;
            }
        }

        public bool DeleteSLAHoliday(SLAHoliday slaHoliday)  
        {
            if (slaHoliday == null)
                throw new ArgumentNullException(nameof(slaHoliday));

            if (GetSLAHolidayById(slaHoliday.Id) == null)
                throw new ArgumentNullException(nameof(slaHoliday));

            try
            {
                _SLAHolidayRepository.Delete(slaHoliday.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting SLA Holiday of {holidayDate}", slaHoliday.Date);

                return false;
            }
        }

        public bool DeleteStaffLeaveHoliday(SLAStaffLeave slaStaffLeave)
        {
            if (slaStaffLeave == null)
                throw new ArgumentNullException(nameof(slaStaffLeave));

            if (GetSLAStaffLeaveById(slaStaffLeave.Id) == null)
                throw new ArgumentNullException(nameof(slaStaffLeave));

            try
            {
                _SLAStaffLeaveRepository.Delete(slaStaffLeave.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting SLA Holiday of {fromleave} to {toleave} for {userId}", slaStaffLeave.StartDate, slaStaffLeave.EndDate, slaStaffLeave.StaffId);

                return false;
            }
        }

        public SLASettingDTO GetSLASettings()
        {
            return _mapper.Map<SLASettingDTO>(_slaSetting.Value);
        }

        public decimal GetAverageTimeToResolutionInHours(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month, int year)
        {
            List<RequestProduct> ticketsResolved = new List<RequestProduct>();

            if (isAdminUser)
            {
                ticketsResolved = _requestProductRepository.Table
                    .Where(x => x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                ticketsResolved = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }
            else
            {
                ticketsResolved = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }

            TimeSpan totalResolutionTime = TimeSpan.Zero;

            foreach (var ticket in ticketsResolved)
                totalResolutionTime += ticket.TimeToResolution;

            decimal averageTimeToResolution = 0.0m;

            if (totalResolutionTime != TimeSpan.Zero)
                averageTimeToResolution = (decimal)totalResolutionTime.TotalHours / ticketsResolved.Count;

            averageTimeToResolution = Math.Round(averageTimeToResolution, 2);

            return averageTimeToResolution;
        }


        public int GetBreachedTicketCount(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month, int year)
        {
            List<int> breachedRequests = new List<int>();

            if (isAdminUser)
            {
                breachedRequests = _requestProductRepository.Table
                    .Where(x => x.Breached == true && (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .Select(x => x.RequestId)
                    .Distinct()
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                breachedRequests = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && x.Breached == true && (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .Select(x => x.RequestId)
                    .Distinct()
                    .ToList();
            }
            else
            {
                breachedRequests = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && x.Breached == true && (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .Select(x => x.RequestId)
                    .Distinct()
                    .ToList();
            }

            return breachedRequests.Count;
        }


        public int GetTotalTicketCount(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month, int year)
        {
            List<Request> totalTickets = new List<Request>();

            if (isAdminUser)
            {
                totalTickets = _requestRepository.Table
                    .Where(x => (month == 0 || x.CreatedOnUTC.Month == month) && (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }
            else
            {
                var relevantRequestProductIds = _requestProductRepository.Table
                    .Where(rp => (isSalesHeadUser && users.Contains(rp.FulfillerId)) ||
                                 (!isSalesHeadUser && rp.FulfillerId == user.Id))
                    .Select(rp => rp.RequestId)
                    .Distinct()
                    .ToList();

                totalTickets = _requestRepository.Table
                    .Where(x => relevantRequestProductIds.Contains(x.Id) &&
                                (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }

            return totalTickets.Count;
        }

        public int GetTotalOpenTicketCount(
            ApplicationUser user,
            bool isSalesHeadUser,
            List<string> users,
            List<RequestDTO> requests,
            bool isAdminUser,
            int month,
            int year)
        {
            List<Request> totalTickets = new List<Request>();

            var closedStates = new[]
                    {
                (int)ApprovalStateEnum.Approved,
                (int)ApprovalStateEnum.RejectedByFulfiller,
                (int)ApprovalStateEnum.RejectedByRequester,
                (int)ApprovalStateEnum.RejectedBySalesSectionHead,
                (int)ApprovalStateEnum.Cancelled,
                (int)ApprovalStateEnum.DealExited
            };

            if (isAdminUser)
            {
                totalTickets = _requestRepository.Table
                    .Where(x => !closedStates.Contains(x.ApprovalState) &&
                                (month == 0 || x.CreatedOnUTC.Month == month) &&
                                (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }
            else
            {
                var relevantRequestIds = _requestProductRepository.Table
                    .Where(rp => (isSalesHeadUser && users.Contains(rp.FulfillerId)) ||
                                 (!isSalesHeadUser && rp.FulfillerId == user.Id))
                    .Select(rp => rp.RequestId)
                    .Distinct()
                    .ToList();

                totalTickets = _requestRepository.Table
                    .Where(x => relevantRequestIds.Contains(x.Id) &&
                                !closedStates.Contains(x.ApprovalState) &&
                                (month == 0 || x.CreatedOnUTC.Month == month) &&
                                (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }

            return totalTickets.Count;
        }



        public int GetTotalClosedTicketCount(
            ApplicationUser user,
            bool isSalesHeadUser,
            List<string> users,
            List<RequestDTO> requests,
            bool isAdminUser,
            int month,
            int year)
        {
            List<Request> totalTickets = new List<Request>();

            var closedStates = new[]
                    {
                (int)ApprovalStateEnum.Approved,
                (int)ApprovalStateEnum.RejectedByFulfiller,
                (int)ApprovalStateEnum.RejectedByRequester,
                (int)ApprovalStateEnum.RejectedBySalesSectionHead,
                (int)ApprovalStateEnum.Cancelled,
                (int)ApprovalStateEnum.DealExited
            };

            if (isAdminUser)
            {
                totalTickets = _requestRepository.Table
                    .Where(x => closedStates.Contains(x.ApprovalState) &&
                                (month == 0 || x.CreatedOnUTC.Month == month) &&
                                (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }
            else
            {
                var relevantRequestIds = _requestProductRepository.Table
                    .Where(rp => (isSalesHeadUser && users.Contains(rp.FulfillerId)) ||
                                 (!isSalesHeadUser && rp.FulfillerId == user.Id))
                    .Select(rp => rp.RequestId)
                    .Distinct()
                    .ToList();

                totalTickets = _requestRepository.Table
                    .Where(x => relevantRequestIds.Contains(x.Id) &&
                                closedStates.Contains(x.ApprovalState) &&
                                (month == 0 || x.CreatedOnUTC.Month == month) &&
                                (year == 0 || x.CreatedOnUTC.Year == year))
                    .ToList();
            }

            return totalTickets.Count;
        }



        private int GetApprovedTickets(
            ApplicationUser user, bool isSalesHeadUser, List<string> users,
            List<RequestDTO> requests, bool isAdminUser, int month)
        {
            IQueryable<RequestProduct> q = _requestProductRepository.Table;

            if (isAdminUser)
            {
                // no role restriction
            }
            else if (isSalesHeadUser)
            {
                q = q.Where(x => users.Contains(x.FulfillerId));
            }
            else
            {
                q = q.Where(x => x.FulfillerId == user.Id);
            }

            if (month != 0)
                q = q.Where(x => x.CreatedOnUTC.Month == month);

            return q.Where(x => x.HasFulfilled == true)
                    .Select(x => x.RequestId)
                    .Distinct()
                    .Count();
        }

        public decimal GetSuccessRateOfTickets(
            ApplicationUser user, bool isSalesHeadUser, List<string> users,
            List<RequestDTO> requests, bool isAdminUser, int month, int year)
        {
            IQueryable<RequestProduct> q = _requestProductRepository.Table;

            if (isAdminUser)
            {
                // no role restriction
            }
            else if (isSalesHeadUser)
            {
                q = q.Where(x => users.Contains(x.FulfillerId));
            }
            else
            {
                q = q.Where(x => x.FulfillerId == user.Id);
            }

            if (month != 0) q = q.Where(x => x.CreatedOnUTC.Month == month);
            if (year != 0) q = q.Where(x => x.CreatedOnUTC.Year == year);

            // Denominator at request level: requests with at least one approved product
            var approvedRequestIds = q.Where(x => x.HasFulfilled == true)
                                      .Select(x => x.RequestId)
                                      .Distinct()
                                      .ToList();

            var totalRequestsApproved = approvedRequestIds.Count;
            if (totalRequestsApproved == 0) return 0m;

            // A request fails if ANY in-scope product in that request is breached
            var breachedApprovedRequestCount = _requestProductRepository.Table
                .Where(x => approvedRequestIds.Contains(x.RequestId)
                            && x.Breached == true
                            && (month == 0 || x.CreatedOnUTC.Month == month)
                            && (year == 0 || x.CreatedOnUTC.Year == year))
                .Select(x => x.RequestId)
                .Distinct()
                .Count();

            var successfulRequests = totalRequestsApproved - breachedApprovedRequestCount;
            var rate = (decimal)successfulRequests / totalRequestsApproved * 100m;
            return Math.Round(rate, 2);
        }


    }
}
