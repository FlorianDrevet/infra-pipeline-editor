import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { RESOURCE_TYPE_ICONS } from '../../../../shared/resource-metadata/resource-type.metadata';
import { ResourceEditIdentityAccessSection } from './resource-edit-identity-access-section.interface';

@Component({
  selector: 'app-resource-edit-role-assignments-section',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    DsSpinnerComponent,
    MatTooltipModule,
    RouterLink,
    TranslateModule,
  ],
  templateUrl: './resource-edit-role-assignments-section.component.html',
  styleUrl: './resource-edit-role-assignments-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceEditRoleAssignmentsSectionComponent {
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  readonly canWrite = input.required<boolean>();
  readonly configId = input.required<string>();
  readonly section = input.required<ResourceEditIdentityAccessSection>();
}