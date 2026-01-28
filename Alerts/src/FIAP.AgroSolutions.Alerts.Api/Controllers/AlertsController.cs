using FIAP.AgroSolutions.Alerts.Application.DTOs;
using FIAP.AgroSolutions.Alerts.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Alerts.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly AlertsQueryService _service;
    private readonly AlertsCommandService _commands;

    public AlertsController(AlertsQueryService service, AlertsCommandService commands)
    {
        _service = service;
        _commands = commands;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertResponse>>> GetByField([FromQuery] Guid fieldId, CancellationToken ct)
    {
        if (fieldId == Guid.Empty)
        {
            return BadRequest("fieldId is required");
        }

        return Ok(await _service.GetAlertsByFieldAsync(fieldId, ct));
    }

    [HttpPatch("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct)
    {
        try
        {
            await _commands.ResolveAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
