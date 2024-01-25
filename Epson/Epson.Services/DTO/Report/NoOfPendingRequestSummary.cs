using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Report
{
    public class NoOfPendingRequestSummary
    {
        public string Period { get; set; }
        public int PendingRequests { get; set; }
    }
}
