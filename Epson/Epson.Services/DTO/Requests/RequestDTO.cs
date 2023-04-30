namespace Epson.Services.DTO.Requests
{
    public class RequestDTO
    {
        public int Id { get; set; }
        public DateTime ApprovedTime { get; set; }
        public int ApprovedBy { get; set; }
        public string Segment { get; set; }
        public string ManagerId { get; set; }
        public string ManagerName { get; set; }
        public int Quantity { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Decimal TotalPrice { get; set; }
        public DateTime TimeToResolution { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
    }
}
