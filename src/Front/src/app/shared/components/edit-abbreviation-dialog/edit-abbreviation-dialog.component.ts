import { Component, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent, DsTextFieldComponent } from '../ds';

export interface EditAbbreviationDialogData {
  resourceType: string;
  defaultAbbreviation: string;
  currentAbbreviation: string;
}

export interface EditAbbreviationDialogResult {
  abbreviation: string;
}

@Component({
  selector: 'app-edit-abbreviation-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './edit-abbreviation-dialog.component.html',
  styleUrl: './edit-abbreviation-dialog.component.scss',
})
export class EditAbbreviationDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<EditAbbreviationDialogComponent>);
  private readonly data: EditAbbreviationDialogData = inject(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);

  protected readonly resourceType = this.data.resourceType;
  protected readonly defaultAbbreviation = this.data.defaultAbbreviation;

  protected readonly form = this.fb.group({
    abbreviation: [
      this.data.currentAbbreviation,
      [
        Validators.required,
        Validators.maxLength(10),
        Validators.pattern(/^[a-z0-9]+$/),
      ],
    ],
  });

  protected onSubmit(): void {
    if (this.form.invalid) return;

    const result: EditAbbreviationDialogResult = {
      abbreviation: this.form.controls.abbreviation.value!,
    };
    this.dialogRef.close(result);
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }
}
