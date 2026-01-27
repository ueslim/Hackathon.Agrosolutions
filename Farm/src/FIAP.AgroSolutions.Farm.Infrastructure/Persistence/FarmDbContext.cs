using FIAP.AgroSolutions.Farm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Farm.Infrastructure.Persistence;

public class FarmDbContext : DbContext
{
    public FarmDbContext(DbContextOptions<FarmDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.Farm> Farms => Set<Domain.Entities.Farm>();
    public DbSet<Field> Fields => Set<Field>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Farm>(b =>
        {
            b.ToTable("Farms");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(120);
            b.Property(x => x.LocationDescription).HasMaxLength(250);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasMany(x => x.Fields)
             .WithOne(x => x.Farm!)
             .HasForeignKey(x => x.FarmId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Field>(b =>
        {
            b.ToTable("Fields");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.OwnerUserId).IsRequired();
            b.Property(x => x.FarmId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(120);
            b.Property(x => x.Crop).IsRequired().HasMaxLength(80);
            b.Property(x => x.BoundaryDescription).HasMaxLength(500);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => new { x.OwnerUserId, x.FarmId });
        });
    }
}
