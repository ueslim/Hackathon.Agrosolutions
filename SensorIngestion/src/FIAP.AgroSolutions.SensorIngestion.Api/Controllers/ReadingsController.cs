using FIAP.AgroSolutions.SensorIngestion.Application.DTOs;
using FIAP.AgroSolutions.SensorIngestion.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.SensorIngestion.Api.Controllers;

[ApiController]
[Route("api/readings")]
[Authorize]
public class ReadingsController : ControllerBase
{
    private readonly ReadingService _service;

    public ReadingsController(ReadingService service) => _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(ReadingResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReadingResponse>> Create([FromBody] CreateReadingRequest req, CancellationToken ct)
    {
        var created = await _service.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetByField), new { fieldId = created.FieldId }, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReadingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadingResponse>>> GetByField(
        [FromQuery] Guid fieldId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int take = 200,
        CancellationToken ct = default)
    {
        if (fieldId == Guid.Empty)
        {
            return BadRequest(new { message = "fieldId is required" });
        }

        var list = await _service.GetByFieldAsync(fieldId, fromUtc, toUtc, take, ct);
        return Ok(list);
    }
}
