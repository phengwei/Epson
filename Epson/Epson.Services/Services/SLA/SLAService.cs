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

namespace Epson.Services.Services.SLA
{
    public class SLAService : ISLAService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SLAHoliday> _SLAHolidayRepository;
        private readonly IRepository<SLAStaffLeave> _SLAStaffLeaveRepository;
        private readonly IRepository<Request> _requestRepository;
        private readonly ILogger _logger;
        private readonly IOptions<SLASetting> _slaSetting;
        private readonly IConfiguration _configuration;

        public SLAService
            (IMapper mapper,
            IRepository<SLAHoliday> slaHolidayRepository,
            IRepository<SLAStaffLeave> slaStaffLeaveRepository,
            IRepository<Request> requestRepository,
            ILogger logger,
            IOptions<SLASetting> slaSetting,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _SLAHolidayRepository = slaHolidayRepository;
            _SLAStaffLeaveRepository = slaStaffLeaveRepository;
            _requestRepository = requestRepository;
            _logger = logger;
            _slaSetting = slaSetting;
            _configuration = configuration;
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

        public decimal GetAverageTimeToResolutionInHours(string userId)
        {
            var ticketsResolved = _requestRepository.Table
                .Where(x => x.CreatedById == userId 
                && x.ApprovalState == (int)ApprovalStateEnum.Approved)
                .ToList();

            TimeSpan totalResolutionTime = TimeSpan.Zero;

            foreach (var ticket in ticketsResolved)
                totalResolutionTime += ticket.TimeToResolution;

            decimal averageTimeToResolution = (decimal)totalResolutionTime.TotalHours / ticketsResolved.Count;
            averageTimeToResolution = Math.Round(averageTimeToResolution, 2);

            return averageTimeToResolution;
        }

        public int GetBreachedTicketCount(string userId)
        {
            var ticketsBreached = _requestRepository.Table
                .Where(x => x.Breached &&
                            x.CreatedById == userId &&
                            x.ApprovalState == (int)ApprovalStateEnum.Approved)
                .ToList();

            return ticketsBreached.Count;
        }

        public int GetTotalTicketCount(string userId)
        {
            var totalTickets = _requestRepository.Table
                .Where(x => x.CreatedById == userId)
                .ToList();

            return totalTickets.Count;
        }

        public int GetApprovedTickets(string userId)
        {
            var approvedTickets = _requestRepository.Table
                .Where(x => x.CreatedById == userId &&
                            x.ApprovalState == (int)ApprovalStateEnum.Approved)
                .ToList();

            return approvedTickets.Count;
        }

        public decimal GetSuccessRateOfTickets(string userId)
        {
            var successTickets = _requestRepository.Table
                .Where(x => x.CreatedById == userId &&
                            !x.Breached &&
                            x.ApprovalState == (int)ApprovalStateEnum.Approved)
                .ToList();

            var totalTickets = GetApprovedTickets(userId);

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
