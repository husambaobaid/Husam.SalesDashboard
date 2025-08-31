using System.Globalization;
using Husam.SalesDashboard.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Husam.SalesDashboard.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var totalRevenue = await _context.Sales
                .SumAsync(s => (decimal?)s.UnitPriceAtSale * s.Quantity) ?? 0m;

            var salesCount = await _context.Sales.CountAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.SalesCount = salesCount;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RevenueByMonth()
        {
            var data = await _context.Sales
                .GroupBy(s => new { s.SoldAt.Year, s.SoldAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.UnitPriceAtSale * x.Quantity)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // ✅ Format labels in C# (not in SQL)
            var result = data
                .Select(x => new
                {
                    Label = new DateTime(x.Year, x.Month, 1).ToString("yyyy-MM"),
                    x.Total
                });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> TopProducts()
        {
            var data = await _context.Sales
                .GroupBy(s => s.Product!.Name)
                .Select(g => new
                {
                    Product = g.Key!,
                    Total = g.Sum(x => x.UnitPriceAtSale * x.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
    }
}
