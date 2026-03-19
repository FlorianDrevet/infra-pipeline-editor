import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { ResourceGroupService } from '../../../shared/services/resource-group.service';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse } from '../../../shared/interfaces/resource-group.interface';

@Component({
  selector: 'app-config-details',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatChipsModule,
    MatDividerModule,
  ],
  template: `
    <div class="details-container">
      <button mat-icon-button class="back-btn" (click)="goBack()" matTooltip="Back">
        <mat-icon>arrow_back</mat-icon>
      </button>

      <div *ngIf="isLoading()" class="loading">
        <mat-spinner diameter="50"></mat-spinner>
      </div>

      <div *ngIf="!isLoading() && currentConfig()" class="config-details">
        <div class="header-bar">
          <div>
            <h1>{{ currentConfig()!.name }}</h1>
            <p class="subtitle">Configuration ID: {{ currentConfig()!.id }}</p>
          </div>
          <div class="actions">
            <button mat-stroked-button>Edit</button>
            <button mat-stroked-button color="warn">Delete</button>
          </div>
        </div>

        <mat-tab-group class="tabs">
          <!-- Resources Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>storage</mat-icon>
              <span>Resources</span>
            </ng-template>

            <div class="tab-content">
              <div class="resource-groups">
                <h2>Resource Groups</h2>
                <div class="table-container">
                  <table mat-table [dataSource]="resourceGroups()" class="resources-table">
                    <!-- Name Column -->
                    <ng-container matColumnDef="name">
                      <th mat-header-cell *matHeaderCellDef>Name</th>
                      <td mat-cell *matCellDef="let element">{{ element.name }}</td>
                    </ng-container>

                    <!-- Location Column -->
                    <ng-container matColumnDef="location">
                      <th mat-header-cell *matHeaderCellDef>Location</th>
                      <td mat-cell *matCellDef="let element">{{ element.location }}</td>
                    </ng-container>

                    <!-- Resources Count Column -->
                    <ng-container matColumnDef="resources">
                      <th mat-header-cell *matHeaderCellDef>Resources</th>
                      <td mat-cell *matCellDef="let element">
                        <mat-chip variant="outlined" disabled>0 resources</mat-chip>
                      </td>
                    </ng-container>

                    <!-- Actions Column -->
                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let element">
                        <button mat-icon-button matTooltip="View">
                          <mat-icon>visibility</mat-icon>
                        </button>
                        <button mat-icon-button matTooltip="Edit">
                          <mat-icon>edit</mat-icon>
                        </button>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
                  </table>

                  <div class="empty-state" *ngIf="resourceGroups().length === 0">
                    <mat-icon>folder_open</mat-icon>
                    <p>No resource groups configured yet</p>
                  </div>
                </div>
              </div>
            </div>
          </mat-tab>

          <!-- Environments Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>cloud_queue</mat-icon>
              <span>Environments</span>
            </ng-template>

            <div class="tab-content">
              <div class="environments">
                <h2>Environment Definitions</h2>

                <div class="env-list">
                  <div
                    class="env-card"
                    *ngFor="let env of currentConfig()!.environmentDefinitions"
                  >
                    <div class="env-header">
                      <h3>{{ env.name }}</h3>
                      <mat-chip variant="filled" color="accent">
                        {{ env.order }}
                      </mat-chip>
                    </div>

                    <div class="env-details">
                      <div class="detail-row">
                        <span class="label">Location:</span>
                        <span class="value">{{ env.location }}</span>
                      </div>
                      <div class="detail-row">
                        <span class="label">Prefix:</span>
                        <span class="value">{{ env.prefix }}</span>
                      </div>
                      <div class="detail-row">
                        <span class="label">Suffix:</span>
                        <span class="value">{{ env.suffix }}</span>
                      </div>
                      <div class="detail-row">
                        <span class="label">Requires Approval:</span>
                        <span class="value">
                          <mat-icon class="status-icon" [class.approved]="env.requiresApproval">
                            {{ env.requiresApproval ? 'check_circle' : 'cancel' }}
                          </mat-icon>
                        </span>
                      </div>
                    </div>

                    <div class="tags" *ngIf="env.tags && env.tags.length > 0">
                      <p class="tags-label">Tags</p>
                      <mat-chip *ngFor="let tag of env.tags" variant="outlined">
                        {{ tag.name }} = {{ tag.value }}
                      </mat-chip>
                    </div>
                  </div>

                  <div class="empty-state" *ngIf="currentConfig()!.environmentDefinitions.length === 0">
                    <mat-icon>cloud_off</mat-icon>
                    <p>No environments defined yet</p>
                  </div>
                </div>
              </div>
            </div>
          </mat-tab>

          <!-- Members Tab -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>people</mat-icon>
              <span>Members</span>
            </ng-template>

            <div class="tab-content">
              <div class="members">
                <h2>Team Members</h2>

                <div class="table-container">
                  <table mat-table [dataSource]="currentConfig()!.members || []" class="members-table">
                    <!-- User Column -->
                    <ng-container matColumnDef="user">
                      <th mat-header-cell *matHeaderCellDef>User</th>
                      <td mat-cell *matCellDef="let element">
                        <div class="user-cell">
                          <div class="avatar">
                            {{ element.id.charAt(0).toUpperCase() }}
                          </div>
                          <div class="user-info">
                            <p class="user-name">{{ element.userId }}</p>
                            <p class="user-id">{{ element.id }}</p>
                          </div>
                        </div>
                      </td>
                    </ng-container>

                    <!-- Role Column -->
                    <ng-container matColumnDef="role">
                      <th mat-header-cell *matHeaderCellDef>Role</th>
                      <td mat-cell *matCellDef="let element">
                        <mat-chip [class]="'role-' + element.role.toLowerCase()" variant="filled">
                          {{ element.role }}
                        </mat-chip>
                      </td>
                    </ng-container>

                    <!-- Actions Column -->
                    <ng-container matColumnDef="actions">
                      <th mat-header-cell *matHeaderCellDef>Actions</th>
                      <td mat-cell *matCellDef="let element">
                        <button mat-icon-button matTooltip="Change Role">
                          <mat-icon>edit</mat-icon>
                        </button>
                        <button mat-icon-button matTooltip="Remove" color="warn">
                          <mat-icon>delete</mat-icon>
                        </button>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="memberColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: memberColumns;"></tr>
                  </table>

                  <div class="empty-state" *ngIf="!currentConfig()!.members || currentConfig()!.members.length === 0">
                    <mat-icon>person_off</mat-icon>
                    <p>No members in this configuration</p>
                  </div>
                </div>

                <div class="add-member-section">
                  <button mat-raised-button color="primary">
                    <mat-icon>person_add</mat-icon>
                    Add Member
                  </button>
                </div>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </div>

      <div *ngIf="!isLoading() && !currentConfig()" class="not-found">
        <mat-icon>error_outline</mat-icon>
        <p>Configuration not found</p>
        <button mat-raised-button color="primary" (click)="goBack()">Go Back</button>
      </div>
    </div>
  `,
  styles: `
    .details-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .back-btn {
      margin-bottom: 16px;
    }

    .loading {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 400px;
    }

    .config-details {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      overflow: hidden;
    }

    .header-bar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 24px;
      border-bottom: 1px solid #e0e0e0;

      h1 {
        margin: 0 0 8px;
        font-size: 24px;
      }

      .subtitle {
        margin: 0;
        font-size: 12px;
        color: #999;
      }

      .actions {
        display: flex;
        gap: 8px;
      }
    }

    .tabs {
      ::ng-deep {
        .mdc-tab__text-label {
          display: flex;
          align-items: center;
          gap: 8px;
        }
      }
    }

    .tab-content {
      padding: 24px;
      min-height: 400px;
    }

    .table-container {
      overflow-x: auto;
    }

    .resources-table,
    .members-table {
      width: 100%;
      border-collapse: collapse;

      th {
        background: #f5f5f5;
        font-weight: 600;
        padding: 12px;
      }

      td {
        padding: 12px;
        border-bottom: 1px solid #e0e0e0;
      }
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 48px 24px;
      color: #999;

      mat-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        opacity: 0.5;
      }

      p {
        margin: 0;
      }
    }

    .resource-groups {
      h2 {
        margin: 0 0 16px;
        font-size: 18px;
      }
    }

    .environments {
      h2 {
        margin: 0 0 16px;
        font-size: 18px;
      }

      .env-list {
        display: grid;
        gap: 16px;
      }
    }

    .env-card {
      border: 1px solid #e0e0e0;
      border-radius: 6px;
      padding: 16px;
      background: #fafafa;

      .env-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;

        h3 {
          margin: 0;
          font-size: 16px;
        }
      }

      .env-details {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 12px;
        margin-bottom: 16px;
      }

      .detail-row {
        display: flex;
        justify-content: space-between;
        align-items: center;

        .label {
          font-weight: 600;
          font-size: 12px;
          color: #666;
        }

        .value {
          font-size: 14px;
          color: #333;
        }
      }

      .tags {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        align-items: center;

        .tags-label {
          margin: 0;
          font-weight: 600;
          font-size: 12px;
          color: #666;
          width: 100%;
        }
      }
    }

    .members {
      h2 {
        margin: 0 0 16px;
        font-size: 18px;
      }
    }

    .user-cell {
      display: flex;
      align-items: center;
      gap: 12px;

      .avatar {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background: #1976d2;
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 600;
      }

      .user-info {
        .user-name {
          margin: 0;
          font-weight: 600;
          font-size: 14px;
        }

        .user-id {
          margin: 0;
          font-size: 12px;
          color: #999;
        }
      }
    }

    .role-owner {
      background: #fff3e0 !important;
      color: #e65100 !important;
    }

    .role-contributor {
      background: #e3f2fd !important;
      color: #1565c0 !important;
    }

    .role-reader {
      background: #f3e5f5 !important;
      color: #6a1b9a !important;
    }

    .status-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;

      &.approved {
        color: #4caf50;
      }

      &:not(.approved) {
        color: #f44336;
      }
    }

    .add-member-section {
      display: flex;
      justify-content: center;
      padding: 24px 0 0;
      border-top: 1px solid #e0e0e0;
      margin-top: 24px;
    }

    .not-found {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 16px;
      padding: 60px 24px;
      background: white;
      border-radius: 8px;

      mat-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        opacity: 0.5;
      }

      p {
        margin: 0;
        font-size: 16px;
        color: #999;
      }
    }
  `,
})
export class ConfigDetailsComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly infraService = inject(InfraConfigService);
  private readonly resourceService = inject(ResourceGroupService);
  private readonly destroy$ = new Subject<void>();

  currentConfig = this.infraService.currentConfig;
  isLoading = this.infraService.isLoadingDetails;
  resourceGroups = this.resourceService.resourceGroups;

  displayedColumns = ['name', 'location', 'resources', 'actions'];
  memberColumns = ['user', 'role', 'actions'];

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe((params) => {
        if (params['id']) {
          this.infraService.loadConfigDetails(params['id']);
          this.resourceService.loadResourceGroups(params['id']);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  goBack(): void {
    this.router.navigate(['/configs']);
  }
}
