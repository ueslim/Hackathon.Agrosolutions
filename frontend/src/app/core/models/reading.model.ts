export interface ReadingResponse {
  id: string;
  fieldId: string;
  soilMoisturePercent: number;
  temperatureC: number;
  rainMm: number;
  measuredAtUtc: string;
  receivedAtUtc: string;
}
