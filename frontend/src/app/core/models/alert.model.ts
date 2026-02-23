export interface AlertResponse {
  id: string;
  fieldId: string;
  type: string;
  severity: string;
  message: string;
  triggeredAtUtc: string;
  resolvedAtUtc: string | null;
}
