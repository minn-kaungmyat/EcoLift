using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcoLift.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }
        
        [Required]
        [ForeignKey("Trip")]
        public int TripId { get; set; }
        
        [Required]
        [ForeignKey("Driver")]
        public string DriverId { get; set; } = string.Empty;
        
        [Required]
        [ForeignKey("Passenger")]
        public string PassengerId { get; set; } = string.Empty;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastMessageAt { get; set; }
        
        // Navigation properties
        public virtual Trip Trip { get; set; } = null!;
        public virtual ApplicationUser Driver { get; set; } = null!;
        public virtual ApplicationUser Passenger { get; set; } = null!;
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        
        // Computed properties
        public string TripRoute => $"{Trip?.PickupLocation} → {Trip?.DropoffLocation}";
        public string OtherParticipantName(string currentUserId) => 
            currentUserId == DriverId ? Passenger.FullName : Driver.FullName;
    }
}
