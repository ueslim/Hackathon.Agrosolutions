import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type {
  FarmResponse,
  FarmWithFieldsResponse,
  CreateFarmRequest,
  CreateFieldRequest,
  FieldResponse,
} from '../models/farm.model';

@Injectable({ providedIn: 'root' })
export class FarmsService {
  private readonly base = `${environment.api.farms}/api`;

  constructor(private http: HttpClient) {}

  getAllWithFields(): Observable<FarmWithFieldsResponse[]> {
    return this.http.get<FarmWithFieldsResponse[]>(`${this.base}/farms/all-with-fields`);
  }

  getAll(): Observable<FarmResponse[]> {
    return this.http.get<FarmResponse[]>(`${this.base}/farms`);
  }

  createFarm(body: CreateFarmRequest): Observable<FarmResponse> {
    return this.http.post<FarmResponse>(`${this.base}/farms`, body);
  }

  getFields(farmId?: string): Observable<FieldResponse[]> {
    let params = new HttpParams();
    if (farmId) params = params.set('farmId', farmId);
    return this.http.get<FieldResponse[]>(`${this.base}/fields`, { params });
  }

  createField(farmId: string, body: CreateFieldRequest): Observable<FieldResponse> {
    return this.http.post<FieldResponse>(`${this.base}/farms/${farmId}/fields`, body);
  }
}
