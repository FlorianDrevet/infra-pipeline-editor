import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { DiagnosticPopoverComponent } from '../../../../shared/components/diagnostic-popover/diagnostic-popover.component';
import { DsSelectComponent } from '../../../../shared/components/ds';
import { ConfigDetailResourcesSectionViewModel } from './config-detail-resources-section.view-model';

@Component({
  selector: 'app-config-detail-resources-section',
  standalone: true,
  imports: [
    DiagnosticPopoverComponent,
    DsSelectComponent,
    FormsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    RouterLink,
    TranslateModule,
  ],
  templateUrl: './config-detail-resources-section.component.html',
  styleUrl: './config-detail-resources-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailResourcesSectionComponent {
  readonly viewModel = input.required<ConfigDetailResourcesSectionViewModel>();
}