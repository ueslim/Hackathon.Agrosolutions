namespace FIAP.AgroSolutions.SensorIngestion.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
