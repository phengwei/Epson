﻿using Epson.Core.Domain.Requests;

namespace Epson.Model.Request
{
    public class RequestModel
    {
        public int Id { get; set; }
        public DateTime ApprovedTime { get; set; }
        public string ApprovedBy { get; set; }
        public string Segment { get; set; }
        public string ManagerId { get; set; }
        public string ManagerName { get; set; }
        public int Quantity { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public Decimal TotalPrice { get; set; }
        public TimeSpan TimeToResolution { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public string CreatedById { get; set; }
        public string UpdatedById { get; set; }
        public List<RequestProduct> RequestProducts { get; set; } = new List<RequestProduct>();
    }
}
