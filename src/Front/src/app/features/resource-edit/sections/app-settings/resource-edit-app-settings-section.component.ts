import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditKvMissingRoleCardComponent } from '../shared/resource-edit-kv-missing-role-card.component';
import { ResourceEditAppSettingsSection } from './resource-edit-app-settings-section.interface';

@Component({
  selector: 'app-resource-edit-app-settings-section',
  standalone: true,
  imports: [
    DsButtonComponent,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    ResourceEditKvMissingRoleCardComponent,
    TranslateModule,
  ],
  templateUrl: './resource-edit-app-settings-section.component.html',
  styleUrl: './resource-edit-app-settings-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditAppSettingsSectionComponent {
  protected readonly addActionIcon = 'add';
  protected readonly importActionIcon = 'upload_file';
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  readonly canWrite = input.required<boolean>();
  readonly environments = input.required<EnvironmentDefinitionResponse[]>();
  readonly section = input.required<ResourceEditAppSettingsSection>();
}