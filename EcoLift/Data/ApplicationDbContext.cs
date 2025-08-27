using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcoLift.Models;
using EcoLift.Models.Enums;

namespace EcoLift.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser relationships
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasMany(u => u.TripsAsProvider)
                      .WithOne(t => t.Provider)
                      .HasForeignKey(t => t.ProviderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Bookings)
                      .WithOne(b => b.Seeker)
                      .HasForeignKey(b => b.SeekerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Vehicles)
                      .WithOne(v => v.Owner)
                      .HasForeignKey(v => v.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.ReviewsGiven)
                      .WithOne(r => r.Reviewer)
                      .HasForeignKey(r => r.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ReviewsReceived)
                      .WithOne(r => r.ReviewedUser)
                      .HasForeignKey(r => r.ReviewedUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Messages)
                      .WithOne(m => m.Sender)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Conversation relationships
                entity.HasMany(u => u.ConversationsAsDriver)
                      .WithOne(c => c.Driver)
                      .HasForeignKey(c => c.DriverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.ConversationsAsPassenger)
                      .WithOne(c => c.Passenger)
                      .HasForeignKey(c => c.PassengerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Vehicle relationships
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasOne(v => v.Owner)
                      .WithMany(u => u.Vehicles)
                      .HasForeignKey(v => v.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Trips)
                      .WithOne(t => t.Vehicle)
                      .HasForeignKey(t => t.VehicleId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(v => v.LicensePlate)
                      .IsUnique();
            });

            // Configure Trip relationships
            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasOne(t => t.Provider)
                      .WithMany(u => u.TripsAsProvider)
                      .HasForeignKey(t => t.ProviderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Vehicle)
                      .WithMany(v => v.Trips)
                      .HasForeignKey(t => t.VehicleId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(t => t.Bookings)
                      .WithOne(b => b.Trip)
                      .HasForeignKey(b => b.TripId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(t => t.Reviews)
                      .WithOne(r => r.Trip)
                      .HasForeignKey(r => r.TripId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Create indexes for location-based queries
                entity.HasIndex(t => new { t.PickupLocation, t.DepartureDate, t.DepartureTime })
                      .HasDatabaseName("IX_Trip_PickupLocation_DepartureDateTime");

                entity.HasIndex(t => new { t.DropoffLocation, t.DepartureDate, t.DepartureTime })
                      .HasDatabaseName("IX_Trip_DropoffLocation_DepartureDateTime");

                entity.HasIndex(t => new { t.DepartureTime, t.IsActive })
                      .HasDatabaseName("IX_Trip_DepartureTime_IsActive");
            });

            // Configure Booking relationships
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasOne(b => b.Trip)
                      .WithMany(t => t.Bookings)
                      .HasForeignKey(b => b.TripId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Seeker)
                      .WithMany(u => u.Bookings)
                      .HasForeignKey(b => b.SeekerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(b => b.Conversation)
                      .WithMany()
                      .HasForeignKey(b => b.ConversationId)
                      .OnDelete(DeleteBehavior.NoAction);

                // Ensure a user can only book a trip once (composite unique index)
                entity.HasIndex(b => new { b.TripId, b.SeekerId })
                      .IsUnique()
                      .HasDatabaseName("IX_Booking_TripId_SeekerId_Unique");

                // Convert enum to string in database
                entity.Property(b => b.Status)
                      .HasConversion<string>();
            });

            // Configure Review relationships
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Trip)
                      .WithMany(t => t.Reviews)
                      .HasForeignKey(r => r.TripId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Reviewer)
                      .WithMany(u => u.ReviewsGiven)
                      .HasForeignKey(r => r.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReviewedUser)
                      .WithMany(u => u.ReviewsReceived)
                      .HasForeignKey(r => r.ReviewedUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ensure unique reviews per trip per reviewer-reviewee pair
                entity.HasIndex(r => new { r.TripId, r.ReviewerId, r.ReviewedUserId })
                      .IsUnique()
                      .HasDatabaseName("IX_Review_TripId_ReviewerId_ReviewedUserId_Unique");
            });

            // Configure Conversation relationships
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasOne(c => c.Trip)
                      .WithMany()
                      .HasForeignKey(c => c.TripId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Driver)
                      .WithMany(u => u.ConversationsAsDriver)
                      .HasForeignKey(c => c.DriverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Passenger)
                      .WithMany(u => u.ConversationsAsPassenger)
                      .HasForeignKey(c => c.PassengerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ensure unique conversation per trip per driver-passenger pair
                entity.HasIndex(c => new { c.TripId, c.DriverId, c.PassengerId })
                      .IsUnique()
                      .HasDatabaseName("IX_Conversation_TripId_DriverId_PassengerId_Unique");

                entity.HasIndex(c => c.LastMessageAt)
                      .HasDatabaseName("IX_Conversation_LastMessageAt");
            });

            // Configure Message relationships
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasOne(m => m.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(m => m.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired();

                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.Messages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => m.SentAt)
                      .HasDatabaseName("IX_Message_SentAt");

                entity.HasIndex(m => new { m.ConversationId, m.SentAt })
                      .HasDatabaseName("IX_Message_ConversationId_SentAt");
            });
        }
    }
}
