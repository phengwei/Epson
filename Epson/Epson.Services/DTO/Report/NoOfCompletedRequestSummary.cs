using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Report
{
    public class NoOfCompletedRequestSummary
    {
        public string Period { get; set; }
        public int CompletedRequests { get; set; }
    }
}
