import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailVariableGroupsSection } from './config-detail-variable-groups-section.interface';

@Component({
  selector: 'app-config-detail-variable-groups-section',
  standalone: true,
  imports: [MatIconModule, MatProgressSpinnerModule, MatTooltipModule, TranslateModule],
  templateUrl: './config-detail-variable-groups-section.component.html',
  styleUrl: './config-detail-variable-groups-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailVariableGroupsSectionComponent {
  readonly canWrite = input.required<boolean>();
  readonly section = input.required<ConfigDetailVariableGroupsSection>();
}