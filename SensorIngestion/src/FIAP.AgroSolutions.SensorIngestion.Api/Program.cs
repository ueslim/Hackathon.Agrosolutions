using FIAP.AgroSolutions.SensorIngestion.Api.Security;
using FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;
using FIAP.AgroSolutions.SensorIngestion.Application.Services;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Outbox;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Persistence;
using FIAP.AgroSolutions.SensorIngestion.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dev Auth (bypass)
builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

// Db
builder.Services.AddDbContext<IngestionDbContext>(o =>
    o.UseSqlServer(config.GetConnectionString("SensorIngestionDb")));

// DI
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<IOutboxWriter, OutboxWriter>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IngestionDbContext>());
builder.Services.AddScoped<ReadingService>();

builder.Services.AddHostedService<OutboxPublisher>();

var app = builder.Build();

await DatabaseInitializer.EnsureDatabaseMigratedAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
