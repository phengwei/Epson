using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Report
{
    public class FulfillmentSummary
    {
        public DateTime? Date { get; set; }
        public int? Week { get; set; }
        public int? Month { get; set; }
        public int Fulfillments { get; set; }
    }
}
