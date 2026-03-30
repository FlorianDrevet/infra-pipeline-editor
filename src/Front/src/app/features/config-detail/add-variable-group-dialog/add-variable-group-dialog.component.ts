import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-add-variable-group-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './add-variable-group-dialog.component.html',
  styleUrl: './add-variable-group-dialog.component.scss',
})
export class AddVariableGroupDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddVariableGroupDialogComponent>);
  private readonly fb = inject(FormBuilder);

  protected readonly form = this.fb.group({
    groupName: ['', [Validators.required, Validators.maxLength(200)]],
  });

  protected onCancel(): void {
    this.dialogRef.close();
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;
    this.dialogRef.close(this.form.getRawValue().groupName);
  }
}
