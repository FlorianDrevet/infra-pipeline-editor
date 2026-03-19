import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { CreateConfigDialogComponent } from '../create-config-dialog/create-config-dialog.component';

@Component({
  selector: 'app-infrastructure-configs-list',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule],
  templateUrl: './infrastructure-configs-list.component.html',
})
export class InfrastructureConfigsListComponent implements OnInit {
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  protected configs = signal<InfrastructureConfigResponse[]>([]);
  protected isLoading = signal(true);
  protected error = signal<string | null>(null);

  ngOnInit(): void {
    void this.loadConfigs();
  }

  private async loadConfigs(): Promise<void> {
    try {
      this.isLoading.set(true);
      this.error.set(null);
      const configs = await this.infraConfigService.getAll();
      this.configs.set(configs);
    } catch {
      this.error.set('Failed to load configurations.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openNewConfigDialog(): void {
    const ref = this.dialog.open(CreateConfigDialogComponent, { width: '400px' });
    ref.afterClosed().subscribe((result) => {
      if (result) {
        void this.loadConfigs();
      }
    });
  }

  protected viewDetail(id: string): void {
    void this.router.navigate(['/infrastructure-configs', id]);
  }
}
