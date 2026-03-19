import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';

@Component({
  selector: 'app-config-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatChipsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="config-card">
      <mat-card-content>
        <div class="header">
          <h2 class="title">{{ config.name }}</h2>
          <button mat-icon-button class="menu-btn" (click)="$event.stopPropagation()">
            <mat-icon>more_vert</mat-icon>
          </button>
        </div>

        <div class="stats">
          <div class="stat-item">
            <mat-icon>storage</mat-icon>
            <div class="stat-content">
              <p class="stat-value">{{ getResourcesCount() }}</p>
              <p class="stat-label">Resources</p>
            </div>
          </div>

          <div class="stat-item">
            <mat-icon>cloud_queue</mat-icon>
            <div class="stat-content">
              <p class="stat-value">{{ config.environmentDefinitions?.length || 0 }}</p>
              <p class="stat-label">Environments</p>
            </div>
          </div>

          <div class="stat-item">
            <mat-icon>people</mat-icon>
            <div class="stat-content">
              <p class="stat-value">{{ config.members?.length || 0 }}</p>
              <p class="stat-label">Members</p>
            </div>
          </div>
        </div>

        <div class="members" *ngIf="config.members && config.members.length > 0">
          <p class="members-label">Team</p>
          <div class="member-avatars">
            <div
              class="avatar"
              *ngFor="let member of config.members | slice : 0 : 3"
              [title]="member.id"
            >
              {{ member.id.charAt(0).toUpperCase() }}
            </div>
            <span class="more" *ngIf="config.members.length > 3">
              +{{ config.members.length - 3 }}
            </span>
          </div>
        </div>

        <div class="resources" *ngIf="config.environmentDefinitions && config.environmentDefinitions.length > 0">
          <p class="resources-label">Environments</p>
          <div class="env-tags">
            <mat-chip
              *ngFor="let env of config.environmentDefinitions | slice : 0 : 2"
              variant="outlined"
              class="env-chip"
            >
              {{ env.name }}
            </mat-chip>
            <span class="more-envs" *ngIf="config.environmentDefinitions.length > 2">
              +{{ config.environmentDefinitions.length - 2 }}
            </span>
          </div>
        </div>
      </mat-card-content>

      <mat-card-actions>
        <button mat-button class="view-btn" (click)="$event.stopPropagation()">
          View Details
          <mat-icon>arrow_forward</mat-icon>
        </button>
      </mat-card-actions>
    </mat-card>
  `,
  styles: `
    .config-card {
      height: 100%;
      display: flex;
      flex-direction: column;
      background: #fff;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      border-radius: 8px;
      transition: box-shadow 0.2s;

      &:hover {
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
      }
    }

    mat-card-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    mat-card-actions {
      display: flex;
      justify-content: flex-start;
      padding: 0 16px 16px;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 12px;

      .title {
        margin: 0;
        font-size: 18px;
        font-weight: 500;
        flex: 1;
      }

      .menu-btn {
        width: 32px;
        height: 32px;
        flex-shrink: 0;
      }
    }

    .stats {
      display: flex;
      gap: 12px;
      padding: 12px;
      background: #f5f5f5;
      border-radius: 6px;
    }

    .stat-item {
      display: flex;
      align-items: center;
      gap: 8px;
      flex: 1;

      mat-icon {
        color: #1976d2;
        font-size: 20px;
        width: 20px;
        height: 20px;
      }
    }

    .stat-content {
      display: flex;
      flex-direction: column;
      gap: 0;
    }

    .stat-value {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #333;
    }

    .stat-label {
      margin: 0;
      font-size: 11px;
      color: #999;
      text-transform: uppercase;
    }

    .members {
      .members-label {
        margin: 0 0 8px;
        font-size: 12px;
        color: #999;
        text-transform: uppercase;
      }

      .member-avatars {
        display: flex;
        align-items: center;
        gap: 4px;
      }
    }

    .avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: #1976d2;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 12px;
      font-weight: 600;
    }

    .more {
      margin-left: 4px;
      font-size: 12px;
      color: #666;
    }

    .resources {
      .resources-label {
        margin: 0 0 8px;
        font-size: 12px;
        color: #999;
        text-transform: uppercase;
      }

      .env-tags {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        align-items: center;
      }
    }

    .env-chip {
      font-size: 12px !important;
      height: 24px !important;
    }

    .more-envs {
      font-size: 12px;
      color: #666;
    }

    .view-btn {
      width: 100%;
      justify-content: space-between;
      color: #1976d2;

      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
        margin-left: 4px;
      }
    }
  `,
})
export class ConfigCardComponent {
  @Input() config!: InfrastructureConfigResponse;
  @Output() click = new EventEmitter<void>();

  getResourcesCount(): number {
    return this.config.environmentDefinitions?.reduce((sum, env) => {
      return sum + (env.tags?.length || 0);
    }, 0) || 0;
  }
}
