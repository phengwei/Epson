using Epson.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class Request : BaseEntityExtension
    {
        public DateTime ApprovedTime { get; set; }
        public string? ApprovedBy { get; set; }
        public string? CreatedByStr { get; set; }
        public DateTime? AmendQuotationTime { get; set; }
        public string? Segment { get; set; }
        public decimal TotalBudget { get; set; }
        public int ApprovalState { get; set; }
        public Decimal TotalPrice { get; set; }
        public TimeSpan TimeToResolution { get; set; }
        public bool Breached { get; set; }
        public string? Comments { get; set; }
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string SLA { get; set; }
        public virtual ICollection<CompetitorInformation> CompetitorInformations { get; set; }
        public virtual ICollection<RequestProduct> RequestProducts { get; set; }
        public virtual RequestSubmissionDetail RequestSubmissionDetail { get; set; }
        public virtual ProjectInformation ProjectInformation { get; set; }
    }

}
