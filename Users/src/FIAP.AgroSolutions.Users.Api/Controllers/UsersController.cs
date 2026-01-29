using FIAP.AgroSolutions.Users.Application.DTOs;
using FIAP.AgroSolutions.Users.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UsersService _service;

    public UsersController(UsersService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest req, CancellationToken ct)
    {
        var created = await _service.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _service.GetByIdAsync(id, ct);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<UserResponse>> GetByEmail([FromQuery] string email, CancellationToken ct)
    {
        var user = await _service.GetByEmailAsync(email, ct);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest req, CancellationToken ct)
    {
        var updated = await _service.UpdateAsync(id, req, ct);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req, CancellationToken ct)
    {
        var res = await _service.LoginAsync(req, ct);
        return res == null ? Unauthorized() : Ok(res);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue("x-dev-user-id", out var val) || !Guid.TryParse(val, out var userId))
        {
            return Unauthorized(new { message = "Missing/invalid x-dev-user-id" });
        }

        var user = await _service.GetByIdAsync(userId, ct);
        return user is null ? NotFound() : Ok(user);
    }
}
