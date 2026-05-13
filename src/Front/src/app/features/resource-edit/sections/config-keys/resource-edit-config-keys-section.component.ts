import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { CompactSelectComponent } from '../../../../shared/components/compact-select/compact-select.component';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditConfigKeysSection } from './resource-edit-config-keys-section.interface';

@Component({
  selector: 'app-resource-edit-config-keys-section',
  standalone: true,
  imports: [
    CompactSelectComponent,
    MatButtonModule,
    MatButtonToggleModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
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