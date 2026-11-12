using CivicFix.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CivicFix.Data;

public sealed class CivicFixContext(DbContextOptions<CivicFixContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<CivicReport> Reports => Set<CivicReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CivicReport>(entity =>
        {
            entity.HasIndex(report => report.Folio).IsUnique();
            entity.Property(report => report.Folio).HasMaxLength(24).IsRequired();
            entity.Property(report => report.Category).HasMaxLength(80).IsRequired();
            entity.Property(report => report.Location).HasMaxLength(240).IsRequired();
            entity.Property(report => report.Priority).HasMaxLength(20).IsRequired();
            entity.Property(report => report.Status).HasMaxLength(24).IsRequired();
            entity.Property(report => report.CitizenName).HasMaxLength(120).IsRequired();
            entity.Property(report => report.CitizenUserId).HasMaxLength(450);
            entity.Property(report => report.Crew).HasMaxLength(80);
            entity.Property(report => report.PhotoPath).HasMaxLength(260);
        });
    }
}