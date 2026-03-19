import { Component, inject, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { InfraConfigService } from '../../../../shared/services/infra-config.service';
import { ResourceGroupService } from '../../../../shared/services/resource-group.service';
import {
  LoadingSpinnerComponent,
  EmptyStateComponent,
  AvatarComponent,
  DataTableComponent,
  TableColumn,
} from '../../../../shared/ui-library';

@Component({
  selector: 'app-config-details',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    AvatarComponent,
    DataTableComponent,
  ],
  templateUrl: './config-details.component.html',
  styleUrl: './config-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
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

  resourceGroupColumns: TableColumn[] = [
    { key: 'name', label: 'Name', width: '40%' },
    { key: 'location', label: 'Location', width: '30%' },
    { key: 'id', label: 'ID', width: '30%' },
  ];

  memberColumns: TableColumn[] = [
    { key: 'userId', label: 'User', width: '40%' },
    { key: 'role', label: 'Role', width: '30%' },
    { key: 'id', label: 'ID', width: '30%' },
  ];

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

  getMemberInitial(userId: string): string {
    return userId.charAt(0).toUpperCase();
  }

  getRoleVariant(role: string): 'primary' | 'secondary' | 'accent' {
    switch (role.toLowerCase()) {
      case 'owner':
        return 'accent';
      case 'contributor':
        return 'primary';
      default:
        return 'secondary';
    }
  }
}
