﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Core.Domain.Requests
{
    public class RequestProduct
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? DistyPrice { get; set; }
        public decimal? DealerPrice { get; set; }
        public decimal? EndUserPrice { get; set; }
        public string? FulfillerId { get; set; }
        public decimal FulfilledPrice { get; set; }
        public bool HasFulfilled { get; set; }
        public DateTime FulfilledDate { get; set; }
        public TimeSpan TimeToResolution { get; set; }
        public int Status { get; set; }
        public string? Remarks { get; set; }
        public bool Breached { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public bool IsCoverplus { get; set; }
        public bool HasReminded { get; set; }
    }
}
