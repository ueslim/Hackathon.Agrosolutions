namespace FIAP.AgroSolutions.Farm.Domain.Entities;

/// <summary>
/// Representa uma fazenda (propriedade agrícola).
/// 
/// Uma Farm agrupa vários campos agrícolas (talhões),
/// que são as áreas monitoradas individualmente pelo sistema.
/// </summary>
public class Farm
{
    /// <summary>
    /// Identificador único da fazenda.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Usuário proprietário ou responsável pela fazenda.
    /// </summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>
    /// Nome da fazenda (ex: "Fazenda Santa Maria").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descrição opcional da localização da fazenda.
    /// Pode ser endereço, região ou coordenadas aproximadas.
    /// </summary>
    public string? LocationDescription { get; set; }

    /// <summary>
    /// Data de criação do registro da fazenda no sistema.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Lista de campos agrícolas (talhões) pertencentes à fazenda.
    /// 
    /// Cada Field representa uma área específica monitorada
    /// individualmente por sensores e regras de alerta.
    /// </summary>
    public List<Field> Fields { get; set; } = new();
}
