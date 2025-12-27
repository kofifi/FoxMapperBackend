using FoxMapperBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FoxMapperBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Courier> Couriers => Set<Courier>();
    public DbSet<Depot> Depots => Set<Depot>();
    public DbSet<DeliveryRun> DeliveryRuns => Set<DeliveryRun>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<RouteStop> RouteStops => Set<RouteStop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === Courier ===
        modelBuilder.Entity<Courier>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                  .HasMaxLength(64)
                  .IsRequired();

            entity.Property(e => e.FirstName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.LastName)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.Email)
                  .HasMaxLength(200);

            entity.Property(e => e.PhoneNumber)
                  .HasMaxLength(50);

            entity.HasIndex(e => e.Code)
                  .IsUnique();

            entity.HasIndex(e => e.LastUpdatedUtc);
        });

        // === Depot ===
        modelBuilder.Entity<Depot>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(e => e.AddressLine)
                  .HasMaxLength(200);

            entity.Property(e => e.City)
                  .HasMaxLength(100);

            entity.Property(e => e.PostalCode)
                  .HasMaxLength(20);

            entity.HasIndex(e => e.LastUpdatedUtc);
        });

        // === DeliveryRun ===
        modelBuilder.Entity<DeliveryRun>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Courier)
                  .WithMany(c => c.DeliveryRuns)
                  .HasForeignKey(e => e.CourierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Depot)
                  .WithMany(d => d.DeliveryRuns)
                  .HasForeignKey(e => e.DepotId)
                  .OnDelete(DeleteBehavior.Restrict);

            // enum jako int
            entity.Property(e => e.Status)
                  .HasConversion<int>();

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastUpdatedUtc);
        });

        // === Package ===
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.DeliveryRun)
                  .WithMany(r => r.Packages)
                  .HasForeignKey(e => e.DeliveryRunId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.ExternalCode)
                  .HasMaxLength(128);

            entity.Property(e => e.RecipientName)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(e => e.AddressLine)
                  .HasMaxLength(200);

            entity.Property(e => e.City)
                  .HasMaxLength(100);

            entity.Property(e => e.PostalCode)
                  .HasMaxLength(20);

            // enum jako int
            entity.Property(e => e.Status)
                  .HasConversion<int>();

            entity.HasIndex(e => e.ExternalCode);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastUpdatedUtc);
        });

        // === RouteStop ===
        modelBuilder.Entity<RouteStop>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.DeliveryRun)
                  .WithMany(r => r.RouteStops)
                  .HasForeignKey(e => e.DeliveryRunId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Package)
                  .WithMany(p => p.RouteStops)
                  .HasForeignKey(e => e.PackageId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Type)
                  .HasConversion<int>();

            // Unikalne: w jednym runie dany OrderIndex tylko raz
            entity.HasIndex(e => new { e.DeliveryRunId, e.OrderIndex })
                  .IsUnique();

            entity.HasIndex(e => e.LastUpdatedUtc);
        });
    }
}
