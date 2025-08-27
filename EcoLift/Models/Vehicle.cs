using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoLift.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }
        
        [Required]
        [ForeignKey("Owner")]
        public string OwnerId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ApplicationUser Owner { get; set; } = null!;
        public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
