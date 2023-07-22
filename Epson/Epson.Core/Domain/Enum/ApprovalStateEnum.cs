using System;
using System.ComponentModel;

namespace Epson.Core.Domain.Enum
{
    public enum ApprovalStateEnum
    {
        [Description("Pending Sales Section Head Action")]
        PendingSalesSectionHeadAction = 10,

        [Description("Pending Fulfiller Action")]
        PendingFulfillerAction = 20,

        [Description("Pending Requester Action")]
        PendingRequesterAction = 30,

        [Description("Pending Sales Section Head Final Action")]
        PendingSalesSectionHeadFinalAction = 40,

        [Description("Approved")]
        Approved = 50,

        [Description("Amend Quotation")]
        AmendQuotation = 60,

        [Description("Rejected By Fulfiller")]
        RejectedByFulfiller = 70,

        [Description("Rejected By Requester")]
        RejectedByRequester = 80,

        [Description("Cancelled")]
        Cancelled = 90
    }
}