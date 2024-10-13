using Epson.Core.Domain.Requests;
using Epson.Services.DTO.Requests;

namespace Epson.Model.Request
{
    public class RequestModel
    {
        public int Id { get; set; }
        public DateTime? ApprovedTime { get; set; }
        public string? ApprovedBy { get; set; }
        public string? FinalApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? AmendQuotationTime { get; set; }
        public string Segment { get; set; }
        public decimal TotalBudget { get; set; }
        public int ApprovalState { get; set; }
        public string? ApprovalStateStr { get; set; }
        public Decimal? TotalPrice { get; set; }
        public TimeSpan? TimeToResolution { get; set; }
        public bool Breached { get; set; }
        public DateTime? CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedById { get; set; }
        public string? UpdatedById { get; set; }
        public string? Comments { get; set; }
        public string? CreatedTeam { get; set; }
        public int TeamId { get; set; }
        public List<RequestProductModel> RequestProductsModel { get; set; } = new List<RequestProductModel>();
        public List<CompetitorInformationModel> CompetitorInformationModel { get; set; } = new List<CompetitorInformationModel>();
        public RequestSubmissionDetailModel RequestSubmissionDetailModel { get; set; } = new RequestSubmissionDetailModel();
        public ProjectInformationDTO ProjectInformationModel { get; set; } = new ProjectInformationDTO();
    }
}
