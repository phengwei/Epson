using Epson.Core.Domain.Enum;
using Epson.Core.Domain.SLA;
using Epson.Core.Domain.Users;
using Epson.Services.DTO.Requests;
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
        public decimal GetAverageTimeToResolutionInHours(ApplicationUser user, bool isSalesHead, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month);
        public int GetBreachedTicketCount(ApplicationUser user, bool isSalesHead, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month);
        public int GetTotalTicketCount(ApplicationUser user, bool isSalesHead, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month);
        public decimal GetSuccessRateOfTickets(ApplicationUser user, bool isSalesHead, List<string> users, List<RequestDTO> requests, bool isAdminUser, int month);
    }
}
