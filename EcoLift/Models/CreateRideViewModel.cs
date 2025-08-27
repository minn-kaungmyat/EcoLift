using System.ComponentModel.DataAnnotations;

namespace EcoLift.Models
{
    public class CreateRideViewModel
    {
        [Required(ErrorMessage = "Pickup location is required")]
        [Display(Name = "Pickup Location")]
        public string PickupLocation { get; set; }

        [Required(ErrorMessage = "Dropoff location is required")]
        [Display(Name = "Dropoff Location")]
        public string DropoffLocation { get; set; }

        [Display(Name = "Pickup Latitude")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? PickupLatitude { get; set; }

        [Display(Name = "Pickup Longitude")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? PickupLongitude { get; set; }

        [Display(Name = "Dropoff Latitude")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? DropoffLatitude { get; set; }

        [Display(Name = "Dropoff Longitude")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? DropoffLongitude { get; set; }

        [Required(ErrorMessage = "Departure date is required")]
        [Display(Name = "Departure Date")]
        [DataType(DataType.Date)]
        public DateTime DepartureDate { get; set; }

        [Required(ErrorMessage = "Departure time is required")]
        [Display(Name = "Departure Time")]
        [DataType(DataType.Time)]
        public TimeSpan DepartureTime { get; set; }

        [Required(ErrorMessage = "Number of available seats is required")]
        [Display(Name = "Available Seats")]
        [Range(1, 4, ErrorMessage = "Available seats must be between 1 and 4")]
        public int AvailableSeats { get; set; }

        [Required(ErrorMessage = "Price per seat is required")]
        [Display(Name = "Price per Seat")]
        [Range(0.01, 1000.00, ErrorMessage = "Price must be between €0.01 and €1000.00")]
        [DataType(DataType.Currency)]
        public decimal PricePerSeat { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Display(Name = "Allow Smoking")]
        public bool AllowSmoking { get; set; }

        [Display(Name = "Allow Pets")]
        public bool AllowPets { get; set; }

        // Computed property for departure datetime
        public DateTime DepartureDateTime => DepartureDate.Date.Add(DepartureTime);
    }
}
