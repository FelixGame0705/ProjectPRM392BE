using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesApp.Models.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int? CartID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        [StringLength(255)]
        public string BillingAddress { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CartID")]
        public virtual Cart? Cart { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}