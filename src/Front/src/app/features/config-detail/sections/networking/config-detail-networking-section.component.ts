import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import {
  DsButtonComponent,
  DsSelectComponent,
  DsSpinnerComponent,
  DsTextFieldComponent,
  DsToggleComponent,
} from '../../../../shared/components/ds';
import { ConfigDetailNetworkingSectionViewModel } from './config-detail-networking-section.view-model';

@Component({
  selector: 'app-config-detail-networking-section',
  standalone: true,
  imports: [
    DsButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
    DsToggleComponent,
    DsSpinnerComponent,
    FormsModule,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './config-detail-networking-section.component.html',
  styleUrl: './config-detail-networking-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailNetworkingSectionComponent {
  readonly viewModel = input.required<ConfigDetailNetworkingSectionViewModel>();
}
