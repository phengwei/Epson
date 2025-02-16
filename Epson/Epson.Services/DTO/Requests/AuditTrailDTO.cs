using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Requests
{
    public class AuditTrailDTO
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public string Entity { get; set; }
        public DateTime ActionTime { get; set; }
        public string Actor { get; set; }
        public string ActorStr { get; set; }
        public string ActionDetails { get; set; }
        public string Action { get; set; }
        public DateTime CreatedOnUTC { get; set; }
    }

}
