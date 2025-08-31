using System.ComponentModel.DataAnnotations;

namespace Husam.SalesDashboard.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
    }
}
