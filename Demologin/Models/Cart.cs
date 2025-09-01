using System;
using System.ComponentModel.DataAnnotations;

namespace Demologin.Models
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; }   // FK to IdentityUser
        public ApplicationUser User { get; set; }

        [Required]
        public Guid ProductId { get; set; }  // FK to Product
        public Product Product { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
