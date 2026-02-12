using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Meal> Meals { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.User)
                .WithMany()
                .HasForeignKey(wt => wt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create unique index for booking per user per day
            builder.Entity<Booking>()
                .HasIndex(b => new { b.UserId, b.DataMarcacao })
                .IsUnique();

            // Create index for meals by date
            builder.Entity<Meal>()
                .HasIndex(m => m.Data)
                .IsUnique();
        }
    }
}
