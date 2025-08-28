using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demologin.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }


        [Required]
        [DataType(DataType.Currency)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Price { get; set; }

        public string UserId { get; set; } 
        public ApplicationUser User { get; set; } 

        public ProductStatus Status { get; set; }
        public string ImageUrl { get; set; }

        [NotMapped]
        public IFormFile Photo { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public enum ProductStatus
    {
        Pending,
        Approved,
        Rejected
    }
}