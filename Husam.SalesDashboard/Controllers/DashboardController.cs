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

        // GET: /Dashboard?year=2025
        public async Task<IActionResult> Index(int? year)
        {
            // Build a base query and optionally filter by year
            var salesQuery = _context.Sales.AsQueryable();
            if (year.HasValue)
                salesQuery = salesQuery.Where(s => s.SoldAt.Year == year.Value);

            // KPIs
            var totalRevenue = await salesQuery
                .SumAsync(s => (decimal?)s.UnitPriceAtSale * s.Quantity) ?? 0m;

            var salesCount = await salesQuery.CountAsync();

            // Distinct years for the dropdown (from all sales)
            var years = await _context.Sales
                .Select(s => s.SoldAt.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync();

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.SalesCount = salesCount;
            ViewBag.SelectedYear = year;   // used by view & JS fetches
            ViewBag.Years = years;  // dropdown options

            return View();
        }

        // JSON: /Dashboard/RevenueByMonth?year=2025
        [HttpGet]
        public async Task<IActionResult> RevenueByMonth(int? year)
        {
            var query = _context.Sales.AsQueryable();
            if (year.HasValue)
                query = query.Where(s => s.SoldAt.Year == year.Value);

            // Do grouping/summing in SQL first
            var data = await query
                .GroupBy(s => new { s.SoldAt.Year, s.SoldAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.UnitPriceAtSale * x.Quantity)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Then format labels in C# (avoids SQL translation issues)
            var result = data.Select(x => new
            {
                Label = new DateTime(x.Year, x.Month, 1).ToString("yyyy-MM", CultureInfo.InvariantCulture),
                x.Total
            });

            return Json(result);
        }

        // JSON: /Dashboard/TopProducts?year=2025
        [HttpGet]
        public async Task<IActionResult> TopProducts(int? year)
        {
            var query = _context.Sales.AsQueryable();
            if (year.HasValue)
                query = query.Where(s => s.SoldAt.Year == year.Value);

            var data = await query
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

        // JSON: /Dashboard/TopCustomers?year=2025
        [HttpGet]
        public async Task<IActionResult> TopCustomers(int? year)
        {
            var query = _context.Sales.AsQueryable();
            if (year.HasValue)
                query = query.Where(s => s.SoldAt.Year == year.Value);

            var data = await query
                .GroupBy(s => s.Customer!.Name)
                .Select(g => new
                {
                    Customer = g.Key!,
                    Total = g.Sum(x => x.UnitPriceAtSale * x.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
    }
}
