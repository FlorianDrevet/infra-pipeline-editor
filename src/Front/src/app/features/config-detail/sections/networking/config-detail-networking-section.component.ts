import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import {
  DsButtonComponent,
  DsCardMatComponent,
  DsPanelActionButtonComponent,
  DsSelectComponent,
  DsSpinnerComponent,
  DsTextFieldComponent,
  DsToggleComponent,
} from '../../../../shared/components/ds';
import { VnetHelpDialogComponent } from '../../../../shared/components/vnet-help-dialog/vnet-help-dialog.component';
import { ConfigDetailNetworkingSectionViewModel } from './config-detail-networking-section.view-model';

@Component({
  selector: 'app-config-detail-networking-section',
  standalone: true,
  imports: [
    DsButtonComponent,
    DsCardMatComponent,
    DsPanelActionButtonComponent,
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
  private readonly dialog = inject(MatDialog);

  readonly viewModel = input.required<ConfigDetailNetworkingSectionViewModel>();

  protected openNetworkingHelpDialog(): void {
    this.dialog.open(VnetHelpDialogComponent, {
      width: '640px',
      data: { context: 'networkingProfile' as const },
    });
  }
}
