using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Email
{
    public class EmailQueue : BaseEntityExtension
    {
        public int Priority { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AttachmentName { get; set; }
        public DateTime ScheduleTime { get; set; }
        public int SendAttempts { get; set; }
        public DateTime SentTime { get; set; }
        public int EmailAccountId { get; set; }

    }
}
