using Epson.Core.Domain.SLA;
using Epson.Services.DTO.SLA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.SLA
{
    public interface ISLAService
    {
        public SLAHolidayDTO GetSLAHolidayById(int id);
        public SLAStaffLeaveDTO GetSLAStaffLeaveById(int id);
        public List<SLAHolidayDTO> GetSLAHolidays();
        public List<SLAStaffLeaveDTO> GetSLAStaffLeaves();
        public List<SLAStaffLeaveDTO> GetSLAStaffLeavesByStaffId(string staffId);
        public bool InsertSLAHoliday(SLAHoliday slaHoliday);
        public bool InsertSLAStaffLeave(SLAStaffLeave slaStaffLeave);
        public bool DeleteSLAHoliday(SLAHoliday slaHoliday);
        public bool DeleteStaffLeaveHoliday(SLAStaffLeave slaStaffLeave);
        public SLASettingDTO GetSLASettings();
    }
}
