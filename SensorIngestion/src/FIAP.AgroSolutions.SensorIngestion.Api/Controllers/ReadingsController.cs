using FIAP.AgroSolutions.SensorIngestion.Application.DTOs;
using FIAP.AgroSolutions.SensorIngestion.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.AgroSolutions.SensorIngestion.Api.Controllers;

/// <summary>
/// Controller responsável por receber leituras de sensores.
/// 
/// Este é o principal ponto de entrada de dados do sistema:
/// todas as medições ambientais passam por aqui antes de serem
/// publicadas para processamento de alertas.
/// </summary>
[ApiController]
[Route("api/readings")]
[Authorize]
public class ReadingsController : ControllerBase
{
    private readonly ReadingService _service;

    public ReadingsController(ReadingService service) => _service = service;

    /// <summary>
    /// Recebe uma nova leitura de sensor e a registra no sistema.
    /// 
    /// Após o registro, a leitura é publicada para processamento
    /// assíncrono pelo serviço de alertas.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReadingResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReadingResponse>> Create([FromBody] CreateReadingRequest req, CancellationToken ct)
    {
        var created = await _service.CreateAsync(req, ct);

        // Retorna 201 Created com referência ao campo associado
        return CreatedAtAction(
            nameof(GetByField),
            new { fieldId = created.FieldId },
            created);
    }

    /// <summary>
    /// Retorna o histórico de leituras de um campo agrícola.
    /// 
    /// Pode ser filtrado por período (fromUtc / toUtc) e limitado
    /// por quantidade para evitar retornos muito grandes.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadingResponse>>> GetByField([FromQuery] Guid fieldId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        if (fieldId == Guid.Empty)
        {
            return BadRequest(new { message = "fieldId is required" });
        }

        var list = await _service.GetByFieldAsync(fieldId, fromUtc, toUtc, take, ct);
        return Ok(list);
    }
}
