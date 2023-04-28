using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Email
{
    public class EmailAccountDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IncomingProtocol { get; set; }
        public string IncomingServer { get; set; }
        public string IncomingPort { get; set; }
        public string IncomingSsl { get; set; }
        public string OutgoingProtocol { get; set; }
        public string OutgoingServer { get; set; }
        public string OutgoingPort { get; set; }
        public string OutgoingSsl { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastCheckedTime { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
    }
}
