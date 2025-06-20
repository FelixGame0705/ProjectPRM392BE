using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesApp.Models.Entities
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        public int? CartID { get; set; }

        public int? ProductID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Navigation properties
        [ForeignKey("CartID")]
        public virtual Cart? Cart { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}