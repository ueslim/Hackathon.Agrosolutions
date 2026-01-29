namespace FIAP.AgroSolutions.Alerts.Domain.Enums;

/// <summary>
/// Define os tipos de alertas que podem ser gerados pelo sistema,
/// representando diferentes riscos agronômicos, climáticos e operacionais.
/// </summary>
public enum AlertType
{
    // Água / solo

    /// <summary>
    /// Indica seca prolongada, quando a umidade do solo permanece
    /// abaixo do nível adequado por um período significativo.
    /// </summary>
    Drought = 1,

    /// <summary>
    /// Indica solo encharcado, normalmente associado a excesso de água
    /// e risco de doenças radiculares.
    /// </summary>
    Waterlogging = 2,

    /// <summary>
    /// Indica ausência de chuva por um longo período,
    /// avaliada por agregação de dados em uma janela de tempo.
    /// </summary>
    NoRain = 3,

    // Clima / temperatura

    /// <summary>
    /// Indica estresse térmico causado por temperaturas elevadas
    /// mantidas por um período contínuo.
    /// </summary>
    HeatStress = 4,

    /// <summary>
    /// Indica estresse por frio, quando a temperatura permanece
    /// abaixo de níveis seguros para a cultura.
    /// </summary>
    ColdStress = 5,

    /// <summary>
    /// Indica risco elevado de geada, condição mais extrema
    /// associada a danos severos às plantas.
    /// </summary>
    FrostRisk = 6,

    // Chuva / eventos extremos

    /// <summary>
    /// Indica ocorrência de chuva intensa em curto intervalo de tempo,
    /// podendo causar erosão, alagamentos ou perdas de nutrientes.
    /// </summary>
    HeavyRain = 7,

    // Biológico / operacional

    /// <summary>
    /// Indica risco de pragas ou doenças, avaliado a partir de
    /// condições climáticas favoráveis (ex: alta umidade e temperatura).
    /// </summary>
    DiseaseRisk = 8,

    /// <summary>
    /// Indica que o sensor deixou de enviar leituras por um período
    /// acima do esperado, caracterizando falha operacional.
    /// </summary>
    SensorStale = 9
}
