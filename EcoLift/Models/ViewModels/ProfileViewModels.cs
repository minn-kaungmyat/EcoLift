using System.ComponentModel.DataAnnotations;
using EcoLift.Models.Enums;

namespace EcoLift.Models.ViewModels
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Vehicle> Vehicles { get; set; } = new();
        public bool IsOwnProfile { get; set; } = true;
    }

    public class EditPersonalDetailsViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        [Display(Name = "Bio")]
        public string? Bio { get; set; }
    }

    public class EditTravelPreferencesViewModel
    {
        [Display(Name = "Chattiness Level")]
        public ChattinessLevel ChattinessLevel { get; set; } = ChattinessLevel.Moderate;

        [Display(Name = "Music Preference")]
        public MusicPreference MusicPreference { get; set; } = MusicPreference.Moderate;

        [Display(Name = "Smoking Policy")]
        public SmokingPolicy SmokingPolicy { get; set; } = SmokingPolicy.NoSmoking;

        [Display(Name = "Pet Policy")]
        public PetPolicy PetPolicy { get; set; } = PetPolicy.NoPets;
    }

    public class AddVehicleViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Color")]
        public string Color { get; set; } = string.Empty;
    }

    public class EditVehicleViewModel : AddVehicleViewModel
    {
        public int VehicleId { get; set; }
    }
}
