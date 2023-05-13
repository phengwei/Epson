using Epson.Core.Domain.Products;
using Epson.Core.Domain.Requests;
using Epson.Core.Domain.Users;
using Epson.Data;
using Epson.Services.DTO.Report;
using Epson.Services.Interface.Report;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<RequesterSales>> GetMonthlySalesByRequester()
    {
        var monthlySales = _requestRepository.Table
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
                (j, p) => new { j.Request, j.RequestProduct, Product = p }
            )
            .GroupBy(
                j => new
                {
                    Month = j.Request.CreatedOnUTC.ToString("yyyy-MM"),
                    RequesterId = j.Request.CreatedById
                }
            )
            .Select(g => new RequesterSales
            {
                Month = g.Key.Month,
                RequesterId = g.Key.RequesterId,
                MonthlySales = g.Sum(x => x.RequestProduct.Quantity * x.Product.Price),
            })
            .ToList();

        foreach (var sales in monthlySales)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(sales.RequesterId);
            sales.RequesterName = user?.UserName ?? "Unknown";
        }

        return monthlySales;
    }

    public async Task<List<RequesterSales>> GetTopRequestersBySales()
    {
        var topRequesters = _requestRepository.Table
            .Where(r => r.ApprovalState == 30)
            .Join(
                _requestProductRepository.Table,
                r => r.Id,
                rp => rp.RequestId,
                (r, rp) => new { Request = r, RequestProduct = rp }
            )
            .GroupBy(
                j => j.Request.CreatedById,
                (key, group) => new RequesterSales
                {
                    RequesterId = key,
                    TotalNumberOfSales = group.Count(),
                    TotalSales = group.Sum(x => x.Request.TotalPrice)
                }
            )
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