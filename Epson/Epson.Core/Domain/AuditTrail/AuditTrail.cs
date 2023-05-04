using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.AuditTrail
{
    public class AuditTrail
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public string Entity { get; set; }
        public DateTime ActionTime { get; set; }
        public string Actor { get; set; }
        public string ActionDetails { get; set; }
        public string Action { get; set; }
        public DateTime CreatedOnUTC { get; set; }
    }
}
