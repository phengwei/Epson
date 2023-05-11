using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.SLA
{
    public class SLASettingDTO
    {
        public bool IncludeHoliday { get; set; }
        public bool IncludeStaffLeaves { get; set; }
        public bool IncludeWorkingHours { get; set; }
        public int WorkingStartHour { get; set; }
        public int WorkingStartMinute { get; set; }
        public int WorkingEndHour { get; set; }
        public int WorkingEndMinute { get; set; }
        public int DeadlineInHours { get; set; }
    }
}
