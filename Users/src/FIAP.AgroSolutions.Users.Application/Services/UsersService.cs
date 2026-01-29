using FIAP.AgroSolutions.Users.Application.Abstractions;
using FIAP.AgroSolutions.Users.Application.DTOs;
using FIAP.AgroSolutions.Users.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace FIAP.AgroSolutions.Users.Application.Services;

public class UsersService
{
    private readonly IUserRepository _repo;

    public UsersService(IUserRepository repo) => _repo = repo;

    public async Task<UserResponse> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        var existing = await _repo.GetByEmailAsync(req.Email, ct);
        if (existing != null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            Name = req.Name,
            Email = req.Email.Trim().ToLowerInvariant(),
            PasswordHash = Hash(req.Password)
        };

        await _repo.AddAsync(user, ct);
        await _repo.SaveChangesAsync(ct);

        return new UserResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        return user == null ? null : new UserResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc);
    }

    public async Task<UserResponse?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), ct);
        return user == null ? null : new UserResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _repo.GetByEmailAsync(email, ct);
        if (user == null)
        {
            return null;
        }

        if (user.PasswordHash != Hash(req.Password))
        {
            return null;
        }

        // token fake
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return new LoginResponse(user.Id, user.Name, user.Email, token);
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UpdateUserRequest req, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        if (user == null)
        {
            return null;
        }

        user.Name = req.Name;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _repo.SaveChangesAsync(ct);
        return new UserResponse(user.Id, user.Name, user.Email, user.CreatedAtUtc);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
