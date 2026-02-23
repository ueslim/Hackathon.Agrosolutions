export interface CreateFarmRequest {
  name: string;
  locationDescription: string | null;
}

export interface FarmResponse {
  id: string;
  name: string;
  locationDescription: string | null;
  createdAtUtc: string;
}

export interface CreateFieldRequest {
  name: string;
  crop: string;
  boundaryDescription: string | null;
}

export interface FieldResponse {
  id: string;
  farmId: string;
  name: string;
  crop: string;
  boundaryDescription: string | null;
  createdAtUtc: string;
}

export interface FarmWithFieldsResponse extends FarmResponse {
  fields: FieldResponse[];
}
