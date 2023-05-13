using Epson.Services.DTO.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epson.Services.Interface.Report
{
    public interface IReportService
    {
        public Task<List<RequesterSales>> GetMonthlySalesByRequester();
        public Task<List<RequesterSales>> GetTopRequestersBySales();
        public Task<List<ProductRevenue>> GetTopProductsByRevenue();
    }
}
