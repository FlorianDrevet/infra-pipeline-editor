import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsButtonComponent } from '../../../../shared/components/ds/ds-button/ds-button.component';
import { DsTooltipDirective } from '../../../../shared/components/ds/ds-tooltip/ds-tooltip.directive';
import { TranslateModule } from '@ngx-translate/core';

import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditIdentityAccessSection } from './resource-edit-identity-access-section.interface';

@Component({
  selector: 'app-resource-edit-used-by-section',
  standalone: true,
  imports: [MatIconModule, DsSpinnerComponent, DsButtonComponent, DsTooltipDirective, TranslateModule],
  templateUrl: './resource-edit-used-by-section.component.html',
  styleUrl: './resource-edit-used-by-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditUsedBySectionComponent {
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ResourceEditIdentityAccessSection>();
}