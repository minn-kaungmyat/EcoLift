using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcoLift.Data;
using EcoLift.Models;
using EcoLift.Models.ViewModels;

namespace EcoLift.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var vehicles = await _context.Vehicles
                .Where(v => v.OwnerId == user.Id)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                Vehicles = vehicles
            };

            return View(viewModel);
        }

        // GET: Profile/EditPersonalDetails
        public async Task<IActionResult> EditPersonalDetails()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditPersonalDetailsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Bio = user.Bio
            };

            return View(viewModel);
        }

        // POST: Profile/EditPersonalDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonalDetails(EditPersonalDetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;
            user.Bio = model.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Personal details updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Profile/EditTravelPreferences
        public async Task<IActionResult> EditTravelPreferences()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditTravelPreferencesViewModel
            {
                ChattinessLevel = user.ChattinessLevel,
                MusicPreference = user.MusicPreference,
                SmokingPolicy = user.SmokingPolicy,
                PetPolicy = user.PetPolicy
            };

            return View(viewModel);
        }

        // POST: Profile/EditTravelPreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTravelPreferences(EditTravelPreferencesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.ChattinessLevel = model.ChattinessLevel;
            user.MusicPreference = model.MusicPreference;
            user.SmokingPolicy = model.SmokingPolicy;
            user.PetPolicy = model.PetPolicy;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Travel preferences updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Profile/AddVehicle
        public IActionResult AddVehicle()
        {
            return View(new AddVehicleViewModel());
        }

        // POST: Profile/AddVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVehicle(AddVehicleViewModel car)
        {
            _logger.LogInformation("AddVehicle POST called with model: {@Model}", car);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid: {@ModelStateErrors}", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(car);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("User not found in AddVehicle POST");
                return NotFound();
            }

            _logger.LogInformation("User found: {UserId}", user.Id);

            try
            {
                var vehicle = new Vehicle
                {
                    OwnerId = user.Id,
                    Brand = car.Brand,
                    Model = car.Model,
                    LicensePlate = car.LicensePlate,
                    Color = car.Color
                };

                _logger.LogInformation("Creating vehicle: {@Vehicle}", vehicle);

                _context.Vehicles.Add(vehicle);
                var result = await _context.SaveChangesAsync();
                
                _logger.LogInformation("SaveChanges result: {Result}", result);

                TempData["SuccessMessage"] = "Vehicle added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle");
                TempData["ErrorMessage"] = "An error occurred while adding the vehicle: " + ex.Message;
                return View(car);
            }
        }

        // GET: Profile/EditVehicle/5
        public async Task<IActionResult> EditVehicle(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == id && v.OwnerId == user.Id);

            if (vehicle == null)
            {
                return NotFound();
            }

            var viewModel = new EditVehicleViewModel
            {
                VehicleId = vehicle.VehicleId,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                LicensePlate = vehicle.LicensePlate,
                Color = vehicle.Color
            };

            return View(viewModel);
        }

        // POST: Profile/EditVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(int id, EditVehicleViewModel model)
        {
            if (id != model.VehicleId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == id && v.OwnerId == user.Id);

            if (vehicle == null)
            {
                return NotFound();
            }

            try
            {
                vehicle.Brand = model.Brand;
                vehicle.Model = model.Model;
                vehicle.LicensePlate = model.LicensePlate;
                vehicle.Color = model.Color;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Vehicle updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the vehicle: " + ex.Message;
                return View(model);
            }
        }

        // POST: Profile/DeleteVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == id && v.OwnerId == user.Id);

            if (vehicle == null)
            {
                return NotFound();
            }

            try
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Vehicle deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the vehicle: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Profile/ViewUser/5
        public async Task<IActionResult> ViewUser(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Prevent users from viewing their own profile through this action
            if (currentUser.Id == id)
            {
                return RedirectToAction(nameof(Index));
            }

            // Check if the current user is connected to the target user through rides
            var hasConnection = await HasRideConnection(currentUser.Id, id);
            if (!hasConnection)
            {
                TempData["ErrorMessage"] = "You can only view profiles of users you're connected to through rides.";
                return RedirectToAction("Index", "Ride");
            }

            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                return NotFound();
            }

            var vehicles = await _context.Vehicles
                .Where(v => v.OwnerId == id)
                .ToListAsync();

            // Get ride statistics for the user
            var publishedRides = await _context.Trips
                .Where(t => t.ProviderId == id)
                .CountAsync();

            var completedRides = await _context.Trips
                .Where(t => t.ProviderId == id && t.Status == EcoLift.Models.Enums.TripStatus.Completed)
                .CountAsync();

            var totalBookings = await _context.Bookings
                .Where(b => b.SeekerId == id && b.Status == EcoLift.Models.Enums.BookingStatus.Confirmed)
                .CountAsync();

            var viewModel = new ProfileViewModel
            {
                User = targetUser,
                Vehicles = vehicles,
                IsOwnProfile = false
            };

            ViewBag.PublishedRidesCount = publishedRides;
            ViewBag.CompletedRidesCount = completedRides;
            ViewBag.TotalBookingsCount = totalBookings;

            return View("ViewUser", viewModel);
        }

        private async Task<bool> HasRideConnection(string currentUserId, string targetUserId)
        {
            // Check if current user has booked rides from target user (target is provider)
            var hasBookedFromTarget = await _context.Bookings
                .AnyAsync(b => b.SeekerId == currentUserId &&
                              b.Trip.ProviderId == targetUserId);

            // Check if target user has booked rides from current user (current is provider)
            var targetHasBookedFromCurrent = await _context.Bookings
                .AnyAsync(b => b.SeekerId == targetUserId &&
                              b.Trip.ProviderId == currentUserId);

            return hasBookedFromTarget || targetHasBookedFromCurrent;
        }
    }
}
