using FIAP.AgroSolutions.Alerts.Application.Abstractions;
using FIAP.AgroSolutions.Alerts.Application.DTOs;
using FIAP.AgroSolutions.Alerts.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.Alerts.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsResolutionController : ControllerBase
{
    private readonly IAlertRepository _alerts;
    private readonly IUnitOfWork _uow;

    public AlertsResolutionController(IAlertRepository alerts, IUnitOfWork uow)
    {
        _alerts = alerts;
        _uow = uow;
    }

    // ✅ POST /api/alerts/{alertId}/resolve
    [HttpPost("{alertId:guid}/resolve")]
    public async Task<IActionResult> ResolveById([FromRoute] Guid alertId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var count = await _alerts.ResolveAsync(alertId, now, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new ResolveResponse(count, now));
    }

    // ✅ POST /api/alerts/resolve/by-type?fieldId=...&type=NoRain
    [HttpPost("resolve/by-type")]
    public async Task<IActionResult> ResolveByType([FromQuery] Guid fieldId, [FromQuery] AlertType type, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var count = await _alerts.ResolveActiveByTypeAsync(fieldId, type, now, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new ResolveResponse(count, now));
    }

    // ✅ POST /api/alerts/resolve/all?fieldId=...
    [HttpPost("resolve/all")]
    public async Task<IActionResult> ResolveAll([FromQuery] Guid fieldId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var count = await _alerts.ResolveAllActiveAsync(fieldId, now, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new ResolveResponse(count, now));
    }
}
