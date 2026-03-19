import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { InfraConfigService } from '../../../../shared/services/infra-config.service';
import {
  LoadingSpinnerComponent,
  EmptyStateComponent,
} from '../../../../shared/ui-library';
import { ConfigCardComponent } from '../../components/config-card/config-card.component';

@Component({
  selector: 'app-configs-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    ConfigCardComponent,
  ],
  templateUrl: './configs-list.component.html',
  styleUrl: './configs-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigsListComponent implements OnInit {
  private readonly infraService = inject(InfraConfigService);
  private readonly router = inject(Router);

  configurations = this.infraService.configurations;
  isLoading = this.infraService.isLoading;
  error = this.infraService.error;

  ngOnInit(): void {
    this.infraService.loadConfigurations();
  }

  openConfig(configId: string): void {
    this.router.navigate(['/configs', configId]);
  }

  createNewConfig(): void {
    // TODO: implement create config dialog
  }
}
