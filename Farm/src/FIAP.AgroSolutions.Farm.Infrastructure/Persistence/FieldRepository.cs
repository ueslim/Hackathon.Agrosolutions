using FIAP.AgroSolutions.Farm.Application.Abstractions;
using FIAP.AgroSolutions.Farm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Farm.Infrastructure.Persistence;

public class FieldRepository : IFieldRepository
{
    private readonly FarmDbContext _db;

    public FieldRepository(FarmDbContext db) => _db = db;

    public Task<Field?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken ct) =>
        _db.Fields.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerUserId == ownerUserId, ct);

    public Task<List<Field>> GetAllAsync(Guid ownerUserId, Guid? farmId, CancellationToken ct)
    {
        var q = _db.Fields.AsNoTracking().Where(x => x.OwnerUserId == ownerUserId);
        if (farmId.HasValue)
        {
            q = q.Where(x => x.FarmId == farmId.Value);
        }

        return q.ToListAsync(ct);
    }

    public async Task AddAsync(Field field, CancellationToken ct)
    {
        await _db.Fields.AddAsync(field, ct);
        await _db.SaveChangesAsync(ct);
    }
}
