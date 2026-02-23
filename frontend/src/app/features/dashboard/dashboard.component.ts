import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FarmsService } from '../../core/services/farms.service';
import { ReadingsService } from '../../core/services/readings.service';
import { AlertsService } from '../../core/services/alerts.service';
import type { FarmWithFieldsResponse, FieldResponse } from '../../core/models/farm.model';
import type { ReadingResponse } from '../../core/models/reading.model';
import type { AlertResponse } from '../../core/models/alert.model';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { Observable, EMPTY, forkJoin, tap, finalize, catchError, of } from 'rxjs';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('readingsChart') chartCanvas: ElementRef<HTMLCanvasElement> | null = null;

  farmsWithFields: FarmWithFieldsResponse[] = [];
  selectedFieldId = '';
  fields: FieldResponse[] = [];
  readings: ReadingResponse[] = [];
  alerts: AlertResponse[] = [];
  loadingReadings = false;
  loadingAlerts = false;
  error = '';
  isSimulating = false;
  private chart: Chart | null = null;

  constructor(
    private farms: FarmsService,
    private readingsService: ReadingsService,
    private alertsService: AlertsService,
  ) {}

  ngOnInit(): void {
    this.farms.getAllWithFields().subscribe({
      next: (list) => {
        this.farmsWithFields = list;
        const allFields = list.flatMap((f) => f.fields.map((fd) => ({ ...fd, farmName: f.name })));
        this.fields = allFields;
        if (this.fields.length && !this.selectedFieldId) {
          this.selectedFieldId = this.fields[0].id;
          this.loadData().subscribe();
        }
      },
      error: () => (this.error = 'Erro ao carregar propriedades.'),
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  ngAfterViewInit(): void {
    // Chart is built when data loads
  }

  onFieldChange(): void {
    this.loadData().subscribe();
  }

  simulateData(): void {
    if (!this.selectedFieldId) return;
    this.isSimulating = true;
    this.readingsService.simulateBurst(this.selectedFieldId).subscribe({
      next: () => {
        setTimeout(() => {
          this.loadData().subscribe({
            next: () => {},
            error: () => (this.isSimulating = false),
            complete: () => (this.isSimulating = false),
          });
        }, 3000);
      },
      error: () => (this.isSimulating = false),
    });
  }

  loadData(): Observable<{ readings: ReadingResponse[]; alerts: AlertResponse[] }> {
    if (!this.selectedFieldId) return EMPTY;
    this.loadingReadings = true;
    this.loadingAlerts = true;
    const readings$ = this.readingsService.getByField(this.selectedFieldId).pipe(
      tap((data) => {
        this.readings = data;
        setTimeout(() => {
          this.buildChart();
        }, 100);
      }),
      catchError(() => {
        this.readings = [];
        return of([]);
      }),
      finalize(() => (this.loadingReadings = false)),
    );
    const alerts$ = this.alertsService.getByField(this.selectedFieldId).pipe(
      tap((data) => {
        this.alerts = (data || []).sort(
          (a, b) => new Date(b.triggeredAtUtc).getTime() - new Date(a.triggeredAtUtc).getTime(),
        );
      }),
      catchError(() => {
        this.alerts = [];
        return of([]);
      }),
      finalize(() => (this.loadingAlerts = false)),
    );
    return forkJoin({ readings: readings$, alerts: alerts$ });
  }

  private buildChart(): void {
    this.chart?.destroy();
    this.chart = null;
    if (!this.chartCanvas?.nativeElement || !this.readings.length) return;

    const labels = this.readings
      .slice()
      .reverse()
      .map((r) => new Date(r.measuredAtUtc).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' }));
    const moisture = this.readings.slice().reverse().map((r) => r.soilMoisturePercent);
    const temp = this.readings.slice().reverse().map((r) => r.temperatureC);
    const rain = this.readings.slice().reverse().map((r) => r.rainMm);

    const config: ChartConfiguration = {
      type: 'line',
      data: {
        labels,
        datasets: [
          { label: 'Umidade solo (%)', data: moisture, borderColor: '#0d6efd', tension: 0.2, fill: false },
          { label: 'Temperatura (°C)', data: temp, borderColor: '#dc3545', tension: 0.2, fill: false },
          { label: 'Chuva (mm)', data: rain, borderColor: '#198754', tension: 0.2, fill: false },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: { y: { beginAtZero: true } },
      },
    };

    this.chart = new Chart(this.chartCanvas.nativeElement, config);
  }

  getFieldName(id: string): string {
    const f = this.fields.find((x) => x.id === id);
    return f ? f.name : id;
  }

  severityClass(severity: string): string {
    const s = (severity || '').toLowerCase();
    if (s === 'critical') return 'danger';
    if (s === 'warning') return 'warning';
    return 'info';
  }
}
