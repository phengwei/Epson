using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.AuditTrail
{
    public class AuditTrail : BaseEntityExtension
    {
        public int EntityId { get; set; }
        public string Entity { get; set; }
        public DateTime ActionTime { get; set; }
        public string Actor { get; set; }
        public string AactionDetails { get; set; }
        public string Action { get; set; }
    }
}
