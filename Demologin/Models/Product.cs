using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Demologin.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required, DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        // FK to Identity user
        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ✅ Navigation to carts
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();

        // ✅ Navigation to orders
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
