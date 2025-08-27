using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using EcoLift.Models.Enums;

namespace EcoLift.Models
{
    public class ApplicationUser : IdentityUser
    {
        // IdentityUser already provides: Id, UserName, Email, PhoneNumber, PasswordHash
        
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? ProfilePicture { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        
        [StringLength(500)]
        public string? Bio { get; set; }
        
        // Travel Preferences
        public ChattinessLevel ChattinessLevel { get; set; } = ChattinessLevel.Moderate;
        public MusicPreference MusicPreference { get; set; } = MusicPreference.Moderate;
        public SmokingPolicy SmokingPolicy { get; set; } = SmokingPolicy.NoSmoking;
        public PetPolicy PetPolicy { get; set; } = PetPolicy.NoPets;
        
        // Navigation properties
        public virtual ICollection<Trip> TripsAsProvider { get; set; } = new List<Trip>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        
        // Conversation navigation properties
        public virtual ICollection<Conversation> ConversationsAsDriver { get; set; } = new List<Conversation>();
        public virtual ICollection<Conversation> ConversationsAsPassenger { get; set; } = new List<Conversation>();
        
        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";
    }
}
