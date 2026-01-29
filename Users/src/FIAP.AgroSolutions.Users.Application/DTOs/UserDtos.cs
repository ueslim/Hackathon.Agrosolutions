namespace FIAP.AgroSolutions.Users.Application.DTOs;

public record CreateUserRequest(string Name, string Email, string Password);

public record UpdateUserRequest(string Name);

public record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAtUtc);

public record LoginRequest(string Email, string Password);

public record LoginResponse(Guid UserId, string Name, string Email, string Token);
