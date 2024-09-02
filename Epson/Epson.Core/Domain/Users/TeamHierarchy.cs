using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Users
{
    public class TeamHierarchy
    {
        public int ID { get; set; }
        public string RequestingTeam { get; set; }
        public string ApproverTeam { get; set; }
        public bool IsSalesHead{ get; set; }
        public int ApprovalLevel { get; set; }
        public string EmailRecipient { get; set; }
    }
}
