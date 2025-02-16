using System;
using System.ComponentModel;

namespace Epson.Core.Domain.Enum
{
    public enum ServiceRequestStatusEnum
    {
        [Description("Pending Assignment")]
        PendingAssignment = 10,

        [Description("Pending Maker Decision")]
        PendingMakerDecision = 20,

        [Description("Pending Checker Decision")]
        PendingCheckerDecision = 30,

        [Description("Closed")]
        Closed = 40,

        [Description("Checker Rejected")]
        CheckerRejected = 50
    }
}