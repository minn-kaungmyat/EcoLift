using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcoLift.Models;
using EcoLift.Data;
using EcoLift.Models.Enums;

namespace EcoLift.Controllers
{
    public class RideController : Controller
    {
        private readonly ILogger<RideController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RideController(
            ILogger<RideController> logger,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Get user's published rides (as provider)
            var publishedRides = await _context.Trips
                .Where(t => t.ProviderId == user.Id)
                .Include(t => t.Vehicle)
                .OrderByDescending(t => t.DepartureDate)
                .ToListAsync();

            // Get user's booked rides (as seeker)
            var bookedRides = await _context.Bookings
                .Where(b => b.SeekerId == user.Id)
                .Include(b => b.Trip)
                .ThenInclude(t => t.Provider)
                .Include(b => b.Trip)
                .ThenInclude(t => t.Vehicle)
                .OrderByDescending(b => b.Trip.DepartureDate)
                .ToListAsync();

            ViewBag.PublishedRides = publishedRides;
            ViewBag.BookedRides = bookedRides;
            return View();
        }

        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("RideController.Create() action called");
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userVehicles = await _context.Vehicles
                .Where(v => v.OwnerId == user.Id)
                .ToListAsync();

            if (!userVehicles.Any())
            {
                TempData["ErrorMessage"] = "You need to add a vehicle before creating a ride. Please add a vehicle in your profile first.";
                return RedirectToAction("Index", "Profile");
            }

            ViewBag.UserVehicles = userVehicles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRideViewModel model)
        {
            _logger.LogInformation("RideController.Create(POST) action called");
            _logger.LogInformation($"Pickup: {model.PickupLocation} ({model.PickupLatitude}, {model.PickupLongitude})");
            _logger.LogInformation($"Dropoff: {model.DropoffLocation} ({model.DropoffLatitude}, {model.DropoffLongitude})");
            _logger.LogInformation($"Vehicle ID: {model.VehicleId}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToPage("/Account/Login", new { area = "Identity" });
                    }

                    // Verify the vehicle belongs to the user
                    var vehicle = await _context.Vehicles
                        .FirstOrDefaultAsync(v => v.VehicleId == model.VehicleId && v.OwnerId == user.Id);
                    
                    if (vehicle == null)
                    {
                        ModelState.AddModelError("VehicleId", "Invalid vehicle selected");
                        var userVehicles = await _context.Vehicles.Where(v => v.OwnerId == user.Id).ToListAsync();
                        ViewBag.UserVehicles = userVehicles;
                        return View(model);
                    }

                    // Create and save the trip to the database
                    var trip = new Trip
                    {
                        ProviderId = user.Id,
                        PickupLocation = model.PickupLocation,
                        PickupLatitude = model.PickupLatitude,
                        PickupLongitude = model.PickupLongitude,
                        DropoffLocation = model.DropoffLocation,
                        DropoffLatitude = model.DropoffLatitude,
                        DropoffLongitude = model.DropoffLongitude,
                        DepartureDate = model.DepartureDate,
                        DepartureTime = model.DepartureTime,
                        AvailableSeats = model.AvailableSeats,
                        PricePerSeat = model.PricePerSeat,
                        VehicleId = model.VehicleId,
                        Notes = model.Notes,
                        AllowSmoking = model.AllowSmoking,
                        AllowPets = model.AllowPets,
                        IsActive = true,
                        Status = TripStatus.Published
                    };

                    _context.Trips.Add(trip);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ride created successfully! Your journey is now available for others to book.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while creating the ride: " + ex.Message;
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var userVehicles = await _context.Vehicles.Where(v => v.OwnerId == user.Id).ToListAsync();
                        ViewBag.UserVehicles = userVehicles;
                    }
                    return View(model);
                }
            }
            
            // If we got this far, something failed, redisplay form
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var userVehicles = await _context.Vehicles.Where(v => v.OwnerId == currentUser.Id).ToListAsync();
                ViewBag.UserVehicles = userVehicles;
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation($"RideController.Edit() action called for trip ID: {id}");
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Get the trip and verify it belongs to the user
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.TripId == id && t.ProviderId == user.Id);

            if (trip == null)
            {
                TempData["ErrorMessage"] = "Ride not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            var userVehicles = await _context.Vehicles
                .Where(v => v.OwnerId == user.Id)
                .ToListAsync();

            if (!userVehicles.Any())
            {
                TempData["ErrorMessage"] = "You need to add a vehicle before editing a ride. Please add a vehicle in your profile first.";
                return RedirectToAction("Index", "Profile");
            }

            // Create the edit view model
            var editModel = new EditRideViewModel
            {
                TripId = trip.TripId,
                PickupLocation = trip.PickupLocation,
                PickupLatitude = trip.PickupLatitude,
                PickupLongitude = trip.PickupLongitude,
                DropoffLocation = trip.DropoffLocation,
                DropoffLatitude = trip.DropoffLatitude,
                DropoffLongitude = trip.DropoffLongitude,
                DepartureDate = trip.DepartureDate,
                DepartureTime = trip.DepartureTime,
                AvailableSeats = trip.AvailableSeats,
                PricePerSeat = trip.PricePerSeat,
                VehicleId = trip.VehicleId ?? 0,
                Notes = trip.Notes,
                AllowSmoking = trip.AllowSmoking,
                AllowPets = trip.AllowPets
            };

            ViewBag.UserVehicles = userVehicles;
            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRideViewModel model)
        {
            _logger.LogInformation($"RideController.Edit(POST) action called for trip ID: {model.TripId}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToPage("/Account/Login", new { area = "Identity" });
                    }

                    // Get the existing trip and verify it belongs to the user
                    var existingTrip = await _context.Trips
                        .FirstOrDefaultAsync(t => t.TripId == model.TripId && t.ProviderId == user.Id);

                    if (existingTrip == null)
                    {
                        TempData["ErrorMessage"] = "Ride not found or you don't have permission to edit it.";
                        return RedirectToAction("Index");
                    }

                    // Verify the vehicle belongs to the user
                    var vehicle = await _context.Vehicles
                        .FirstOrDefaultAsync(v => v.VehicleId == model.VehicleId && v.OwnerId == user.Id);
                    
                    if (vehicle == null)
                    {
                        ModelState.AddModelError("VehicleId", "Invalid vehicle selected");
                        var userVehicles = await _context.Vehicles.Where(v => v.OwnerId == user.Id).ToListAsync();
                        ViewBag.UserVehicles = userVehicles;
                        return View(model);
                    }

                    // Update the trip
                    existingTrip.PickupLocation = model.PickupLocation;
                    existingTrip.PickupLatitude = model.PickupLatitude;
                    existingTrip.PickupLongitude = model.PickupLongitude;
                    existingTrip.DropoffLocation = model.DropoffLocation;
                    existingTrip.DropoffLatitude = model.DropoffLatitude;
                    existingTrip.DropoffLongitude = model.DropoffLongitude;
                    existingTrip.DepartureDate = model.DepartureDate;
                    existingTrip.DepartureTime = model.DepartureTime;
                    existingTrip.AvailableSeats = model.AvailableSeats;
                    existingTrip.PricePerSeat = model.PricePerSeat;
                    existingTrip.VehicleId = model.VehicleId;
                    existingTrip.Notes = model.Notes;
                    existingTrip.AllowSmoking = model.AllowSmoking;
                    existingTrip.AllowPets = model.AllowPets;

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ride updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the ride: " + ex.Message;
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var userVehicles = await _context.Vehicles.Where(v => v.OwnerId == user.Id).ToListAsync();
                        ViewBag.UserVehicles = userVehicles;
                    }
                    return View(model);
                }
            }
            
            // If we got this far, something failed, redisplay form
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var userVehicles = await _context.Vehicles.Where(v => v.OwnerId == currentUser.Id).ToListAsync();
                ViewBag.UserVehicles = userVehicles;
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // First, check if the user is the provider of this ride (created ride)
            var trip = await _context.Trips
                .Include(t => t.Provider)
                .Include(t => t.Vehicle)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.Seeker)
                .Include(t => t.Reviews)
                .FirstOrDefaultAsync(t => t.TripId == id && t.ProviderId == user.Id);

            if (trip != null)
            {
                // User is the provider - show full details with all bookings
                ViewBag.IsProvider = true;
                ViewBag.UserBookings = trip.Bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();
                return View(trip);
            }

            // If not the provider, check if user has booked this ride
            var booking = await _context.Bookings
                .Where(b => b.TripId == id && b.SeekerId == user.Id)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Provider)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Vehicle)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Bookings)
                        .ThenInclude(bk => bk.Seeker)
                .Include(b => b.Trip)
                    .ThenInclude(t => t.Reviews)
                .FirstOrDefaultAsync();

            if (booking != null)
            {
                // User has booked this ride - show details with their booking info
                ViewBag.IsProvider = false;
                ViewBag.UserBooking = booking;
                return View(booking.Trip);
            }

            // User has no access to this ride
            TempData["ErrorMessage"] = "Ride not found or you don't have permission to view it.";
            return RedirectToAction("Index");
        }
    }
}
