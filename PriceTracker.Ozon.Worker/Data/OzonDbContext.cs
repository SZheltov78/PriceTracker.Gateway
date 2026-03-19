using Microsoft.EntityFrameworkCore;
using PriceTracker.Ozon.Worker.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PriceTracker.Ozon.Worker.Data;

public class OzonDbContext : DbContext
{
    public OzonDbContext(DbContextOptions<OzonDbContext> options)
        : base(options)
    {
    }

    public DbSet<OzonTask> OzonTasks { get; set; }
    public DbSet<PriceHistory> PriceHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OzonTask>(entity =>
        {
            entity.ToTable("OzonTasks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ThresholdPrice)
                .HasPrecision(18, 2);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasMany(e => e.PriceHistories)
                .WithOne(e => e.OzonTask)
                .HasForeignKey(e => e.OzonTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.ToTable("PriceHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.OzonTaskId);
            entity.HasIndex(e => e.CheckedAt);
        });
    }
}