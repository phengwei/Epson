using Epson.Core.Domain.Requests;

namespace Epson.Services.DTO.Requests
{
    public class RequestDTO
    {
        public int Id { get; set; }
        public DateTime ApprovedTime { get; set; }
        public string ApprovedBy { get; set; }
        public string Segment { get; set; }
        public decimal TotalBudget { get; set; }
        public int ApprovalState { get; set; }
        public Decimal TotalPrice { get; set; }
        public TimeSpan TimeToResolution { get; set; }
        public bool Breached { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public string Comments { get; set; }
        public List<RequestProductDTO> RequestProducts { get; set; } = new List<RequestProductDTO>();
        public List<CompetitorInformation> CompetitorInformations { get; set; } = new List<CompetitorInformation>();
        public RequestSubmissionDetail RequestSubmissionDetail { get; set; } = new RequestSubmissionDetail();
        public ProjectInformationDTO ProjectInformation { get; set; } = new ProjectInformationDTO();
    }
}
