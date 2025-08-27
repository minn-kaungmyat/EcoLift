using System.ComponentModel.DataAnnotations;

namespace EcoLift.Models
{
    public class CreateBookingViewModel
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        [Range(1, 8, ErrorMessage = "Number of seats must be between 1 and 8")]
        [Display(Name = "Number of Seats")]
        public int SeatsBooked { get; set; } = 1;

        [Display(Name = "Maximum Seats Available")]
        public int MaxSeatsAvailable { get; set; }

        // Navigation property for display purposes
        public Trip? Trip { get; set; }

        // Computed properties
        public decimal TotalCost => Trip?.PricePerSeat * SeatsBooked ?? 0;
        public string FormattedTotalCost => TotalCost.ToString("F2");
    }
}
