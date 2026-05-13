import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailNamingSectionViewModel } from './config-detail-naming-section.view-model';

@Component({
  selector: 'app-config-detail-naming-section',
  standalone: true,
  imports: [MatIconModule, MatProgressSpinnerModule, MatSlideToggleModule, MatTooltipModule, TranslateModule],
  templateUrl: './config-detail-naming-section.component.html',
  styleUrl: './config-detail-naming-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailNamingSectionComponent {
  readonly viewModel = input.required<ConfigDetailNamingSectionViewModel>();
}