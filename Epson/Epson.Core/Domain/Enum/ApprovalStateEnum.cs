using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Enum
{
    public enum ApprovalStateEnum
    {
        PendingSalesSectionHeadAction = 10,

        PendingFulfillerAction = 20,

        PendingRequesterAction = 30,

        Approved = 40,

        AmendQuotation = 50,

        Rejected = 60,
        
        Cancelled = 70
    }
}
