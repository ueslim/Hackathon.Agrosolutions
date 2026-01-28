using FIAP.AgroSolutions.SensorIngestion.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.SensorIngestion.Api.Controllers;

[ApiController]
[Route("api/simulate")]
[Authorize]
public class SimulationController : ControllerBase
{
    private readonly SensorSimulatorService _sim;

    public SimulationController(SensorSimulatorService sim) => _sim = sim;

    [HttpPost("burst")]
    public async Task<IActionResult> Burst([FromQuery] Guid fieldId, [FromQuery] int count = 50, [FromQuery] int stepSeconds = 60, [FromQuery] string scenario = "normal", CancellationToken ct = default)
    {
        if (fieldId == Guid.Empty)
        {
            return BadRequest("fieldId is required");
        }

        if (count <= 0 || count > 2000)
        {
            return BadRequest("count must be 1..2000");
        }

        await _sim.GenerateBurstAsync(fieldId, count, TimeSpan.FromSeconds(stepSeconds), scenario, ct);
        return Accepted(new { fieldId, count, stepSeconds, scenario });
    }
}
