import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse } from '../../../shared/interfaces/resource-group.interface';

@Component({
  selector: 'app-infrastructure-config-detail',
  standalone: true,
  imports: [MatTabsModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './infrastructure-config-detail.component.html',
})
export class InfrastructureConfigDetailComponent implements OnInit {
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected config = signal<InfrastructureConfigResponse | null>(null);
  protected resourceGroups = signal<ResourceGroupResponse[]>([]);
  protected isLoading = signal(true);
  protected error = signal<string | null>(null);

  private configId = '';

  ngOnInit(): void {
    this.configId = this.route.snapshot.paramMap.get('id') ?? '';
    if (!this.configId) {
      void this.router.navigate(['/infrastructure-configs']);
      return;
    }
    void this.loadData();
  }

  private async loadData(): Promise<void> {
    try {
      this.isLoading.set(true);
      this.error.set(null);
      const [config, resourceGroups] = await Promise.all([
        this.infraConfigService.getById(this.configId),
        this.infraConfigService.getResourceGroups(this.configId),
      ]);
      this.config.set(config);
      this.resourceGroups.set(resourceGroups);
    } catch {
      this.error.set('Failed to load configuration.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected goBack(): void {
    void this.router.navigate(['/infrastructure-configs']);
  }

  protected removeMember(userId: string): void {
    void this.doRemoveMember(userId);
  }

  private async doRemoveMember(userId: string): Promise<void> {
    try {
      await this.infraConfigService.removeMember(this.configId, userId);
      await this.loadData();
    } catch {
      // silently handle error
    }
  }

  protected removeEnvironment(envId: string): void {
    void this.doRemoveEnvironment(envId);
  }

  private async doRemoveEnvironment(envId: string): Promise<void> {
    try {
      await this.infraConfigService.removeEnvironment(this.configId, envId);
      await this.loadData();
    } catch {
      // silently handle error
    }
  }
}
