import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsAlertComponent, DsButtonComponent } from '../ds';

interface VnetHelpDialogSection {
  readonly icon: string;
  readonly titleKey: string;
  readonly bodyKey: string;
  readonly guidanceKey?: string;
}

const ResourceSections: readonly VnetHelpDialogSection[] = [
  {
    icon: 'lan',
    titleKey: 'COMMON.VNET_HELP_DIALOG.ADDRESS_SPACES.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.ADDRESS_SPACES.BODY',
    guidanceKey: 'COMMON.VNET_HELP_DIALOG.ADDRESS_SPACES.GUIDANCE',
  },
  {
    icon: 'dns',
    titleKey: 'COMMON.VNET_HELP_DIALOG.DNS_SERVERS.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.DNS_SERVERS.BODY',
    guidanceKey: 'COMMON.VNET_HELP_DIALOG.DNS_SERVERS.GUIDANCE',
  },
  {
    icon: 'shield',
    titleKey: 'COMMON.VNET_HELP_DIALOG.DDOS.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.DDOS.BODY',
    guidanceKey: 'COMMON.VNET_HELP_DIALOG.DDOS.GUIDANCE',
  },
  {
    icon: 'route',
    titleKey: 'COMMON.VNET_HELP_DIALOG.IMPACT.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.IMPACT.BODY',
  },
];

const NetworkingProfileSections: readonly VnetHelpDialogSection[] = [
  {
    icon: 'lan',
    titleKey: 'COMMON.VNET_HELP_DIALOG.ADDRESS_SPACES.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.ADDRESS_SPACES.BODY',
    guidanceKey: 'COMMON.VNET_HELP_DIALOG.ADDRESS_SPACES.GUIDANCE',
  },
  {
    icon: 'hub',
    titleKey: 'COMMON.VNET_HELP_DIALOG.SUBNET_PREFIX.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.SUBNET_PREFIX.BODY',
    guidanceKey: 'COMMON.VNET_HELP_DIALOG.SUBNET_PREFIX.GUIDANCE',
  },
  {
    icon: 'route',
    titleKey: 'COMMON.VNET_HELP_DIALOG.IMPACT.TITLE',
    bodyKey: 'COMMON.VNET_HELP_DIALOG.IMPACT.BODY',
  },
];

export interface VnetHelpDialogData {
  readonly context: 'resourceCreate' | 'resourceEdit' | 'networkingProfile';
}

@Component({
  selector: 'app-vnet-help-dialog',
  standalone: true,
  imports: [MatDialogModule, MatIconModule, TranslateModule, DsAlertComponent, DsButtonComponent],
  templateUrl: './vnet-help-dialog.component.html',
  styleUrl: './vnet-help-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VnetHelpDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<VnetHelpDialogComponent>);
  protected readonly data: VnetHelpDialogData = inject(MAT_DIALOG_DATA);

  protected readonly titleKey = this.resolveTitleKey();
  protected readonly subtitleKey = this.resolveSubtitleKey();
  protected readonly sections = this.data.context === 'networkingProfile'
    ? NetworkingProfileSections
    : ResourceSections;

  protected close(): void {
    this.dialogRef.close();
  }

  private resolveTitleKey(): string {
    switch (this.data.context) {
      case 'resourceEdit':
        return 'COMMON.VNET_HELP_DIALOG.RESOURCE_EDIT.TITLE';
      case 'networkingProfile':
        return 'COMMON.VNET_HELP_DIALOG.NETWORKING_PROFILE.TITLE';
      default:
        return 'COMMON.VNET_HELP_DIALOG.RESOURCE_CREATE.TITLE';
    }
  }

  private resolveSubtitleKey(): string {
    switch (this.data.context) {
      case 'resourceEdit':
        return 'COMMON.VNET_HELP_DIALOG.RESOURCE_EDIT.SUBTITLE';
      case 'networkingProfile':
        return 'COMMON.VNET_HELP_DIALOG.NETWORKING_PROFILE.SUBTITLE';
      default:
        return 'COMMON.VNET_HELP_DIALOG.RESOURCE_CREATE.SUBTITLE';
    }
  }
}