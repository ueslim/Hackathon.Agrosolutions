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
          this.loadData();
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
    this.loadData();
  }

  loadData(): void {
    if (!this.selectedFieldId) return;
    this.loadingReadings = true;
    this.loadingAlerts = true;
    this.readingsService.getByField(this.selectedFieldId).subscribe({
      next: (data) => {
        this.readings = data;
        this.loadingReadings = false;
        this.buildChart();
      },
      error: () => {
        this.readings = [];
        this.loadingReadings = false;
      },
    });
    this.alertsService.getByField(this.selectedFieldId).subscribe({
      next: (data) => {
        this.alerts = (data || []).sort(
          (a, b) => new Date(b.triggeredAtUtc).getTime() - new Date(a.triggeredAtUtc).getTime(),
        );
        this.loadingAlerts = false;
      },
      error: () => {
        this.alerts = [];
        this.loadingAlerts = false;
      },
    });
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
