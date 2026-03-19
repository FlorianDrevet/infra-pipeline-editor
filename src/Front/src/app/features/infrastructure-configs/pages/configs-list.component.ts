import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ConfigCardComponent } from '../components/config-card.component';

@Component({
  selector: 'app-configs-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    ConfigCardComponent,
  ],
  template: `
    <div class="configs-list-container">
      <div class="header">
        <h1>Infrastructure Configurations</h1>
        <button mat-raised-button color="primary" class="new-config-btn">
          <mat-icon>add</mat-icon>
          New Configuration
        </button>
      </div>

      <div class="loading" *ngIf="isLoading()">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Loading configurations...</p>
      </div>

      <div class="error" *ngIf="error()">
        <mat-icon>error_outline</mat-icon>
        <p>{{ error() }}</p>
      </div>

      <div class="cards-grid" *ngIf="!isLoading() && !error()">
        <app-config-card
          *ngFor="let config of configurations()"
          [config]="config"
          (click)="openConfig(config.id)"
          class="card-item"
        ></app-config-card>

        <div class="empty-state" *ngIf="configurations().length === 0">
          <mat-icon>devices_other</mat-icon>
          <p>No configurations yet. Create one to get started.</p>
        </div>
      </div>
    </div>
  `,
  styles: `
    .configs-list-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;

      h1 {
        margin: 0;
        font-size: 28px;
        font-weight: 500;
      }
    }

    .new-config-btn {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 16px;
      min-height: 400px;
      color: #666;

      p {
        margin: 0;
        font-size: 14px;
      }
    }

    .error {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 24px;
      background: #ffebee;
      border-left: 4px solid #f44336;
      border-radius: 4px;
      color: #c62828;

      mat-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;
      }

      p {
        margin: 0;
      }
    }

    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 24px;
    }

    .card-item {
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;

      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 8px 16px rgba(0, 0, 0, 0.15);
      }
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 16px;
      padding: 60px 24px;
      color: #999;
      grid-column: 1 / -1;

      mat-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        opacity: 0.5;
      }

      p {
        margin: 0;
        font-size: 16px;
      }
    }
  `,
})
export class ConfigsListComponent implements OnInit {
  private readonly infra = inject(InfraConfigService);
  private readonly router = inject(Router);

  configurations = this.infra.configurations;
  isLoading = this.infra.isLoading;
  error = this.infra.error;

  ngOnInit(): void {
    this.infra.loadConfigurations();
  }

  openConfig(configId: string): void {
    this.router.navigate(['/configs', configId]);
  }
}
