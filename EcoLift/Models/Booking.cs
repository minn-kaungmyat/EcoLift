using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoLift.Models.Enums;

namespace EcoLift.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        
        [Required]
        [ForeignKey("Trip")]
        public int TripId { get; set; }
        
        [Required]
        [ForeignKey("Seeker")]
        public string SeekerId { get; set; } = string.Empty;
        
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        
        [Required]
        [Range(1, 8)]
        public int SeatsBooked { get; set; } = 1;
        
        // Navigation properties
        public virtual Trip Trip { get; set; } = null!;
        public virtual ApplicationUser Seeker { get; set; } = null!;
        
        // Computed properties
        public decimal TotalCost => Trip?.PricePerSeat * SeatsBooked ?? 0;
        public bool IsActive => Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;
    }
}
