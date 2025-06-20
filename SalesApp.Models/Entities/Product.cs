using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesApp.Models.Entities
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; }

        [StringLength(255)]
        public string? BriefDescription { get; set; }

        public string? FullDescription { get; set; }

        public string? TechnicalSpecifications { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string? ImageURL { get; set; }

        public int? CategoryID { get; set; }

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}