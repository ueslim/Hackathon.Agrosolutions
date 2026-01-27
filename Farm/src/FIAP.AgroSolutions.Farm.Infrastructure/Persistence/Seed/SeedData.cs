using FIAP.AgroSolutions.Farm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Farm.Infrastructure.Persistence.Seed;

public static class SeedData
{
    // ===== Usuários (Produtores) =====
    public static readonly Guid User1 = Guid.Parse("9c1b8c6a-9f8a-4e6f-9c6d-8b7d9b8b6c11");
    public static readonly Guid User2 = Guid.Parse("a7d3b92f-6c3f-4e1b-8f6a-3c9e7b4a2d22");
    public static readonly Guid User3 = Guid.Parse("b3e91c44-2d7e-4a4c-b8b5-4f9d2c6a3e33");

    public static async Task EnsureSeededAsync(FarmDbContext db, CancellationToken ct = default)
    {
        if (await db.Farms.AsNoTracking().AnyAsync(ct))
        {
            return;
        }

        var now = DateTime.UtcNow;

        // ===== Farms =====
        var farm1 = new Domain.Entities.Farm
        {
            Id = Guid.Parse("4f8c6e21-4a5c-4f9a-8e4b-2d7e91c44a01"),
            OwnerUserId = User1,
            Name = "Fazenda Santa Clara",
            LocationDescription = "Sorriso - MT (região do médio norte)",
            CreatedAtUtc = now.AddDays(-60)
        };

        var farm2 = new Domain.Entities.Farm
        {
            Id = Guid.Parse("7a2c8e1b-3f6a-4b9e-8d2f-91c44a5e2b02"),
            OwnerUserId = User1,
            Name = "Sítio Boa Esperança",
            LocationDescription = "Rio Verde - GO (entorno da BR-060)",
            CreatedAtUtc = now.AddDays(-25)
        };

        var farm3 = new Domain.Entities.Farm
        {
            Id = Guid.Parse("9d4a5c6e-2b1f-4e8a-91c4-4a5e2b7d3c03"),
            OwnerUserId = User2,
            Name = "Fazenda Horizonte",
            LocationDescription = "Rondonópolis - MT (região sul)",
            CreatedAtUtc = now.AddDays(-90)
        };

        var farm4 = new Domain.Entities.Farm
        {
            Id = Guid.Parse("2c6a3e91-4a5e-4b8b-9d2c-7e1b3f6a4c04"),
            OwnerUserId = User3,
            Name = "Estância Bela Vista",
            LocationDescription = "Uberaba - MG (Triângulo Mineiro)",
            CreatedAtUtc = now.AddDays(-120)
        };

        await db.Farms.AddRangeAsync(new[] { farm1, farm2, farm3, farm4 }, ct);

        // ===== Fields (Talhões) =====
        var fields = new List<Field>
        {
            // Fazenda Santa Clara (User1)
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-1111-4a6b-8c9d-01a2b3c4d101"),
                FarmId = farm1.Id,
                OwnerUserId = User1,
                Name = "Talhão 01 - Soja",
                Crop = "Soja",
                BoundaryDescription = "Área plana, boa retenção de umidade",
                CreatedAtUtc = now.AddDays(-55)
            },
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-2222-4a6b-8c9d-01a2b3c4d102"),
                FarmId = farm1.Id,
                OwnerUserId = User1,
                Name = "Talhão 02 - Milho",
                Crop = "Milho",
                BoundaryDescription = "Área próxima a córrego, solo argiloso",
                CreatedAtUtc = now.AddDays(-54)
            },
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-3333-4a6b-8c9d-01a2b3c4d103"),
                FarmId = farm1.Id,
                OwnerUserId = User1,
                Name = "Talhão 03 - Algodão",
                Crop = "Algodão",
                BoundaryDescription = "Histórico de incidência de pragas",
                CreatedAtUtc = now.AddDays(-53)
            },

            // Sítio Boa Esperança (User1)
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-4444-4a6b-8c9d-01a2b3c4d201"),
                FarmId = farm2.Id,
                OwnerUserId = User1,
                Name = "Talhão A - Café",
                Crop = "Café",
                BoundaryDescription = "Região sombreada, declive leve",
                CreatedAtUtc = now.AddDays(-22)
            },
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-5555-4a6b-8c9d-01a2b3c4d202"),
                FarmId = farm2.Id,
                OwnerUserId = User1,
                Name = "Talhão B - Feijão",
                Crop = "Feijão",
                BoundaryDescription = "Área irrigada por aspersão",
                CreatedAtUtc = now.AddDays(-21)
            },

            // Fazenda Horizonte (User2)
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-6666-4a6b-8c9d-01a2b3c4d301"),
                FarmId = farm3.Id,
                OwnerUserId = User2,
                Name = "Talhão 01 - Soja",
                Crop = "Soja",
                BoundaryDescription = "Solo arenoso, drenagem rápida",
                CreatedAtUtc = now.AddDays(-88)
            },
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-7777-4a6b-8c9d-01a2b3c4d302"),
                FarmId = farm3.Id,
                OwnerUserId = User2,
                Name = "Talhão 02 - Milheto",
                Crop = "Milheto",
                BoundaryDescription = "Cobertura vegetal no período seco",
                CreatedAtUtc = now.AddDays(-87)
            },

            // Estância Bela Vista (User3)
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-8888-4a6b-8c9d-01a2b3c4d401"),
                FarmId = farm4.Id,
                OwnerUserId = User3,
                Name = "Talhão Norte - Cana",
                Crop = "Cana-de-açúcar",
                BoundaryDescription = "Área com leve inclinação",
                CreatedAtUtc = now.AddDays(-118)
            },
            new Field
            {
                Id = Guid.Parse("e1a2b3c4-9999-4a6b-8c9d-01a2b3c4d402"),
                FarmId = farm4.Id,
                OwnerUserId = User3,
                Name = "Talhão Sul - Pastagem",
                Crop = "Pastagem",
                BoundaryDescription = "Rotação com gado",
                CreatedAtUtc = now.AddDays(-117)
            }
        };

        await db.Fields.AddRangeAsync(fields, ct);
        await db.SaveChangesAsync(ct);
    }
}
