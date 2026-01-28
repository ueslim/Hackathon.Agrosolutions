using FIAP.AgroSolutions.Farm.Api.Security;
using FIAP.AgroSolutions.Farm.Application.Abstractions;
using FIAP.AgroSolutions.Farm.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Farm.Api.Controllers;

[ApiController]
[Route("api/fields")]
[Authorize]
public class FieldsController : ControllerBase
{
    private readonly IFarmService _service;

    public FieldsController(IFarmService service) => _service = service;

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
