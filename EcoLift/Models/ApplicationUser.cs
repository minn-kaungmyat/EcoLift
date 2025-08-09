using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
        
        // Navigation properties
        public virtual ICollection<Trip> TripsAsProvider { get; set; } = new List<Trip>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        
        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";
    }
}
