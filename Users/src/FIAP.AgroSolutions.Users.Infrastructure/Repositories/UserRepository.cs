using FIAP.AgroSolutions.Users.Application.Abstractions;
using FIAP.AgroSolutions.Users.Domain.Entities;
using FIAP.AgroSolutions.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FIAP.AgroSolutions.Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _db;

    public UserRepository(UsersDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct)
        => _db.Users.OrderBy(x => x.CreatedAtUtc).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct)
        => await _db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
