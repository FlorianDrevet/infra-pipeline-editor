import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { DependentResourceResponse } from '../../interfaces/dependent-resource.interface';

export interface CascadeDeleteDialogData {
  titleKey: string;
  messageKey: string;
  messageParams?: Record<string, string | number>;
  dependentsHeaderKey: string;
  confirmKey: string;
  cancelKey: string;
  dependents: DependentResourceResponse[];
  resourceTypeIcons: Record<string, string>;
}

@Component({
  selector: 'app-cascade-delete-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, TranslateModule],
  templateUrl: './cascade-delete-dialog.component.html',
  styleUrl: './cascade-delete-dialog.component.scss',
})
export class CascadeDeleteDialogComponent {
  protected readonly dialogRef = inject(MatDialogRef<CascadeDeleteDialogComponent>);
  protected readonly data: CascadeDeleteDialogData = inject(MAT_DIALOG_DATA);

  protected onConfirm(): void {
    this.dialogRef.close(true);
  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }
}
