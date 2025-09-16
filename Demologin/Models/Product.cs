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

        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number.")]
        public int Stock { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Threshold must be a non-negative number.")]
        public int Threshold { get; set; } = 0;

        // ✅ FIXED: Added Column data annotation
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        public DateTime? DiscountValidTill { get; set; }

        [NotMapped]
        public decimal DiscountedPrice
        {
            get
            {
                if (DiscountPercentage > 0 &&
                    (!DiscountValidTill.HasValue || DiscountValidTill.Value >= DateTime.UtcNow))
                {
                    return Price - (Price * (DiscountPercentage / 100));
                }
                return Price;
            }
        }

        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}