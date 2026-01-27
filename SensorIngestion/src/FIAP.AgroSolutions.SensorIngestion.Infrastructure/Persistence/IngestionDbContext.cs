using FIAP.AgroSolutions.SensorIngestion.Domain.Entities;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.SensorIngestion.Infrastructure.Persistence;

public class IngestionDbContext : DbContext
{
    public IngestionDbContext(DbContextOptions<IngestionDbContext> options) : base(options) { }

    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReading>(b =>
        {
            b.ToTable("SensorReadings");
            b.HasKey(x => x.Id);
            b.Property(x => x.FieldId).IsRequired();
            b.Property(x => x.SoilMoisturePercent).HasColumnType("decimal(5,2)");
            b.Property(x => x.TemperatureC).HasColumnType("decimal(6,2)");
            b.Property(x => x.RainMm).HasColumnType("decimal(8,2)");
            b.Property(x => x.MeasuredAtUtc).IsRequired();
            b.Property(x => x.ReceivedAtUtc).IsRequired();

            b.HasIndex(x => new { x.FieldId, x.MeasuredAtUtc });
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(128);
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.OccurredAtUtc).IsRequired();
            b.Property(x => x.ProcessedAtUtc);
            b.Property(x => x.AttemptCount).HasDefaultValue(0);
            b.Property(x => x.LastError).HasMaxLength(1000);

            b.HasIndex(x => x.ProcessedAtUtc);
            b.HasIndex(x => x.OccurredAtUtc);
        });
    }
}
