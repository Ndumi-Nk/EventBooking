using EventBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<CateringMenu> CateringMenus { get; set; }
        public DbSet<BookingCatering> BookingCaterings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Payments> Payment{ get; set; }
        public DbSet<AdditionalService> AdditionalServices { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageCatering> PackageCaterings { get; set; }
        public DbSet<PackageService> PackageServices { get; set; }

        public DbSet<PackageBooking> PackageBookings{ get; set; }
        public DbSet<CustomDecoration> CustomDecorations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BookingCatering>()
                .HasOne(bc => bc.Booking)
                .WithMany(b => b.BookingCaterings)
                .HasForeignKey(bc => bc.BookingId);

            builder.Entity<BookingService>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId);
            builder.Entity<CustomDecoration>()
           .HasOne(c => c.User)
           .WithMany(u => u.CustomDecorations) // You may rename to CustomDecorations in ApplicationUser
           .HasForeignKey(c => c.UserId)
           .OnDelete(DeleteBehavior.Cascade);

        }
    }
}