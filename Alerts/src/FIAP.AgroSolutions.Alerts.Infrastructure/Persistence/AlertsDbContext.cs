using FIAP.AgroSolutions.Alerts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;

public class AlertsDbContext : DbContext, IUnitOfWork
{
    public AlertsDbContext(DbContextOptions<AlertsDbContext> options) : base(options) { }

    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<FieldState> FieldStates => Set<FieldState>();
    public DbSet<RuleState> RuleStates => Set<RuleState>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(b =>
        {
            b.ToTable("Alerts");
            b.HasKey(x => x.Id);
            b.Property(x => x.FieldId).IsRequired();
            b.Property(x => x.Type).HasConversion<int>().IsRequired();
            b.Property(x => x.Severity).HasConversion<int>().IsRequired();
            b.Property(x => x.Message).IsRequired().HasMaxLength(500);
            b.Property(x => x.TriggeredAtUtc).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.HasIndex(x => new { x.FieldId, x.Type, x.TriggeredAtUtc });
        });

        modelBuilder.Entity<FieldState>(b =>
        {
            b.ToTable("FieldStates");
            b.HasKey(x => x.FieldId);
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasMany(x => x.Rules)
                .WithOne()
                .HasForeignKey(x => x.FieldId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RuleState>(b =>
        {
            b.ToTable("RuleStates");
            b.HasKey(x => x.Id);
            b.Property(x => x.RuleKey).IsRequired().HasMaxLength(80);
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasIndex(x => new { x.FieldId, x.RuleKey }).IsUnique();
        });

        modelBuilder.Entity<AlertRule>(b =>
        {
            b.ToTable("AlertRules");
            b.HasKey(x => x.Id);

            b.Property(x => x.RuleKey).IsRequired().HasMaxLength(80);
            b.HasIndex(x => x.RuleKey).IsUnique();

            b.Property(x => x.Name).IsRequired().HasMaxLength(160);
            b.Property(x => x.IsEnabled).IsRequired();

            b.Property(x => x.Type).HasConversion<int>().IsRequired();
            b.Property(x => x.Severity).HasConversion<int>().IsRequired();
            b.Property(x => x.Kind).HasConversion<int>().IsRequired();
            b.Property(x => x.Metric).HasConversion<int>().IsRequired();
            b.Property(x => x.Operator).HasConversion<int>().IsRequired();

            b.Property(x => x.ThresholdValue).HasColumnType("decimal(10,2)").IsRequired();
            b.Property(x => x.MessageTemplate).IsRequired().HasMaxLength(500);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.SecondaryMetric).HasConversion<int>();
            b.Property(x => x.SecondaryMinValue).HasColumnType("decimal(10,2)");
            b.Property(x => x.SecondaryMaxValue).HasColumnType("decimal(10,2)");

        });

        modelBuilder.Entity<SensorReading>(b =>
        {
            b.ToTable("SensorReadings");
            b.HasKey(x => x.Id);

            b.Property(x => x.FieldId).IsRequired();
            b.Property(x => x.SoilMoisturePercent).HasColumnType("decimal(5,2)");
            b.Property(x => x.TemperatureC).HasColumnType("decimal(6,2)");
            b.Property(x => x.RainMm).HasColumnType("decimal(10,2)");
            b.Property(x => x.MeasuredAtUtc).IsRequired();

            b.HasIndex(x => new { x.FieldId, x.MeasuredAtUtc });
        });
    }
}
