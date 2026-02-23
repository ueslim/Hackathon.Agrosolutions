import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ReadingResponse } from '../models/reading.model';

@Injectable({ providedIn: 'root' })
export class ReadingsService {
  private readonly base = `${environment.api.sensorIngestion}/api/readings`;

  constructor(private http: HttpClient) {}

  getByField(
    fieldId: string,
    fromUtc?: string,
    toUtc?: string,
    take = 200,
  ): Observable<ReadingResponse[]> {
    let params = new HttpParams().set('fieldId', fieldId).set('take', take);
    if (fromUtc) params = params.set('fromUtc', fromUtc);
    if (toUtc) params = params.set('toUtc', toUtc);
    return this.http.get<ReadingResponse[]>(this.base, { params });
  }
}
