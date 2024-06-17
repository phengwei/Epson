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
            var last12Months = new List<DateTime>();
            for (int i = 0; i < 12; i++)
            {
                last12Months.Add(DateTime.UtcNow.AddMonths(-i));
            }
            last12Months = last12Months.Select(d => new DateTime(d.Year, d.Month, 1)).OrderBy(d => d).ToList();

            var query = _requestRepository.Table.AsQueryable();

            if (month != 0)
            {
                query = query.Where(r => r.CreatedOnUTC.Month == month);
            }

            if (!allRequester)
            {
                query = query.Where(r => r.CreatedById == requesterId);
            }

            var salesData = query
                .GroupBy(r => new
                {
                    YearMonth = new DateTime(r.CreatedOnUTC.Year, r.CreatedOnUTC.Month, 1),
                    Requester = r.CreatedById
                })
                .Select(g => new RequesterSales
                {
                    Date = g.Key.YearMonth,
                    RequesterName = _userManager.FindByIdAsync(g.Key.Requester).Result.UserName,
                    MonthlySales = g.Sum(r => r.TotalBudget),
                    TotalNumberOfSales = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            var result = last12Months.Select(m => new RequesterSales
            {
                Date = m,
                RequesterName = salesData.FirstOrDefault(s => s.Date == m)?.RequesterName ?? "",
                MonthlySales = salesData.FirstOrDefault(s => s.Date == m)?.MonthlySales ?? 0,
                TotalNumberOfSales = salesData.FirstOrDefault(s => s.Date == m)?.TotalNumberOfSales ?? 0
            }).ToList();

            return await Task.FromResult(result);
            
        }

        //public async Task<List<RequesterSales>> GetMonthlySalesByRequesterOnDonut(int month = 0, bool allRequester = false)
        //{
        //    List<RequesterSales> monthlySales = new List<RequesterSales>();
        //    var query = _requestRepository.Table.AsQueryable();

        //    if (month != 0)
        //    {
        //        query = query.Where(r => r.CreatedOnUTC.Month == month);
        //    }

        //    if (allRequester && month != 0)
        //    {
        //        monthlySales = query
        //            .GroupBy(r => r.CreatedById)
        //            .Select(g => new RequesterSales
        //            {
        //                RequesterId = g.Key,
        //                RequesterName = _userManager.FindByIdAsync(g.Key).Result.UserName,
        //                TotalNumberOfSales = g.Count(),
        //                MonthlySales = g.Sum(r => r.TotalBudget)
        //            })
        //            .OrderByDescending(x => x.TotalNumberOfSales)
        //            .ToList();
        //    }
        //    else
        //    {
        //        monthlySales = query
        //            .GroupBy(r => new
        //            {
        //                YearMonth = new DateTime(r.CreatedOnUTC.Year, r.CreatedOnUTC.Month, 1),
        //                Requester = r.CreatedById
        //            })
        //            .Select(g => new RequesterSales
        //            {
        //                Date = g.Key.YearMonth,
        //                RequesterName = _userManager.FindByIdAsync(g.Key.Requester).Result.UserName,
        //                MonthlySales = g.Sum(r => r.TotalBudget),
        //                TotalNumberOfSales = g.Count()
        //            })
        //            .OrderBy(x => x.Date)
        //            .ToList();
        //    }

        //    return monthlySales;
        //}

        public async Task<List<RequesterSales>> GetMonthlySalesByRequesterByDonut(DateTime fromMonth, DateTime toMonth, bool allRequester = false)
        {
            try
            {
                var query = _requestRepository.Table
                        .Where(r => r.CreatedOnUTC >= fromMonth && r.CreatedOnUTC <= toMonth);

                var groupedData = query
                    .GroupBy(r => r.CreatedById)
                    .Select(g => new
                    {
                        RequesterId = g.Key,
                        MonthlySales = g.Sum(r => r.TotalBudget),
                        TotalNumberOfSales = g.Count()
                    })
                    .OrderByDescending(x => x.TotalNumberOfSales)
                    .ToList(); // Synchronously fetch the grouped data

                var requesterSalesList = new List<RequesterSales>();

                foreach (var data in groupedData)
                {
                    var user = await _userManager.FindByIdAsync(data.RequesterId);
                    requesterSalesList.Add(new RequesterSales
                    {
                        RequesterId = data.RequesterId,
                        RequesterName = user?.UserName ?? "Unknown",
                        MonthlySales = data.MonthlySales,
                        TotalNumberOfSales = data.TotalNumberOfSales
                    });
                }

                return requesterSalesList;
            }
            catch(Exception ex)
            {
                return new List<RequesterSales>();
            }

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
                .GroupBy(r => new { r.CreatedById, YearMonth = new DateTime(r.CreatedOnUTC.Year, r.CreatedOnUTC.Month, 1) })
                .Select(g => new RequesterSales
                {
                    RequesterId = g.Key.CreatedById,
                    TotalNumberOfSales = g.Count(),
                    TotalSales = g.Sum(r => r.TotalBudget),
                    Date = g.Key.YearMonth
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