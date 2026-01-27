using FIAP.AgroSolutions.Farm.Api.Security;
using FIAP.AgroSolutions.Farm.Application.Abstractions;
using FIAP.AgroSolutions.Farm.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Farm.Api.Controllers;

[ApiController]
[Route("api/farms")]
[Authorize]
public class FarmsController : ControllerBase
{
    private readonly IFarmService _service;

    public FarmsController(IFarmService service)
    {
        _service = service;
    }

    [HttpGet("all-with-fields")]
    public async Task<ActionResult<List<FarmWithFieldsResponse>>> GetAllWithFields(CancellationToken ct)
    {
        return Ok(await _service.GetAllFarmsWithFieldsAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<FarmResponse>> Create([FromBody] CreateFarmRequest req, CancellationToken ct)
    {
        var userId = UserContext.GetUserId(HttpContext);
        var created = await _service.CreateFarmAsync(userId, req, ct);
        return CreatedAtAction(nameof(GetById), new { farmId = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<List<FarmResponse>>> GetAll(CancellationToken ct)
    {
        var userId = UserContext.GetUserId(HttpContext);
        return Ok(await _service.GetFarmsAsync(userId, ct));
    }

    [HttpGet("{farmId:guid}")]
    public async Task<ActionResult<FarmResponse>> GetById(Guid farmId, CancellationToken ct)
    {
        var userId = UserContext.GetUserId(HttpContext);
        var farms = await _service.GetFarmsAsync(userId, ct);
        var farm = farms.FirstOrDefault(x => x.Id == farmId);
        return farm is null ? NotFound() : Ok(farm);
    }

    [HttpPost("{farmId:guid}/fields")]
    public async Task<ActionResult<FieldResponse>> CreateField(Guid farmId, [FromBody] CreateFieldRequest req, CancellationToken ct)
    {
        var userId = UserContext.GetUserId(HttpContext);
        var created = await _service.CreateFieldAsync(userId, farmId, req, ct);
        return Created($"/api/fields/{created.Id}", created);
    }
}
