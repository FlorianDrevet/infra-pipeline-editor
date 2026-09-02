import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { DsButtonComponent } from '../ds-button/ds-button.component';
import { DsChipComponent } from '../ds-chip/ds-chip.component';
import { DsPropertyHelpDialogData } from './ds-property-help-button.types';

/**
 * Modal dialog that renders structured help sections for a property.
 * Opened by {@link DsPropertyHelpButtonComponent}.
 */
@Component({
  selector: 'app-ds-property-help-dialog',
  standalone: true,
  imports: [MatDialogModule, MatIconModule, DsButtonComponent, DsChipComponent],
  templateUrl: './ds-property-help-dialog.component.html',
  styleUrl: './ds-property-help-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsPropertyHelpDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<DsPropertyHelpDialogComponent>);
  protected readonly data: DsPropertyHelpDialogData = inject(MAT_DIALOG_DATA);

  protected close(): void {
    this.dialogRef.close();
  }
}
