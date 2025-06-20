using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesApp.Models.Entities
{
    [Table("StoreLocations")]
    public class StoreLocation
    {
        [Key]
        public int LocationID { get; set; }

        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Required]
        [StringLength(255)]
        public string Address { get; set; }
    }
}