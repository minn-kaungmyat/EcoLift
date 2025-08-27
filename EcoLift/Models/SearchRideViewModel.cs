using System.ComponentModel.DataAnnotations;

namespace EcoLift.Models
{
    public class SearchRideViewModel
    {
        [Display(Name = "From")]
        public string? FromLocation { get; set; }

        [Display(Name = "From Latitude")]
        public double? FromLatitude { get; set; }

        [Display(Name = "From Longitude")]
        public double? FromLongitude { get; set; }

        [Display(Name = "To")]
        public string? ToLocation { get; set; }

        [Display(Name = "To Latitude")] 
        public double? ToLatitude { get; set; }

        [Display(Name = "To Longitude")]
        public double? ToLongitude { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }

        [Display(Name = "Passengers")]
        [Range(1, 8, ErrorMessage = "Number of passengers must be between 1 and 8")]
        public int? Passengers { get; set; }

        [Display(Name = "Max Price")]
        [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10000")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Search Radius (km)")]
        [Range(1, 100, ErrorMessage = "Search radius must be between 1 and 100 km")]
        public int SearchRadius { get; set; } = 25; // Default 25km radius

        // For advanced search
        public bool AllowSmoking { get; set; }
        public bool AllowPets { get; set; }
        public bool OnlyDirectRoutes { get; set; } = true;
    }
}
