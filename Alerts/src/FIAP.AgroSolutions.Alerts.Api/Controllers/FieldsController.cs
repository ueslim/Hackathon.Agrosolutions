using FIAP.AgroSolutions.Alerts.Application.DTOs;
using FIAP.AgroSolutions.Alerts.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Alerts.Api.Controllers;

[ApiController]
[Route("api/fields")]
public class FieldsController : ControllerBase
{
    private readonly AlertsQueryService _service;

    public FieldsController(AlertsQueryService service) => _service = service;

    [HttpGet("{fieldId:guid}/status")]
    public async Task<ActionResult<FieldStatusResponse>> GetStatus(Guid fieldId, CancellationToken ct)
    {
        return Ok(await _service.GetFieldStatusAsync(fieldId, ct));
    }
}
