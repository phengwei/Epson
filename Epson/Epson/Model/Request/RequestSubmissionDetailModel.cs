namespace Epson.Model.Request
{
    public class RequestSubmissionDetailModel
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string? DistributorName { get; set; }
        public string? ResellerName { get; set; }
        public string? ContactPersonName { get; set; }
        public string? TelephoneNo { get; set; }
        public string? FaxNo { get; set; }
        public string? Email { get; set; }
        public DateTime? CreatedOnUTC { get; set; }
        public string? CreatedBy { get; set; }
        public string? PreparedBy { get; set; }
    }
}
