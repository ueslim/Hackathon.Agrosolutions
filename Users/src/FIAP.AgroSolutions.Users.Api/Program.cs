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
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        await SeedData.EnsureSeededAsync(db);
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Total");
app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
