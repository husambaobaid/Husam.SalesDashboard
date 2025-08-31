using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Husam.SalesDashboard.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required] public int CustomerId { get; set; }
        [Required] public int ProductId { get; set; }

        [Range(1, 1_000_000)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPriceAtSale { get; set; }

        public DateTime SoldAt { get; set; } = DateTime.UtcNow;

        // Navigation props
        public Customer? Customer { get; set; }
        public Product? Product { get; set; }

        [NotMapped]
        public decimal Total => UnitPriceAtSale * Quantity;
    }
}
