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
        public string Make { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser Owner { get; set; } = null!;
        public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
