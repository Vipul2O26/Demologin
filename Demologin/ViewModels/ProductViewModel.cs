using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Demologin.ViewModels
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }  // ✅ Add this property

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }  // ✅ keep old image

        [Display(Name = "Upload Product Photo")]
        public IFormFile? ImageFile { get; set; }
    }
}
