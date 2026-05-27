import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { DsIconButtonComponent } from '../../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditKvMissingRoleCardComponent } from '../shared/resource-edit-kv-missing-role-card.component';
import { ResourceEditConfigKeysSection } from './resource-edit-config-keys-section.interface';

@Component({
  selector: 'app-resource-edit-config-keys-section',
  standalone: true,
  imports: [
    MatIconModule,
    DsSpinnerComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    MatTooltipModule,
    ResourceEditKvMissingRoleCardComponent,
    TranslateModule,
  ],
  templateUrl: './resource-edit-config-keys-section.component.html',
  styleUrl: './resource-edit-config-keys-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditConfigKeysSectionComponent {
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  readonly canWrite = input.required<boolean>();
  readonly environments = input.required<EnvironmentDefinitionResponse[]>();
  readonly section = input.required<ResourceEditConfigKeysSection>();
}