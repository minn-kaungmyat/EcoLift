using System.Diagnostics;
using EcoLift.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EcoLift.Data;
using EcoLift.Models.Enums;
using System.Text.Json;

namespace EcoLift.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchRideViewModel searchModel)
        {
            // Check if this is a meaningful search (has at least one location)
            bool hasMeaningfulSearch = !string.IsNullOrWhiteSpace(searchModel.FromLocation) || !string.IsNullOrWhiteSpace(searchModel.ToLocation);
            
            // If no meaningful search, get search history
            List<SearchHistory> searchHistory = new List<SearchHistory>();
            if (!hasMeaningfulSearch)
            {
                try
                {
                    // Get search history from session (up to 5 most recent)
                    var historyJson = HttpContext.Session.GetString("SearchHistory");
                    _logger.LogInformation($"Session SearchHistory JSON: {historyJson}");
                    
                    if (!string.IsNullOrEmpty(historyJson))
                    {
                        try
                        {
                            searchHistory = System.Text.Json.JsonSerializer.Deserialize<List<SearchHistory>>(historyJson) ?? new List<SearchHistory>();
                            searchHistory = searchHistory.Take(5).ToList(); // Keep only 5 most recent
                            _logger.LogInformation($"Deserialized {searchHistory.Count} search history items");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error deserializing search history: {ex.Message}");
                            searchHistory = new List<SearchHistory>();
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No search history found in session");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error accessing session: {ex.Message}");
                    searchHistory = new List<SearchHistory>();
                }
                
                // Ensure we always set a valid list and it's properly typed
                if (searchHistory == null)
                {
                    searchHistory = new List<SearchHistory>();
                }
                
                ViewBag.SearchHistory = searchHistory;
                ViewBag.SearchResults = new List<Trip>();
                ViewBag.ResultCount = 0;
                
                _logger.LogInformation($"Returning view with {searchHistory.Count} search history items");
                return View(searchModel);
            }

            var query = _context.Trips
                .Where(t => t.Status == TripStatus.Published || t.Status == TripStatus.PartiallyBooked)
                .Include(t => t.Provider)
                .Include(t => t.Vehicle)
                .AsQueryable();

            // Apply basic filters (excluding location-based ones)
            if (searchModel.DepartureDate.HasValue)
            {
                query = query.Where(t => t.DepartureDate.Date == searchModel.DepartureDate.Value.Date);
            }

            if (searchModel.Passengers.HasValue)
            {
                query = query.Where(t => t.AvailableSeats >= searchModel.Passengers.Value);
            }

            if (searchModel.MaxPrice.HasValue)
            {
                query = query.Where(t => t.PricePerSeat <= searchModel.MaxPrice.Value);
            }

            if (searchModel.AllowSmoking)
            {
                query = query.Where(t => t.AllowSmoking);
            }

            if (searchModel.AllowPets)
            {
                query = query.Where(t => t.AllowPets);
            }

            // Get all trips that match the basic filters
            var filteredTrips = await query.ToListAsync();

            // Apply geographic radius filtering
            var results = new List<Trip>();
            
            // Require at least one location to be specified for a meaningful search
            if (string.IsNullOrWhiteSpace(searchModel.FromLocation) && string.IsNullOrWhiteSpace(searchModel.ToLocation))
            {
                // No locations specified, return empty results
                results = new List<Trip>();
            }
            else if ((!string.IsNullOrWhiteSpace(searchModel.FromLocation) && searchModel.FromLatitude.HasValue && searchModel.FromLongitude.HasValue) || 
                     (!string.IsNullOrWhiteSpace(searchModel.ToLocation) && searchModel.ToLatitude.HasValue && searchModel.ToLongitude.HasValue))
            {
                foreach (var trip in filteredTrips)
                {
                    bool fromLocationMatch = true;
                    bool toLocationMatch = true;

                    // Check pickup location radius using coordinates
                    if (!string.IsNullOrWhiteSpace(searchModel.FromLocation) && searchModel.FromLatitude.HasValue && searchModel.FromLongitude.HasValue)
                    {
                        fromLocationMatch = IsWithinRadius(
                            searchModel.FromLatitude.Value,
                            searchModel.FromLongitude.Value,
                            trip.PickupLatitude,
                            trip.PickupLongitude,
                            searchModel.SearchRadius
                        );
                    }

                    // Check dropoff location radius using coordinates
                    if (!string.IsNullOrWhiteSpace(searchModel.ToLocation) && searchModel.ToLatitude.HasValue && searchModel.ToLongitude.HasValue)
                    {
                        toLocationMatch = IsWithinRadius(
                            searchModel.ToLatitude.Value,
                            searchModel.ToLongitude.Value,
                            trip.DropoffLatitude,
                            trip.DropoffLongitude,
                            searchModel.SearchRadius
                        );
                    }

                    // Only include trips that match both locations (if specified)
                    if (fromLocationMatch && toLocationMatch)
                    {
                        results.Add(trip);
                    }
                }
            }
            else
            {
                // Location specified but missing coordinates, return empty results
                results = new List<Trip>();
            }

            // Order by departure date and price
            results = results
                .OrderBy(t => t.DepartureDate)
                .ThenBy(t => t.PricePerSeat)
                .ToList();

            // Save search to history
            SaveSearchToHistory(searchModel);

            // Debug logging
            _logger.LogInformation($"Search completed: {results.Count} results found");
            if (searchModel.Passengers.HasValue)
            {
                _logger.LogInformation($"Passenger filter: {searchModel.Passengers.Value} passengers requested");
            }
            if (searchModel.FromLocation != null)
            {
                _logger.LogInformation($"From location: {searchModel.FromLocation} (Lat: {searchModel.FromLatitude}, Lng: {searchModel.FromLongitude})");
            }
            if (searchModel.ToLocation != null)
            {
                _logger.LogInformation($"To location: {searchModel.ToLocation} (Lat: {searchModel.ToLatitude}, Lng: {searchModel.ToLongitude})");
            }

            ViewBag.SearchModel = searchModel;
            ViewBag.SearchResults = results;
            ViewBag.ResultCount = results.Count;

            return View(searchModel);
        }

        /// <summary>
        /// Checks if a trip location is within the specified radius of the search location
        /// </summary>
        private bool IsWithinRadius(double searchLat, double searchLng, decimal? tripLat, decimal? tripLng, int searchRadiusKm)
        {
            if (!tripLat.HasValue || !tripLng.HasValue)
                return false;

            try
            {
                // Calculate distance between search location and trip location using coordinates directly
                var distance = CalculateDistance(
                    searchLat,
                    searchLng,
                    (double)tripLat.Value,
                    (double)tripLng.Value
                );

                // Check if within radius (convert km to meters for comparison)
                return distance <= (searchRadiusKm * 1000);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets coordinates for an address using Google Geocoding API
        /// </summary>
        private (double Latitude, double Longitude)? GetCoordinatesFromAddress(string address)
        {
            try
            {
                // In a production app, you would make an HTTP request to Google Geocoding API
                // For now, we'll use a simple mapping for common locations
                var commonLocations = new Dictionary<string, (double, double)>
                {
                    { "bangkok", (13.7563, 100.5018) },
                    { "chiang mai", (18.7883, 98.9853) },
                    { "phuket", (7.8804, 98.3923) },
                    { "pattaya", (12.9236, 100.8824) },
                    { "krabi", (8.0863, 98.9063) },
                    { "koh samui", (9.5120, 100.0136) },
                    { "ayutthaya", (14.3691, 100.5876) },
                    { "sukhothai", (17.0061, 99.8233) },
                    { "kanchanaburi", (14.0021, 99.5328) },
                    { "rayong", (12.6814, 101.2813) }
                };

                var searchTerm = address.ToLower().Trim();
                
                // Check for exact matches first
                if (commonLocations.ContainsKey(searchTerm))
                {
                    var coords = commonLocations[searchTerm];
                    return (coords.Item1, coords.Item2);
                }

                // Check for partial matches
                foreach (var location in commonLocations)
                {
                    if (searchTerm.Contains(location.Key) || location.Key.Contains(searchTerm))
                    {
                        return (location.Value.Item1, location.Value.Item2);
                    }
                }

                // If no match found, return null (will be filtered out)
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Calculates the distance between two points using the Haversine formula
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371000; // Earth's radius in meters

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c; // Distance in meters
        }

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Test action to verify session is working
        /// </summary>
        public IActionResult TestSession()
        {
            try
            {
                var testValue = HttpContext.Session.GetString("TestKey");
                if (string.IsNullOrEmpty(testValue))
                {
                    HttpContext.Session.SetString("TestKey", "Session is working! " + DateTime.Now.ToString());
                    testValue = "Set new value";
                }
                
                ViewBag.SessionTest = testValue;
                ViewBag.SessionId = HttpContext.Session.Id;
                ViewBag.SessionAvailable = HttpContext.Session.IsAvailable;
                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.SessionError = ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Saves the search parameters to session history
        /// </summary>
        private void SaveSearchToHistory(SearchRideViewModel searchModel)
        {
            try
            {
                // Get existing history from session
                var historyJson = HttpContext.Session.GetString("SearchHistory");
                var searchHistory = new List<SearchHistory>();
                
                if (!string.IsNullOrEmpty(historyJson))
                {
                    try
                    {
                        searchHistory = System.Text.Json.JsonSerializer.Deserialize<List<SearchHistory>>(historyJson) ?? new List<SearchHistory>();
                        _logger.LogInformation($"Loaded {searchHistory.Count} existing search history items");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error deserializing existing search history: {ex.Message}");
                        searchHistory = new List<SearchHistory>();
                    }
                }

                // Create new search history entry
                var newSearch = new SearchHistory
                {
                    FromLocation = searchModel.FromLocation,
                    FromLatitude = searchModel.FromLatitude,
                    FromLongitude = searchModel.FromLongitude,
                    ToLocation = searchModel.ToLocation,
                    ToLatitude = searchModel.ToLatitude,
                    ToLongitude = searchModel.ToLongitude,
                    DepartureDate = searchModel.DepartureDate,
                    Passengers = searchModel.Passengers,
                    MaxPrice = searchModel.MaxPrice,
                    SearchRadius = searchModel.SearchRadius,
                    AllowSmoking = searchModel.AllowSmoking,
                    AllowPets = searchModel.AllowPets,
                    OnlyDirectRoutes = searchModel.OnlyDirectRoutes,
                    SearchedAt = DateTime.UtcNow,
                    UserId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : null
                };

                _logger.LogInformation($"Created new search history entry: From={newSearch.FromLocation}, To={newSearch.ToLocation}, Date={newSearch.DepartureDate}");

                // Remove duplicate searches (same parameters)
                searchHistory = searchHistory.Where(h => 
                    h.FromLocation != newSearch.FromLocation ||
                    h.ToLocation != newSearch.ToLocation ||
                    h.DepartureDate != newSearch.DepartureDate ||
                    h.Passengers != newSearch.Passengers
                ).ToList();

                // Add new search at the beginning
                searchHistory.Insert(0, newSearch);

                // Keep only 5 most recent searches
                searchHistory = searchHistory.Take(5).ToList();

                // Save back to session
                var updatedHistoryJson = System.Text.Json.JsonSerializer.Serialize(searchHistory);
                HttpContext.Session.SetString("SearchHistory", updatedHistoryJson);
                _logger.LogInformation($"Saved {searchHistory.Count} search history items to session");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving search history: {ex.Message}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Test actions for TempData notifications
        public IActionResult TestSuccess()
        {
            TempData["SuccessMessage"] = "This is a test success message!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult TestError()
        {
            TempData["ErrorMessage"] = "This is a test error message!";
            return RedirectToAction(nameof(Index));
        }
    }
}
