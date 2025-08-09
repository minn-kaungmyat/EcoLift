using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoLift.Models.Enums;

namespace EcoLift.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }
        
        [Required]
        [ForeignKey("Provider")]
        public string ProviderId { get; set; } = string.Empty;
        
        // Departure location fields (inline)
        [Required]
        [StringLength(100)]
        public string DepartureCity { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal DepartureLatitude { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal DepartureLongitude { get; set; }
        
        // Destination location fields (inline)
        [Required]
        [StringLength(100)]
        public string DestinationCity { get; set; } = string.Empty;
        
        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal DestinationLatitude { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal DestinationLongitude { get; set; }
        
        // Trip timing
        [Required]
        public DateTime DepartureTime { get; set; }
        
        [Required]
        public DateTime ArrivalTime { get; set; }
        
        // Trip details
        [Required]
        [Range(1, 8)]
        public int AvailableSeats { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(6,2)")]
        [Range(0, 9999.99)]
        public decimal PricePerSeat { get; set; }
        
        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ApplicationUser Provider { get; set; } = null!;
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        
        // Computed properties for convenience
        public int BookedSeats => Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.SeatsBooked);
        public int RemainingSeats => AvailableSeats - BookedSeats;
        public bool HasAvailableSeats => RemainingSeats > 0;
        public TimeSpan Duration => ArrivalTime - DepartureTime;
    }
}
