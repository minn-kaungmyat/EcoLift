using System.ComponentModel.DataAnnotations;

namespace EcoLift.Models
{
    public class SearchHistory
    {
        public int Id { get; set; }
        
        [MaxLength(200)]
        public string? FromLocation { get; set; }
        
        public double? FromLatitude { get; set; }
        public double? FromLongitude { get; set; }
        
        [MaxLength(200)]
        public string? ToLocation { get; set; }
        
        public double? ToLatitude { get; set; }
        public double? ToLongitude { get; set; }
        
        public DateTime? DepartureDate { get; set; }
        public int? Passengers { get; set; }
        public decimal? MaxPrice { get; set; }
        public int SearchRadius { get; set; } = 10;
        public bool AllowSmoking { get; set; }
        public bool AllowPets { get; set; }
        public bool OnlyDirectRoutes { get; set; }
        
        public DateTime SearchedAt { get; set; }
        
        [MaxLength(450)]
        public string? UserId { get; set; }
    }
}
