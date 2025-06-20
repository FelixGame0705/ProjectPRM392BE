using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesApp.Models.Entities
{
    [Table("Carts")]
    public class Cart
    {
        [Key]
        public int CartID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}