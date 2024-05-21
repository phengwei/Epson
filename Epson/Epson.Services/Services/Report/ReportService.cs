using Epson.Core.Domain.Enum;
using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Report;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Epson.Services.Services.Report
{
    public class ReportService : IReportService
    {
        private readonly IRepository<Request> _requestRepository;
        private readonly IRepository<RequestProduct> _requestProductRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportService(
            IRepository<Request> requestRepository,
            IRepository<RequestProduct> requestProductRepository,
            IRepository<Product> productRepository,
            UserManager<ApplicationUser> userManager)
        {
            _requestRepository = requestRepository;
            _requestProductRepository = requestProductRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        public async Task<List<RequesterSales>> GetMonthlySalesByRequester(string requesterId, int month = 0, bool allRequester = false)
        {
            if (allRequester)
            {
                return await GetTopRequestersBySales(month);
            }

            List<RequesterSales> monthlySales = new List<RequesterSales>();

            var query = _requestRepository.Table.AsQueryable();

            if (month != 0)
            {
                query = query.Where(r => r.CreatedOnUTC.Month == month);
            }

            if (!allRequester)
            {
                query = query.Where(r => r.CreatedById == requesterId);
            }

            monthlySales = query
                .GroupBy(r => new
                {
                    Month = r.CreatedOnUTC.ToString("yyyy-MM"),
                    Requester = r.CreatedById
                })
                .Select(g => new RequesterSales
                {
                    Month = g.Key.Month,
                    RequesterName = _userManager.FindByIdAsync(g.Key.Requester).Result.UserName,
                    MonthlySales = g.Sum(r => r.TotalBudget),
                    TotalNumberOfSales = g.Count()
                })
                .OrderByDescending(x => x.TotalNumberOfSales)
                .ToList();

            return monthlySales;
        }


        public async Task<List<RequesterSales>> GetTopRequestersBySales(int month)
        {
            var query = _requestRepository.Table
                .Where(r => r.ApprovalState == (int)ApprovalStateEnum.Approved);

            if (month != 0)
            {
                query = query.Where(r => r.CreatedOnUTC.Month == month && r.CreatedOnUTC.Year == DateTime.UtcNow.Year);
            }

            var topRequesters = query
                .GroupBy(r => r.CreatedById)
                .Select(g => new RequesterSales
                {
                    RequesterId = g.Key,
                    TotalNumberOfSales = g.Count(),
                    TotalSales = g.Sum(r => r.TotalBudget)
                })
                .OrderByDescending(x => x.TotalNumberOfSales)
                .Take(25)
                .ToList();

            foreach (var requester in topRequesters)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(requester.RequesterId);
                requester.RequesterName = user?.UserName ?? "Unknown";
            }

            return topRequesters;
        }

        public async Task<List<ProductRevenue>> GetTopProductsByRevenue(int month)
        {
            var query = _requestRepository.Table
                .Where(r => r.ApprovalState == (int)ApprovalStateEnum.Approved);

            if (month != 0)
            {
                query = query.Where(r => r.CreatedOnUTC.Month == month && r.CreatedOnUTC.Year == DateTime.UtcNow.Year);
            }

            var topProducts = query
                .Join(
                    _requestProductRepository.Table,
                    r => r.Id,
                    rp => rp.RequestId,
                    (r, rp) => new { Request = r, RequestProduct = rp }
                )
                .Join(
                    _productRepository.Table,
                    j => j.RequestProduct.ProductId,
                    p => p.Id,
                    (j, p) => new { ProductName = p.Name, TotalRevenue = j.RequestProduct.Quantity * j.RequestProduct.DealerPrice, Quantity = j.RequestProduct.Quantity }
                )
                .GroupBy(
                    j => j.ProductName,
                    (key, group) => new ProductRevenue
                    {
                        ProductName = key,
                        TotalRevenue = group.Sum(x => x.TotalRevenue),
                        TotalNoOfSales = group.Sum(x => x.Quantity)
                    }
                )
                .OrderByDescending(x => x.TotalNoOfSales)
                .Take(10)
                .ToList();

            return topProducts;
        }
    }
}