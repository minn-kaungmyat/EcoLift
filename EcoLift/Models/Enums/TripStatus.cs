namespace EcoLift.Models.Enums
{
    public enum TripStatus
    {
        Published = 1,        // The trip is visible to others and open for bookings
        PartiallyBooked = 2,  // Some seats are reserved, but not all
        Full = 3,             // All seats are booked, so new bookings are not allowed
        Ongoing = 4,          // The trip has started (driver is on the way)
        Completed = 5,        // The trip finished successfully
        Cancelled = 6         // The trip was cancelled by the driver (or system if driver is inactive)
    }
}
