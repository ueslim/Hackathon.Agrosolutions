using FIAP.AgroSolutions.SensorIngestion.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.SensorIngestion.Api.Controllers;

/// <summary>
/// Controller utilitário para simulação de leituras de sensores.
/// 
/// Usado principalmente para testes e demonstrações (hackathon),
/// permitindo gerar dados artificiais que disparam alertas específicos.
/// </summary>
[ApiController]
[Route("api/simulate")]
[Authorize]
public class SimulationController : ControllerBase
{
    private readonly SensorSimulatorService _sim;

    public SimulationController(SensorSimulatorService sim) => _sim = sim;

    /// <summary>
    /// Gera um conjunto de leituras simuladas para um campo agrícola.
    /// 
    /// As leituras são distribuídas no tempo e publicadas como se
    /// viessem de sensores reais.
    /// </summary>
    /// <param name="fieldId">
    /// Identificador do campo (talhão) que receberá as leituras simuladas.
    /// </param>
    /// <param name="count">
    /// Quantidade total de leituras a serem geradas.
    /// Exemplo: 50 leituras.
    /// </param>
    /// <param name="stepSeconds">
    /// Intervalo, em segundos, entre uma leitura e outra.
    /// Exemplo: 60 = uma leitura por minuto.
    /// </param>
    /// <param name="scenario">
    /// Cenário climático a ser simulado.
    /// 
    /// Exemplos:
    /// - normal       → condições médias
    /// - drought      → seca prolongada (baixa umidade, pouca chuva)
    /// - heat         → calor contínuo
    /// - heavyrain    → eventos de chuva intensa
    /// - norain       → ausência de chuva por longo período
    /// - disease      → condições favoráveis a pragas/doenças
    /// </param>
    /// <returns>
    /// Retorna HTTP 202 (Accepted) indicando que a simulação foi iniciada.
    /// </returns>
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

        // Gera leituras simuladas e publica como se fossem reais
        await _sim.GenerateBurstAsync(
            fieldId,
            count,
            TimeSpan.FromSeconds(stepSeconds),
            scenario,
            ct);

        return Accepted(new
        {
            fieldId,
            count,
            stepSeconds,
            scenario
        });
    }
}
