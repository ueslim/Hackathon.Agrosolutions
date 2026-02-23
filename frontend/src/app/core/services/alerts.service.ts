import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { AlertResponse } from '../models/alert.model';

@Injectable({ providedIn: 'root' })
export class AlertsService {
  private readonly base = `${environment.api.alerts}/api/alerts`;

  constructor(private http: HttpClient) {}

  getByField(fieldId: string): Observable<AlertResponse[]> {
    return this.http.get<AlertResponse[]>(this.base, {
      params: new HttpParams().set('fieldId', fieldId),
    });
  }
}
