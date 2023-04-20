using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Request
{
    public class Request : BaseEntityExtension
    {
        public DateTime ApprovedTime { get; set; }
        public int ApprovedBy { get; set; }
        public int ProductId { get; set; }
        public string Segment { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; }
        public int Quantity { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Decimal TotalPrice { get; set; }
        public DateTime TimeToResolution { get; set; }
    }
}
