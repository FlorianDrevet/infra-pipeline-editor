import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PrivateEndpointService } from '../../../../shared/services/private-endpoint.service';
import { PrivateEndpointConfigResponse, AddPrivateEndpointRequest } from '../../../../shared/interfaces/private-endpoint.interface';
import { DsToggleComponent } from '../../../../shared/components/ds';

/**
 * Resource types that support private endpoints.
 */
const PE_SUPPORTED_TYPES = new Set<string>([
  'KeyVault',
  'StorageAccount',
  'AppConfiguration',
  'CosmosDb',
  'SqlServer',
  'RedisCache',
  'ServiceBusNamespace',
  'EventHubNamespace',
  'ContainerRegistry',
  'WebApp',
  'FunctionApp',
  'ApplicationInsights',
  'LogAnalyticsWorkspace',
]);

@Component({
  selector: 'app-networking-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule,
    DsToggleComponent,
  ],
  templateUrl: './networking-tab.component.html',
  styleUrl: './networking-tab.component.scss',
})
export class NetworkingTabComponent {
  private readonly peService = inject(PrivateEndpointService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  /** Resource ID to load private endpoints for. */
  readonly resourceId = input.required<string>();

  /** Resource type discriminator. */
  readonly resourceType = input.required<string>();

  // ─── State ───

  protected readonly endpoints = signal<PrivateEndpointConfigResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly showAddForm = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly pendingDeleteId = signal<string | null>(null);

  // ─── Add form fields ───

  protected subnetId = '';
  protected groupId = '';
  protected autoApproval = false;
  protected privateDnsZoneId = '';
  protected customNicName = '';

  // ─── Computed ───

  protected readonly supportsPe = computed(() => PE_SUPPORTED_TYPES.has(this.resourceType()));

  constructor() {
    effect(() => {
      const id = this.resourceId();
      if (id && this.supportsPe()) {
        this.loadEndpoints(id);
      }
    });
  }

  // ─── Actions ───

  protected async loadEndpoints(resourceId: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');
    try {
      const data = await this.peService.getByResourceId(resourceId);
      this.endpoints.set(data);
    } catch {
      this.loadError.set('RESOURCE_EDIT.NETWORKING.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected toggleAddForm(): void {
    this.showAddForm.update(v => !v);
    if (!this.showAddForm()) {
      this.resetForm();
    }
  }

  protected async submitAdd(): Promise<void> {
    if (!this.subnetId || !this.groupId) return;

    const request: AddPrivateEndpointRequest = {
      subnetId: this.subnetId,
      groupId: this.groupId,
      autoApproval: this.autoApproval,
      privateDnsZoneId: this.privateDnsZoneId || undefined,
      customNetworkInterfaceName: this.customNicName || undefined,
    };

    this.isSaving.set(true);
    try {
      const created = await this.peService.add(this.resourceId(), request);
      this.endpoints.update(list => [...list, created]);
      this.showAddForm.set(false);
      this.resetForm();
    } catch {
      // Error is visible via failed request; keep form open
    } finally {
      this.isSaving.set(false);
    }
  }

  protected async removeEndpoint(peId: string): Promise<void> {
    this.pendingDeleteId.set(peId);
    try {
      await this.peService.remove(this.resourceId(), peId);
      this.endpoints.update(list => list.filter(e => e.id !== peId));
    } catch {
      this.snackBar.open(
        this.translate.instant('RESOURCE_EDIT.NETWORKING.REMOVE_ERROR') as string,
        '✕',
        { duration: 5000, panelClass: 'error-snackbar' }
      );
    } finally {
      this.pendingDeleteId.set(null);
    }
  }

  private resetForm(): void {
    this.subnetId = '';
    this.groupId = '';
    this.autoApproval = false;
    this.privateDnsZoneId = '';
    this.customNicName = '';
  }
}
