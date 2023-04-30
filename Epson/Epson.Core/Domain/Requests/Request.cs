using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class Request : BaseEntityExtension
    {
        public DateTime ApprovedTime { get; set; }
        public int ApprovedBy { get; set; }
        public string Segment { get; set; }
        public string ManagerId { get; set; }
        public string ManagerName { get; set; }
        public int Quantity { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Decimal TotalPrice { get; set; }
        public TimeSpan TimeToResolution { get; set; }
    }
}
