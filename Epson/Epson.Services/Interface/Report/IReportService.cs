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
        public Task<List<RequesterSales>> GetMonthlySalesByRequester(string requesterId, int month = 0, bool allRequester = false);
        Task<List<RequesterSales>> GetMonthlySalesByRequesterByDonut(DateTime fromMonth, DateTime toMonth, bool allRequester = false);
        public Task<List<RequesterSales>> GetTopRequestersBySales(int month);
        //public Task<List<ProductRevenue>> GetTopProductsByRevenue(int month);
        Task<List<ProductRevenue>> GetMonthlySalesByProductByDonut(DateTime fromMonth, DateTime toMonth, bool allProducts = false);
        Task<List<ProductRevenue>> GetMonthlySalesByProduct(int productId, int month = 0, bool allProducts = false);
    }
}
