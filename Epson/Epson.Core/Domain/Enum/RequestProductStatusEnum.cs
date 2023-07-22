using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Enum
{
    public enum RequestProductStatusEnum
    {
        [Description("Pending")]
        Pending = 0, 
        [Description("Cancelled")]
        Cancelled = 10,
        [Description("Rejected")]
        Rejected = 20,
        [Description("Approved")]
        Approved = 30,
    }
}
