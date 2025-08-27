using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoLift.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        
        [Required]
        [ForeignKey("Conversation")]
        public int ConversationId { get; set; }
        
        [Required]
        [ForeignKey("Sender")]
        public string SenderId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        // Navigation properties
        public virtual Conversation Conversation { get; set; } = null!;
        public virtual ApplicationUser Sender { get; set; } = null!;
        
        // Computed property to get sender display name
        public string SenderDisplayName => Sender?.FullName ?? "Unknown User";
    }
}
