using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demologin.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🔗 Product (Guid, matches Product.Id)
        [Required]
        public Guid ProductId { get; set; }
        public virtual Product? Product { get; set; }

        // 🔗 User (string, matches IdentityUser.Id)
        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required, DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }


        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending"; // Default status
    }
}
