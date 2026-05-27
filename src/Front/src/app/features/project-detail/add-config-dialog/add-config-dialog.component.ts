import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { DsButtonComponent, DsTextFieldComponent } from '../../../shared/components/ds';
import { InfraConfigService } from '../../../shared/services/infra-config.service';

export interface AddConfigDialogData {
  projectId: string;
}

@Component({
  selector: 'app-add-config-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './add-config-dialog.component.html',
  styleUrl: './add-config-dialog.component.scss',
})
export class AddConfigDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddConfigDialogComponent>);
  private readonly data: AddConfigDialogData = inject(MAT_DIALOG_DATA);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly fb = inject(FormBuilder);

  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(80)]],
  });

  protected get nameControl() {
    return this.form.controls.name;
  }

  protected async onSubmit(): Promise<void> {
    this.errorKey.set('');

    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const rawName = this.nameControl.getRawValue().trim();
    if (rawName.length < 3) {
      this.nameControl.setValue(rawName);
      this.nameControl.markAsTouched();
      this.nameControl.setErrors({ minlength: true });
      return;
    }

    this.isSubmitting.set(true);

    try {
      const created = await this.infraConfigService.create({
        name: rawName,
        projectId: this.data.projectId,
      });
      this.dialogRef.close(created);
    } catch {
      this.errorKey.set('PROJECT_DETAIL.CONFIGS.ADD_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }
}
