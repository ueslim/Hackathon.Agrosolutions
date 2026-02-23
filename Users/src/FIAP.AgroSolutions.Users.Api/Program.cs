using FIAP.AgroSolutions.Users.Application.Abstractions;
using FIAP.AgroSolutions.Users.Application.Services;
using FIAP.AgroSolutions.Users.Infrastructure.Persistence;
using FIAP.AgroSolutions.Users.Infrastructure.Persistence.Seed;
using FIAP.AgroSolutions.Users.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
builder.Services.AddDbContext<UsersDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("UsersDb")));

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UsersService>();

// CORS - allow Angular frontend (dev and Docker/local production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Total", policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200");
        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Handle CORS preflight (OPTIONS) immediately with explicit headers so browser gets Access-Control-Allow-Origin
app.Use(async (context, next) =>
{
    if (context.Request.Method != "OPTIONS")
    {
        await next();
        return;
    }
    var origin = context.Request.Headers.Origin.FirstOrDefault();
    var allowed = new[] { "http://localhost:4200", "https://localhost:4200", "http://127.0.0.1:4200", "https://127.0.0.1:4200" };
    if (!string.IsNullOrEmpty(origin) && allowed.Contains(origin, StringComparer.OrdinalIgnoreCase))
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = origin;
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, Accept, Origin, X-Requested-With, x-dev-user-id";
        context.Response.Headers["Access-Control-Max-Age"] = "86400";
    }
    context.Response.StatusCode = 204;
});

// CORS for actual requests (GET, POST, etc.)
app.UseCors("Total");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        await SeedData.EnsureSeededAsync(db);
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
