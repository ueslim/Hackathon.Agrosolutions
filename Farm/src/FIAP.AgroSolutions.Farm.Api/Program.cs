using FIAP.AgroSolutions.Farm.Api.Security;
using FIAP.AgroSolutions.Farm.Application.Abstractions;
using FIAP.AgroSolutions.Farm.Application.Services;
using FIAP.AgroSolutions.Farm.Infrastructure.Persistence;
using FIAP.AgroSolutions.Farm.Infrastructure.Persistence.Seed;
using FIAP.AgroSolutions.Farm.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SQL Server
builder.Services.AddDbContext<FarmDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("FarmDb")));

// DI (Application)
builder.Services.AddScoped<IFarmService, FarmService>();

// DI (Infra)
builder.Services.AddScoped<IFarmRepository, FarmRepository>();
builder.Services.AddScoped<IFieldRepository, FieldRepository>();


if (builder.Environment.IsDevelopment())
{
    //Bypass pra testar local
    builder.Services.AddAuthentication(DevAuthHandler.Scheme)
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.Scheme, _ => { });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(o =>
       {
           o.RequireHttpsMetadata = false;
           o.TokenValidationParameters = new()
           {
               ValidateIssuer = false,
               ValidateAudience = false,
               ValidateIssuerSigningKey = false,
               ValidateLifetime = false
           };
       });
}

builder.Services.AddAuthorization();

// CORS
var isDevelopment = builder.Environment.IsDevelopment();

if (isDevelopment)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Total",
            builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
    });
}


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FarmDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        await SeedData.EnsureSeededAsync(db);
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Total");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
