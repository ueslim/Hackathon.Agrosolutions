using FIAP.AgroSolutions.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Users.Infrastructure.Persistence.Seed;

public static class SeedData
{
    // mesmos GUIDs do Farm API
    public static readonly Guid User1 = Guid.Parse("9c1b8c6a-9f8a-4e6f-9c6d-8b7d9b8b6c11");
    public static readonly Guid User2 = Guid.Parse("a7d3b92f-6c3f-4e1b-8f6a-3c9e7b4a2d22");
    public static readonly Guid User3 = Guid.Parse("b3e91c44-2d7e-4a4c-b8b5-4f9d2c6a3e33");

    public static async Task EnsureSeededAsync(UsersDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AsNoTracking().AnyAsync(ct))
        {
            return;
        }

        var now = DateTime.UtcNow;

        // senha padrão do MVP
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("1234");

        var users = new[]
        {
            new User
            {
                Id = User1,
                Name = "Tony Stark",
                Email = "tony.stark@marvel.local",
                PasswordHash = defaultPasswordHash,
                CreatedAtUtc = now.AddDays(-30)
            },
            new User
            {
                Id = User2,
                Name = "Natasha Romanoff",
                Email = "natasha.romanoff@marvel.local",
                PasswordHash = defaultPasswordHash,
                CreatedAtUtc = now.AddDays(-20)
            },
            new User
            {
                Id = User3,
                Name = "Steve Rogers",
                Email = "steve.rogers@marvel.local",
                PasswordHash = defaultPasswordHash,
                CreatedAtUtc = now.AddDays(-10)
            }
        };

        await db.Users.AddRangeAsync(users, ct);
        await db.SaveChangesAsync(ct);
    }
}
