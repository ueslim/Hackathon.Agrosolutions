using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FIAP.AgroSolutions.SensorIngestion.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task EnsureDatabaseMigratedAsync(IServiceProvider services)
    {
        const int maxRetries = 10;
        const int delaySeconds = 6;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();
                var creator = db.GetService<IRelationalDatabaseCreator>();

                if (!await creator.ExistsAsync())
                {
                    Log.Warning("SensorIngestionDb não existe. Criando...");
                    await creator.CreateAsync();
                }

                var pending = await db.Database.GetPendingMigrationsAsync();
                if (pending.Any())
                {
                    Log.Information("Aplicando {Count} migrations pendentes...", pending.Count());
                    await db.Database.MigrateAsync();
                    Log.Information("Migrations aplicadas com sucesso");
                }
                else
                {
                    Log.Information("Banco atualizado, nenhuma migration pendente");
                }

                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Log.Warning(ex, "Tentativa {Attempt}/{Max} falhou. Aguardando {Delay}s...",
                    attempt, maxRetries, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        throw new InvalidOperationException("Não foi possível migrar o banco após várias tentativas.");
    }
}
