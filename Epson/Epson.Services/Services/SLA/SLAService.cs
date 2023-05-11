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

namespace Epson.Services.Services.SLA
{
    public class SLAService : ISLAService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<SLAHoliday> _SLAHolidayRepository;
        private readonly IRepository<SLAStaffLeave> _SLAStaffLeaveRepository;
        private readonly ILogger _logger;
        private readonly IOptions<SLASetting> _slaSetting;
        private readonly IConfiguration _configuration;

        public SLAService
            (IMapper mapper,
            IRepository<SLAHoliday> slaHolidayRepository,
            IRepository<SLAStaffLeave> slaStaffLeaveRepository,
            ILogger logger,
            IOptions<SLASetting> slaSetting,
            IConfiguration configuration)
        {
            _mapper = mapper;
            _SLAHolidayRepository = slaHolidayRepository;
            _SLAStaffLeaveRepository = slaStaffLeaveRepository;
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
            var slaHolidays = _SLAHolidayRepository.GetAll();

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

    }
}
