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
        public int DeadlineInHours { get; set; }
    }
}
