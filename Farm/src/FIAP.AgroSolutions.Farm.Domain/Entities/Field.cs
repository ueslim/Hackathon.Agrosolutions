namespace FIAP.AgroSolutions.Farm.Domain.Entities;

/// <summary>
/// Representa um campo agrícola monitorado pelo sistema.
/// 
/// Um Field é a unidade básica de monitoramento:
/// é nele que sensores coletam dados e onde alertas são gerados.
/// </summary>
public class Field
{
    /// <summary>
    /// Identificador único do campo.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador da fazenda à qual o campo pertence.
    /// </summary>
    public Guid FarmId { get; set; }

    /// <summary>
    /// Navegação para a fazenda (contexto organizacional).
    /// </summary>
    public Farm? Farm { get; set; }

    /// <summary>
    /// Usuário responsável / proprietário do campo.
    /// Usado para controle de acesso e visibilidade.
    /// </summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>
    /// Nome amigável do campo (ex: "Talhão Norte").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cultura plantada no campo (ex: milho, soja, trigo).
    /// </summary>
    public string Crop { get; set; } = string.Empty;

    /// <summary>
    /// Descrição opcional dos limites do campo.
    /// Pode representar coordenadas, polígono ou texto livre.
    /// </summary>
    public string? BoundaryDescription { get; set; }

    /// <summary>
    /// Momento de criação do registro do campo.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
