using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Api.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<UserRequest> UserRequests { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<CallStream> CallStreams { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // PostgreSQL (timestamptz) akzeptiert nur UTC-Zeitpunkte.
            // Der Converter sorgt dafür, dass jede DateTime als UTC gespeichert
            // und beim Lesen wieder als UTC markiert wird.
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Services");
                entity.Property(e => e.PricePerMinute).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasOne(i => i.Service)
                    .WithMany()
                    .HasForeignKey(i => i.ServiceId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(i => i.Customer).WithMany().HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Employee).WithMany().HasForeignKey(i => i.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => i.EmployeeId).HasDatabaseName("IX_Invoices_EmployeeId");
                entity.Property(e => e.NetAmount).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(o => o.Employee).WithMany().HasForeignKey(o => o.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.TimeOfService).HasPrecision(18, 2);
                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            });

            modelBuilder.Entity<UserRequest>(entity =>
            {
                entity.HasOne(r => r.Employee).WithMany().HasForeignKey(r => r.EmployeeId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CallStream>(entity =>
            {
                entity.HasIndex(c => c.UserId).IsUnique();
            });
        }

        private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
        {
            public UtcDateTimeConverter() : base(
                v => v.Kind == DateTimeKind.Utc ? v
                   : v.Kind == DateTimeKind.Local ? v.ToUniversalTime()
                   : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            { }
        }
    }
}
