using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoLift.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        
        [Required]
        [ForeignKey("Trip")]
        public int TripId { get; set; }
        
        [Required]
        [ForeignKey("Reviewer")]
        public string ReviewerId { get; set; } = string.Empty;
        
        [Required]
        [ForeignKey("ReviewedUser")]
        public string ReviewedUserId { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }
        
        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual Trip Trip { get; set; } = null!;
        public virtual ApplicationUser Reviewer { get; set; } = null!;
        public virtual ApplicationUser ReviewedUser { get; set; } = null!;
    }
}
