import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditIdentityAccessSection } from './resource-edit-identity-access-section.interface';

@Component({
  selector: 'app-resource-edit-used-by-section',
  standalone: true,
  imports: [MatIconModule, MatProgressSpinnerModule, MatTooltipModule, TranslateModule],
  templateUrl: './resource-edit-used-by-section.component.html',
  styleUrl: './resource-edit-used-by-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditUsedBySectionComponent {
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ResourceEditIdentityAccessSection>();
}