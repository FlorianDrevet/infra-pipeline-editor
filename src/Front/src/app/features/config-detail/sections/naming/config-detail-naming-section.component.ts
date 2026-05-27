import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent, DsIconButtonComponent, DsToggleComponent } from '../../../../shared/components/ds';
import { ConfigDetailNamingSectionViewModel } from './config-detail-naming-section.view-model';

@Component({
  selector: 'app-config-detail-naming-section',
  standalone: true,
  imports: [FormsModule, MatIconModule, DsSpinnerComponent, MatTooltipModule, TranslateModule, DsToggleComponent, DsButtonComponent, DsIconButtonComponent],
  templateUrl: './config-detail-naming-section.component.html',
  styleUrl: './config-detail-naming-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailNamingSectionComponent {
  readonly viewModel = input.required<ConfigDetailNamingSectionViewModel>();
}