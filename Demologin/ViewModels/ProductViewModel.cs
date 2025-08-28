using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // required for IFormFile

namespace Demologin.ViewModels
{
    public class ProductViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        // Upload field
        [Display(Name = "Upload Product Photo")]
        public IFormFile? ImageFile { get; set; }   // ✅ use IFormFile
    }
}
