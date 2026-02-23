import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FarmsService } from '../../core/services/farms.service';
import type { FarmWithFieldsResponse, CreateFarmRequest, CreateFieldRequest } from '../../core/models/farm.model';

@Component({
  selector: 'app-farms',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './farms.component.html',
  styleUrl: './farms.component.css',
})
export class FarmsComponent implements OnInit {
  farms: FarmWithFieldsResponse[] = [];
  loading = true;
  error = '';
  showNewFarm = false;
  newFarm: CreateFarmRequest = { name: '', locationDescription: '' };
  showNewField: string | null = null;
  newField: CreateFieldRequest = { name: '', crop: '', boundaryDescription: '' };

  constructor(private farmsService: FarmsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.farmsService.getAllWithFields().subscribe({
      next: (list) => {
        this.farms = list;
        this.loading = false;
      },
      error: () => {
        this.error = 'Erro ao carregar propriedades.';
        this.loading = false;
      },
    });
  }

  openNewFarm(): void {
    this.showNewFarm = true;
    this.newFarm = { name: '', locationDescription: '' };
  }

  cancelNewFarm(): void {
    this.showNewFarm = false;
  }

  submitNewFarm(): void {
    if (!this.newFarm.name.trim()) return;
    this.farmsService.createFarm(this.newFarm).subscribe({
      next: () => {
        this.showNewFarm = false;
        this.load();
      },
      error: () => (this.error = 'Erro ao criar propriedade.'),
    });
  }

  openNewField(farmId: string): void {
    this.showNewField = farmId;
    this.newField = { name: '', crop: '', boundaryDescription: '' };
  }

  cancelNewField(): void {
    this.showNewField = null;
  }

  submitNewField(farmId: string): void {
    if (!this.newField.name.trim() || !this.newField.crop.trim()) return;
    this.farmsService.createField(farmId, this.newField).subscribe({
      next: () => {
        this.showNewField = null;
        this.load();
      },
      error: () => (this.error = 'Erro ao criar talhão.'),
    });
  }
}
