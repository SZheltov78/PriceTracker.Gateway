using Microsoft.EntityFrameworkCore;
using PriceTracker.Wildberries.Worker.Entities;

namespace PriceTracker.Wildberries.Worker.Data;

public class WbDbContext : DbContext
{
    public WbDbContext(DbContextOptions<WbDbContext> options)
        : base(options)
    {
    }

    public DbSet<WbTask> WbTasks { get; set; }
    public DbSet<PriceHistory> PriceHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WbTask>(entity =>
        {
            entity.ToTable("WbTasks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ThresholdPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasMany(e => e.PriceHistories)
                .WithOne(e => e.WbTask)
                .HasForeignKey(e => e.WbTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.ToTable("PriceHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.WbTaskId);
            entity.HasIndex(e => e.CheckedAt);
        });
    }
}