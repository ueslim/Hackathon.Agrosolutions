namespace FIAP.AgroSolutions.Alerts.Domain.Enums;

public enum AlertType
{
    // Água / solo
    Drought = 1,          // seca prolongada
    Waterlogging = 2,     // solo encharcado
    NoRain = 3,           // ausência de chuva por período longo

    // Clima / temperatura
    HeatStress = 4,       // calor excessivo
    ColdStress = 5,       // frio excessivo / quase geada
    FrostRisk = 6,        // risco real de geada (mais extremo)

    // Chuva / eventos
    HeavyRain = 7,        // chuva intensa

    // Biológico / operacional
    DiseaseRisk = 8,      // risco de praga/doença (proxy climático)
    SensorStale = 9       // sensor sem enviar dados
}
