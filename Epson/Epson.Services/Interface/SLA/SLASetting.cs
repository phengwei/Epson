using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.SLA
{
    public class SLASetting
    {
        public bool IncludeHoliday { get; set; }
        public bool IncludeStaffLeaves { get; set; }
        public bool IncludeWorkingHours { get; set; }
        public int WorkingStartHour { get; set; }
        public int WorkingStartMinute { get; set; }
        public int WorkingEndHour { get; set; }
        public int WorkingEndMinute { get; set; }
        public int Deadline { get; set; }
    }
}
