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

        public async Task<List<RequesterSales>> GetMonthlySalesByRequester(string requesterId)
        {
            var monthlySales = _requestRepository.Table
                .Where(r => r.ApprovalState == 30 && r.CreatedById == requesterId)
                .GroupBy(r => new
                {
                    Month = r.CreatedOnUTC.ToString("yyyy-MM"),
                    Requester = r.CreatedById
                })
                .Select(g => new RequesterSales
                {
                    Month = g.Key.Month,
                    RequesterName = g.Key.Requester,
                    MonthlySales = g.Sum(r => r.TotalPrice)
                })
                .ToList();

            return monthlySales;
        }

        public async Task<List<RequesterSales>> GetTopRequestersBySales()
        {
            var topRequesters = _requestRepository.Table
                .Where(r => r.ApprovalState == 30)
                .GroupBy(r => r.CreatedById)
                .Select(g => new RequesterSales
                {
                    RequesterId = g.Key,
                    TotalNumberOfSales = g.Count(),
                    TotalSales = g.Sum(r => r.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(10)
                .ToList();

            foreach (var requester in topRequesters)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(requester.RequesterId);
                requester.RequesterName = user?.UserName ?? "Unknown";
            }

            return topRequesters;
        }

        public async Task<List<ProductRevenue>> GetTopProductsByRevenue()
        {
            var topProducts = _requestRepository.Table
                .Where(r => r.ApprovalState == 30)
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
                    (j, p) => new { ProductName = p.Name, TotalRevenue = j.RequestProduct.Quantity * j.RequestProduct.FulfilledPrice }
                )
                .GroupBy(
                    j => j.ProductName,
                    (key, group) => new ProductRevenue
                    {
                        ProductName = key,
                        TotalRevenue = group.Sum(x => x.TotalRevenue)
                    }
                )
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            return topProducts;
        }


    }
}