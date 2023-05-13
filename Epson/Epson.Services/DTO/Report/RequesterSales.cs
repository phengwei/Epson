using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.DTO.Report
{
    public class RequesterSales
    {
        public string RequesterId { get; set; }
        public string RequesterName { get; set; }
        public int TotalNumberOfSales { get; set; }
        public decimal TotalSales { get; set; }
        public string Month { get; set; }
        public decimal MonthlySales { get; set; }
    }
}
