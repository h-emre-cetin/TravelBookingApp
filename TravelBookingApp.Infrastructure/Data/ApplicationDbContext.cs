using Microsoft.EntityFrameworkCore;
using TravelBookingApp.Domain.Models;

namespace TravelBookingApp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<RentalCar> RentalCars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Flight entity
            modelBuilder.Entity<Flight>()
                .Property(f => f.Price)
                .HasColumnType("decimal(18,2)");

            // Configure Hotel entity
            modelBuilder.Entity<Hotel>()
                .Property(h => h.PricePerNight)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Hotel>()
                .Property(h => h.Amenities)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            // Configure RentalCar entity
            modelBuilder.Entity<RentalCar>()
                .Property(c => c.PricePerDay)
                .HasColumnType("decimal(18,2)");

            // Configure Booking entity
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");
        }
    }
}
