import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';

import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditIdentityAccessSection } from './resource-edit-identity-access-section.interface';

@Component({
  selector: 'app-resource-edit-granted-rights-section',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, TranslateModule],
  templateUrl: './resource-edit-granted-rights-section.component.html',
  styleUrl: './resource-edit-granted-rights-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditGrantedRightsSectionComponent {
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ResourceEditIdentityAccessSection>();
}