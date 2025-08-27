using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcoLift.Models;
using EcoLift.Data;
using EcoLift.Models.Enums;

namespace EcoLift.Controllers
{
    public class BookingController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<BookingController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Booking/Create/{tripId}
        public async Task<IActionResult> Create(int tripId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var trip = await _context.Trips
                .Include(t => t.Provider)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
            {
                TempData["ErrorMessage"] = "Trip not found.";
                return RedirectToAction("Search", "Home");
            }

            if (trip.ProviderId == user.Id)
            {
                TempData["ErrorMessage"] = "You cannot book your own trip.";
                return RedirectToAction("Search", "Home");
            }

            if (!trip.IsOpenForBookings)
            {
                TempData["ErrorMessage"] = "This trip is not available for booking.";
                return RedirectToAction("Search", "Home");
            }

            if (trip.RemainingSeats <= 0)
            {
                TempData["ErrorMessage"] = "No seats available on this trip.";
                return RedirectToAction("Search", "Home");
            }

            var viewModel = new CreateBookingViewModel
            {
                TripId = trip.TripId,
                Trip = trip,
                SeatsBooked = 1,
                MaxSeatsAvailable = trip.RemainingSeats
            };

            return View(viewModel);
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var trip = await _context.Trips
                        .Include(t => t.Provider)
                        .FirstOrDefaultAsync(t => t.TripId == model.TripId);

                    if (trip == null)
                    {
                        TempData["ErrorMessage"] = "Trip not found.";
                        return RedirectToAction("Search", "Home");
                    }

                    // Prevent users from booking their own trip
                    if (trip.ProviderId == user.Id)
                    {
                        TempData["ErrorMessage"] = "You cannot book your own trip.";
                        return RedirectToAction("Search", "Home");
                    }

                    // Check if user already has a booking for this trip
                    var existingBooking = await _context.Bookings
                        .FirstOrDefaultAsync(b => b.TripId == model.TripId && b.SeekerId == user.Id);

                    if (existingBooking != null)
                    {
                        TempData["ErrorMessage"] = "You already have a booking for this trip.";
                        return RedirectToAction("Index", "Ride");
                    }

                    // Validate seats availability - calculate remaining seats manually
                    var currentBookedSeats = await _context.Bookings
                        .Where(b => b.TripId == trip.TripId && b.Status == BookingStatus.Confirmed)
                        .SumAsync(b => b.SeatsBooked);
                    
                    var currentRemainingSeats = trip.AvailableSeats - currentBookedSeats;
                    
                    if (model.SeatsBooked > currentRemainingSeats)
                    {
                        ModelState.AddModelError("SeatsBooked", $"Only {currentRemainingSeats} seats available.");
                        model.Trip = trip;
                        model.MaxSeatsAvailable = currentRemainingSeats;
                        return View(model);
                    }

                    // Create the booking with auto-confirm
                    var booking = new Booking
                    {
                        TripId = model.TripId,
                        SeekerId = user.Id,
                        SeatsBooked = model.SeatsBooked,
                        Status = BookingStatus.Confirmed, // Auto-confirm
                        BookingDate = DateTime.UtcNow
                    };

                    _context.Bookings.Add(booking);

                    // Create a conversation for this booking
                    var conversation = new Conversation
                    {
                        TripId = model.TripId,
                        DriverId = trip.ProviderId,
                        PassengerId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Conversations.Add(conversation);
                    await _context.SaveChangesAsync();

                    // Link the booking to the conversation
                    booking.ConversationId = conversation.ConversationId;
                    await _context.SaveChangesAsync();

                    // Update trip status if needed - calculate remaining seats manually
                    var totalBookedSeats = await _context.Bookings
                        .Where(b => b.TripId == trip.TripId && b.Status == BookingStatus.Confirmed)
                        .SumAsync(b => b.SeatsBooked);
                    
                    var remainingSeats = trip.AvailableSeats - totalBookedSeats;
                    
                    if (remainingSeats <= 0)
                    {
                        trip.Status = TripStatus.Full;
                    }
                    else if (trip.Status == TripStatus.Published)
                    {
                        trip.Status = TripStatus.PartiallyBooked;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Booking created: Trip {trip.TripId}, User {user.Id}, Seats {model.SeatsBooked}");

                    TempData["SuccessMessage"] = $"Booking confirmed! You've reserved {model.SeatsBooked} seat(s) for €{trip.PricePerSeat * model.SeatsBooked:F2}.";
                    return RedirectToAction("Index", "Ride");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creating booking: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while creating your booking. Please try again.";
                    return RedirectToAction("Index", "Ride");
                }
            }

            // If we got this far, something failed, redisplay form
            var tripForModel = await _context.Trips
                .Include(t => t.Provider)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.TripId == model.TripId);

            if (tripForModel != null)
            {
                model.Trip = tripForModel;
                model.MaxSeatsAvailable = tripForModel.RemainingSeats;
            }

            return View(model);
        }

        // GET: Booking/MyBookings - Redirect to Ride/Index (consolidated view)
        public IActionResult MyBookings()
        {
            return RedirectToAction("Index", "Ride");
        }

        // POST: Booking/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.SeekerId == user.Id);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction("Index", "Ride");
            }

            if (booking.Status != BookingStatus.Confirmed)
            {
                TempData["ErrorMessage"] = "Only confirmed bookings can be cancelled.";
                return RedirectToAction("Index", "Ride");
            }

            // Check if trip is within 24 hours
            if (booking.Trip.DepartureDateTime <= DateTime.Now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Bookings cannot be cancelled within 24 hours of departure.";
                return RedirectToAction("Index", "Ride");
            }

            try
            {
                // Update trip status if needed
                var trip = booking.Trip;
                if (trip.Status == TripStatus.Full)
                {
                    trip.Status = TripStatus.PartiallyBooked;
                }
                else if (trip.Status == TripStatus.PartiallyBooked && trip.RemainingSeats + booking.SeatsBooked == trip.AvailableSeats)
                {
                    trip.Status = TripStatus.Published;
                }

                // Remove the booking
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Booking cancelled successfully.";
                _logger.LogInformation($"Booking cancelled: {id} by user {user.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling booking: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while cancelling your booking.";
            }

            return RedirectToAction("Index", "Ride");
        }
    }
}
