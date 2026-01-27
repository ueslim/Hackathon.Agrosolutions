using FIAP.AgroSolutions.Farm.Application.Abstractions;
using FIAP.AgroSolutions.Farm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FarmEntity = FIAP.AgroSolutions.Farm.Domain.Entities.Farm;

namespace FIAP.AgroSolutions.Farm.Infrastructure.Repositories;

public class FarmRepository : IFarmRepository
{
    private readonly FarmDbContext _db;

    public FarmRepository(FarmDbContext db) => _db = db;

    public async Task<List<Domain.Entities.Farm>> GetAllWithFieldsAsync(CancellationToken ct)
    {
        return await _db.Farms
            .AsNoTracking()
            .Include(f => f.Fields)
            .ToListAsync(ct);
    }


    public Task<FarmEntity?> GetByIdAsync(Guid id, Guid ownerUserId, CancellationToken ct) =>
        _db.Farms.FirstOrDefaultAsync(x => x.Id == id && x.OwnerUserId == ownerUserId, ct);

    public Task<List<FarmEntity>> GetAllAsync(Guid ownerUserId, CancellationToken ct) =>
        _db.Farms.AsNoTracking().Where(x => x.OwnerUserId == ownerUserId).ToListAsync(ct);

    public async Task AddAsync(FarmEntity farm, CancellationToken ct)
    {
        await _db.Farms.AddAsync(farm, ct);
        await _db.SaveChangesAsync(ct);
    }
}
