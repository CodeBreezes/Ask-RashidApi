using BookingAppAPI.DB.Models.Address;
using BookingAppAPI.DB.Models.User;
using Microsoft.EntityFrameworkCore;
using BookingAppAPI.DB;

namespace BookingAppAPI.DB
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.EngagementRoles();
            modelBuilder.SeedServices();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) { }
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Roles> Roles { get; set; }

        public DbSet<Country> Country { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        public DbSet<Services> Services { get; set; }
        public DbSet<BookingAppAPI.DB.Booking> Booking { get; set; } = default!;

    }
}
