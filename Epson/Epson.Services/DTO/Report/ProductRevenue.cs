using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Report
{
    public class ProductRevenue
    {
        public string ProductName { get; set; }
        public int TotalNumberOfSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Month { get; set; }
        public decimal MonthlySales { get; set; }
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime Date { get; set; }
    }

}
