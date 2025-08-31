using System.Globalization;
using CsvHelper.Configuration;

namespace Husam.SalesDashboard.Models.Imports
{
    public class SaleCsvRow
    {
        public string? Customer { get; set; }
        public string? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime SoldAt { get; set; }
    }

    // Maps CSV headers to properties + parsing
    public sealed class SaleCsvRowMap : ClassMap<SaleCsvRow>
    {
        public SaleCsvRowMap()
        {
            Map(m => m.Customer).Name("Customer");
            Map(m => m.Product).Name("Product");
            Map(m => m.Quantity).Name("Quantity");
            Map(m => m.UnitPrice).Name("UnitPrice").TypeConverterOption.CultureInfo(CultureInfo.InvariantCulture);
            Map(m => m.SoldAt).Name("SoldAt")
                .TypeConverterOption.Format("yyyy-MM-dd", "yyyy/MM/dd", "dd-MM-yyyy", "M/d/yyyy");
        }
    }
}
