using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoLift.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        
        [ForeignKey("Sender")]
        public string? SenderId { get; set; } // Nullable for guest messages
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string SenderEmail { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        // Navigation properties
        public virtual ApplicationUser? Sender { get; set; } // Nullable for guest messages
        
        // Computed property to determine if message is from a guest
        public bool IsFromGuest => SenderId == null;
        
        // Computed property to get sender display name
        public string SenderDisplayName => Sender?.FullName ?? SenderEmail;
    }
}
