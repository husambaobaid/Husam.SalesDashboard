using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Husam.SalesDashboard.Data;
using Husam.SalesDashboard.Models;
using CsvHelper;
using Husam.SalesDashboard.Models.Imports;
using System.Globalization;
using ClosedXML.Excel;

namespace Husam.SalesDashboard.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Sales?year=2025
        public async Task<IActionResult> Index(int? year)
        {
            var salesQuery = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .AsQueryable();

            // Build Year dropdown from all sales (distinct years)
            var years = await _context.Sales
                .Select(s => s.SoldAt.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToListAsync();

            if (year.HasValue)
                salesQuery = salesQuery.Where(s => s.SoldAt.Year == year.Value);

            ViewBag.Years = years;
            ViewBag.SelectedYear = year;

            var items = await salesQuery
                .OrderByDescending(s => s.SoldAt)
                .ToListAsync();

            return View(items);
        }

        // GET: Sales/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // GET: Sales/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerId,ProductId,Quantity,UnitPriceAtSale,SoldAt")] Sale sale)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", sale.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        // GET: Sales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", sale.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        // POST: Sales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,ProductId,Quantity,UnitPriceAtSale,SoldAt")] Sale sale)
        {
            if (id != sale.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sale);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaleExists(sale.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", sale.CustomerId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", sale.ProductId);
            return View(sale);
        }

        // GET: Sales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale != null)
            {
                _context.Sales.Remove(sale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.Id == id);
        }

        // GET: /Sales/Upload
        public IActionResult Upload()
        {
            return View(); // simple file upload form
        }

        // POST: /Sales/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["UploadError"] = "Please choose a CSV file.";
                return RedirectToAction(nameof(Upload));
            }

            int created = 0, updatedCustomers = 0, updatedProducts = 0, skipped = 0;
            var errors = new List<string>();

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<SaleCsvRowMap>();

            List<SaleCsvRow> rows;
            try
            {
                rows = csv.GetRecords<SaleCsvRow>().ToList();
            }
            catch (Exception ex)
            {
                TempData["UploadError"] = $"Invalid CSV format: {ex.Message}";
                return RedirectToAction(nameof(Upload));
            }

            // Optional: transaction for integrity
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var r in rows)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(r.Customer) || string.IsNullOrWhiteSpace(r.Product) || r.Quantity <= 0)
                        {
                            skipped++; continue;
                        }

                        // find or create Customer
                        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == r.Customer!);
                        if (customer == null)
                        {
                            customer = new Customer { Name = r.Customer!, Email = null };
                            _context.Customers.Add(customer);
                            updatedCustomers++;
                        }

                        // find or create Product
                        var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == r.Product!);
                        if (product == null)
                        {
                            product = new Product { Name = r.Product!, UnitPrice = r.UnitPrice };
                            _context.Products.Add(product);
                            updatedProducts++;
                        }

                        // persist new (so IDs exist) without committing entire tx yet
                        await _context.SaveChangesAsync();

                        var sale = new Sale
                        {
                            CustomerId = customer.Id,
                            ProductId = product.Id,
                            Quantity = r.Quantity,
                            UnitPriceAtSale = r.UnitPrice,       // price at sale time
                            SoldAt = r.SoldAt
                        };

                        _context.Sales.Add(sale);
                        created++;
                    }
                    catch (Exception innerEx)
                    {
                        skipped++;
                        errors.Add(innerEx.Message);
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["UploadOk"] = $"Imported {created} sales. New customers: {updatedCustomers}, new products: {updatedProducts}, skipped: {skipped}.";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["UploadError"] = $"Upload failed: {ex.Message}";
            }

            // back to Sales index or show results page
            return RedirectToAction(nameof(Index));
        }

        // GET: /Sales/ExportCsv
        public async Task<FileResult> ExportCsv(int? year = null)
        {
            var query = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .AsQueryable();

            if (year.HasValue)
                query = query.Where(s => s.SoldAt.Year == year.Value);

            var rows = await query
                .OrderBy(s => s.SoldAt)
                .Select(s => new
                {
                    Customer = s.Customer!.Name,
                    Product = s.Product!.Name,
                    s.Quantity,
                    s.UnitPriceAtSale,
                    s.SoldAt,
                    Total = s.UnitPriceAtSale * s.Quantity
                })
                .ToListAsync();

            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
            }
            ms.Position = 0;

            var fileName = year.HasValue ? $"Sales_{year}.csv" : "Sales_All.csv";
            return File(ms.ToArray(), "text/csv", fileName);
        }

        // GET: /Sales/ExportExcel
        public async Task<FileResult> ExportExcel(int? year = null)
        {
            var query = _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Product)
                .AsQueryable();

            if (year.HasValue)
                query = query.Where(s => s.SoldAt.Year == year.Value);

            var rows = await query
                .OrderBy(s => s.SoldAt)
                .Select(s => new
                {
                    Customer = s.Customer!.Name,
                    Product = s.Product!.Name,
                    s.Quantity,
                    s.UnitPriceAtSale,
                    s.SoldAt,
                    Total = s.UnitPriceAtSale * s.Quantity
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Sales");
            ws.Cell(1, 1).InsertTable(rows);
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            var fileName = year.HasValue ? $"Sales_{year}.xlsx" : "Sales_All.xlsx";
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // Optional: quick link to export the *currently selected year* from Dashboard
        // e.g., /Sales/ExportForDashboard?year=2025&type=csv|excel
        public IActionResult ExportForDashboard(int? year, string type = "csv")
        {
            return type.Equals("excel", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(ExportExcel), new { year })
                : RedirectToAction(nameof(ExportCsv), new { year });
        }
    }
}
