import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsBannerComponent } from '../../../../shared/components/ds/ds-banner/ds-banner.component';
import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { DsIconButtonComponent } from '../../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { DsSelectComponent, DsSelectOption } from '../../../../shared/components/ds/ds-select/ds-select.component';
import { DsTextFieldComponent } from '../../../../shared/components/ds/ds-text-field/ds-text-field.component';
import { DsToggleComponent } from '../../../../shared/components/ds/ds-toggle/ds-toggle.component';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { ResourceEditNetworkingSection } from './resource-edit-networking-section.interface';
import { DnsHelpDialogComponent } from './dns-help-dialog/dns-help-dialog.component';

@Component({
  selector: 'app-resource-edit-networking-section',
  standalone: true,
  imports: [
    FormsModule,
    DsBannerComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
    DsToggleComponent,
    DsSpinnerComponent,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './resource-edit-networking-section.component.html',
  styleUrl: './resource-edit-networking-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditNetworkingSectionComponent {
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);

  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ResourceEditNetworkingSection>();

  protected readonly dnsModeOptions = computed<DsSelectOption[]>(() => [
    { value: 'AutoManaged', label: this.translate.instant('RESOURCE_EDIT.NETWORKING.DNS_MODE_AUTO_MANAGED') },
    { value: 'ExistingHub', label: this.translate.instant('RESOURCE_EDIT.NETWORKING.DNS_MODE_EXISTING_HUB') },
    { value: 'Disabled', label: this.translate.instant('RESOURCE_EDIT.NETWORKING.DNS_MODE_DISABLED') },
  ]);

  // ─── Local form state ───
  protected readonly selectedVnetId = signal<string | null>(null);
  protected readonly subnetName = signal('');
  protected readonly selectedDnsMode = signal<string>('AutoManaged');
  protected readonly dnsHubResourceGroupId = signal('');
  protected readonly dnsHubSubscriptionId = signal('');

  constructor() {
    // Load subnets whenever selected VNet changes
    effect(() => {
      const vnetId = this.selectedVnetId();
      if (vnetId) {
        void this.section().loadSubnetsForVnet(vnetId);
      }
    });
  }

  protected get showHubFields(): boolean {
    return this.selectedDnsMode() === 'ExistingHub';
  }

  protected get hasExistingConfig(): boolean {
    return this.section().privateEndpointConfig() !== null;
  }

  protected async onToggleChange(value: boolean): Promise<void> {
    await this.section().togglePrivatization(value);
  }

  protected initFormFromConfig(): void {
    const config = this.section().privateEndpointConfig();
    if (config) {
      this.selectedVnetId.set(config.virtualNetworkId);
      this.subnetName.set(config.subnetName);
      this.selectedDnsMode.set(config.dnsMode);
      this.dnsHubResourceGroupId.set(config.dnsHubResourceGroupId ?? '');
      this.dnsHubSubscriptionId.set(config.dnsHubSubscriptionId ?? '');
    } else {
      this.selectedVnetId.set(null);
      this.subnetName.set('');
      this.selectedDnsMode.set('AutoManaged');
      this.dnsHubResourceGroupId.set('');
      this.dnsHubSubscriptionId.set('');
    }
  }

  protected onVnetChange(vnetId: string): void {
    this.selectedVnetId.set(vnetId);
    this.subnetName.set('');
  }

  protected async onSave(): Promise<void> {
    const vnetId = this.selectedVnetId();
    const subnet = this.subnetName();
    if (!vnetId || !subnet) {
      return;
    }

    await this.section().saveConfig({
      virtualNetworkId: vnetId,
      subnetName: subnet,
      dnsMode: this.selectedDnsMode(),
      dnsHubResourceGroupId: this.showHubFields ? this.dnsHubResourceGroupId() || undefined : undefined,
      dnsHubSubscriptionId: this.showHubFields ? this.dnsHubSubscriptionId() || undefined : undefined,
    });
  }

  protected async onRemove(): Promise<void> {
    await this.section().removeConfig();
  }

  protected openDnsHelp(): void {
    this.dialog.open(DnsHelpDialogComponent, { width: '520px' });
  }
}
