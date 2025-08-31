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

        // ✅ New endpoint for the Chart.js API
        [HttpGet]
        public async Task<IActionResult> RevenueByMonth()
        {
            var data = await _context.Sales
                .GroupBy(s => new { s.SoldAt.Year, s.SoldAt.Month })
                .Select(g => new
                {
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1)
                                .ToString("yyyy-MM", CultureInfo.InvariantCulture),
                    Total = g.Sum(x => x.UnitPriceAtSale * x.Quantity)
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            return Json(data);
        }
    }
}
