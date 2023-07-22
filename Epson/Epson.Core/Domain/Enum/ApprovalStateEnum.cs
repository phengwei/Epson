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

        PendingSalesSectionHeadFinalAction = 40,

        Approved = 50,

        AmendQuotation = 60,

        RejectedByFulfiller = 70,
        
        RejectedByRequester = 80,

        Cancelled = 90
    }
}
