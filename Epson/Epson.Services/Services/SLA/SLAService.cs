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

        public decimal GetAverageTimeToResolutionInHours(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month)
        {
            List<RequestProduct> ticketsResolved = new List<RequestProduct>();

            if (isAdminUser)
            {
                ticketsResolved = _requestProductRepository.Table
                    .Where(x => x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                ticketsResolved = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else
            {
                ticketsResolved = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }

            TimeSpan totalResolutionTime = TimeSpan.Zero;

            foreach (var ticket in ticketsResolved)
            {
                totalResolutionTime += ticket.TimeToResolution;
            }

            decimal averageTimeToResolution = (decimal)totalResolutionTime.TotalHours / ticketsResolved.Count;
            averageTimeToResolution = Math.Round(averageTimeToResolution, 2);

            return averageTimeToResolution;
        }

        public int GetBreachedTicketCount(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month)
        {
            List<RequestProduct> ticketsBreached = new List<RequestProduct>();

            if (isAdminUser)
            {
                ticketsBreached = _requestProductRepository.Table
                    .Where(x => x.Breached == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                ticketsBreached = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && x.Breached == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else
            {
                ticketsBreached = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && x.Breached == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }

            return ticketsBreached.Count;
        }

        public int GetTotalTicketCount(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month)
        {
            List<RequestProduct> totalTickets = new List<RequestProduct>();

            if (isAdminUser)
            {
                totalTickets = _requestProductRepository.Table
                    .Where(x => month == 0 || x.CreatedOnUTC.Month == month)
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                totalTickets = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else
            {
                totalTickets = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }

            return totalTickets.Count;
        }

        private int GetApprovedTickets(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month)
        {
            List<RequestProduct> approvedTickets = new List<RequestProduct>();

            if (isAdminUser)
            {
                approvedTickets = _requestProductRepository.Table
                    .Where(x => x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                approvedTickets = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else
            {
                approvedTickets = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && x.HasFulfilled == true && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }

            return approvedTickets.Count;
        }

        public decimal GetSuccessRateOfTickets(ApplicationUser user, bool isSalesHeadUser, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month)
        {
            List<RequestProduct> successTickets = new List<RequestProduct>();

            if (isAdminUser)
            {
                successTickets = _requestProductRepository.Table
                    .Where(x => x.HasFulfilled == true && !x.Breached && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else if (isSalesHeadUser)
            {
                successTickets = _requestProductRepository.Table
                    .Where(x => users.Contains(x.FulfillerId) && x.HasFulfilled == true && !x.Breached && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }
            else
            {
                successTickets = _requestProductRepository.Table
                    .Where(x => x.FulfillerId == user.Id && x.HasFulfilled == true && !x.Breached && (month == 0 || x.CreatedOnUTC.Month == month))
                    .ToList();
            }

            var totalTickets = GetApprovedTickets(user, isSalesHeadUser, users, requests, isAdminUser, month);

            decimal successRate = 0;
            if (totalTickets > 0)
            {
                successRate = (decimal)successTickets.Count / totalTickets;
                successRate = Math.Round(successRate * 100, 2);
            }

            return successRate;
        }

    }
}
