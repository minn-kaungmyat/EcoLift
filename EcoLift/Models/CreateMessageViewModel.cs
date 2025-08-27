using System.ComponentModel.DataAnnotations;

namespace EcoLift.Models
{
    public class CreateMessageViewModel
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? SenderEmail { get; set; }

        public bool IsGuest { get; set; }

        // For display
        public Trip? Trip { get; set; }
    }
}
