using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Application.Services;
using FIAP.AgroSolutions.Alerts.Infrastructure.Consumers;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence;
using FIAP.AgroSolutions.Alerts.Infrastructure.Persistence.Seed;
using FIAP.AgroSolutions.Alerts.Infrastructure.Queries;
using FIAP.AgroSolutions.Alerts.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db
builder.Services.AddDbContext<AlertsDbContext>(o =>
    o.UseSqlServer(config.GetConnectionString("AlertsDb")));

// DI
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IFieldStateRepository, FieldStateRepository>();
builder.Services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();
builder.Services.AddScoped<IReadingsQuery, ReadingsQuery>();

builder.Services.AddScoped<AlertEngineService>();
builder.Services.AddScoped<AlertsQueryService>();
builder.Services.AddScoped<AlertsCommandService>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AlertsDbContext>());
builder.Services.AddScoped<SensorStaleService>();
builder.Services.AddHostedService<SensorStaleWorker>();

// Rabbit Consumer
builder.Services.AddHostedService<SensorReadingConsumer>();

// CORS - allow Angular frontend (dev and local production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Total", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200");
        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AlertsDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        await RulesSeeder.SeedAsync(db);
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Total");
app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
