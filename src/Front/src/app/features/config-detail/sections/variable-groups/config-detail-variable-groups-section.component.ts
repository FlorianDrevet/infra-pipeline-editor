import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent, DsIconButtonComponent } from '../../../../shared/components/ds';
import { ConfigDetailVariableGroupsSection } from './config-detail-variable-groups-section.interface';

@Component({
  selector: 'app-config-detail-variable-groups-section',
  standalone: true,
  imports: [MatIconModule, DsSpinnerComponent, TranslateModule, DsButtonComponent, DsIconButtonComponent],
  templateUrl: './config-detail-variable-groups-section.component.html',
  styleUrl: './config-detail-variable-groups-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailVariableGroupsSectionComponent {
  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ConfigDetailVariableGroupsSection>();
}