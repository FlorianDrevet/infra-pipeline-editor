import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslateModule } from '@ngx-translate/core';

import { BicepFilePanelComponent } from '../../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import { DsPanelActionButtonComponent } from '../../../../shared/components/ds';
import { ConfigDetailGenerationSectionViewModel } from './config-detail-generation-section.view-model';

@Component({
  selector: 'app-config-detail-generation-section',
  standalone: true,
  imports: [
    BicepFilePanelComponent,
    DsPanelActionButtonComponent,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    TranslateModule,
  ],
  templateUrl: './config-detail-generation-section.component.html',
  styleUrl: './config-detail-generation-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailGenerationSectionComponent {
  readonly viewModel = input.required<ConfigDetailGenerationSectionViewModel>();
}