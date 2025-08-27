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
        
        // Pickup location fields
        [Required]
        [StringLength(500)]
        [Display(Name = "Pickup Location")]
        public string PickupLocation { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(9,6)")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        [Display(Name = "Pickup Latitude")]
        public decimal? PickupLatitude { get; set; }
        
        [Column(TypeName = "decimal(9,6)")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        [Display(Name = "Pickup Longitude")]
        public decimal? PickupLongitude { get; set; }
        
        // Dropoff location fields
        [Required]
        [StringLength(500)]
        [Display(Name = "Dropoff Location")]
        public string DropoffLocation { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(9,6)")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        [Display(Name = "Dropoff Latitude")]
        public decimal? DropoffLatitude { get; set; }
        
        [Column(TypeName = "decimal(9,6)")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        [Display(Name = "Dropoff Longitude")]
        public decimal? DropoffLongitude { get; set; }
        
        // Trip timing
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Departure Date")]
        public DateTime DepartureDate { get; set; }
        
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Departure Time")]
        public TimeSpan DepartureTime { get; set; }
        
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
        
        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Allow Smoking")]
        public bool AllowSmoking { get; set; }
        
        [Display(Name = "Allow Pets")]
        public bool AllowPets { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        [Display(Name = "Trip Status")]
        public TripStatus Status { get; set; } = TripStatus.Published;
        
        // Navigation properties
        public virtual ApplicationUser Provider { get; set; } = null!;
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        
        // Computed properties for convenience
        public int BookedSeats => Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.SeatsBooked);
        public int RemainingSeats => AvailableSeats - BookedSeats;
        public bool HasAvailableSeats => RemainingSeats > 0;
        public DateTime DepartureDateTime => DepartureDate.Date.Add(DepartureTime);
        
        // Status-based computed properties
        public bool IsOpenForBookings => Status == TripStatus.Published || Status == TripStatus.PartiallyBooked;
        public bool IsFullyBooked => Status == TripStatus.Full;
        public bool IsCompleted => Status == TripStatus.Completed;
        public bool IsCancelled => Status == TripStatus.Cancelled;
        public bool IsOngoing => Status == TripStatus.Ongoing;
    }
}
