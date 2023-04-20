using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Email
{
    public class EmailAccount : BaseEntityExtension
    {
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
    }
}
