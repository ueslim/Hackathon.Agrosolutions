using FIAP.AgroSolutions.Farm.Api.Security;
using FIAP.AgroSolutions.Farm.Application.DTOs;
using FIAP.AgroSolutions.Farm.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Farm.Api.Controllers;

[ApiController]
[Route("api/fields")]
[Authorize]
public class FieldsController : ControllerBase
{
    private readonly FarmService _service;

    public FieldsController(FarmService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<FieldResponse>>> GetAll([FromQuery] Guid? farmId, CancellationToken ct)
    {
        var userId = UserContext.GetUserId(HttpContext);
        return Ok(await _service.GetFieldsAsync(userId, farmId, ct));
    }

    [HttpGet("{fieldId:guid}")]
    public async Task<ActionResult<FieldResponse>> GetById(Guid fieldId, CancellationToken ct)
    {
        var userId = UserContext.GetUserId(HttpContext);
        var field = await _service.GetFieldByIdAsync(userId, fieldId, ct);
        return field is null ? NotFound() : Ok(field);
    }
}
