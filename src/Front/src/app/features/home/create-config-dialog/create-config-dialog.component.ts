import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import {
  CreateInfrastructureConfigRequest,
  InfrastructureConfigResponse,
} from '../../../shared/interfaces/infra-config.interface';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
  selector: 'app-create-config-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, TranslateModule],
  templateUrl: './create-config-dialog.component.html',
  styleUrl: './create-config-dialog.component.scss',
})
export class CreateConfigDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreateConfigDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly languageService = inject(LanguageService);

  protected readonly isCreating = signal(false);
  protected readonly createError = signal('');

  protected readonly createForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(80)]],
  });

  protected get nameControl() {
    return this.createForm.controls.name;
  }

  protected async onSubmit(): Promise<void> {
    this.createError.set('');

    if (this.createForm.invalid || this.isCreating()) {
      this.createForm.markAllAsTouched();
      return;
    }

    const rawName = this.nameControl.getRawValue().trim();

    if (rawName.length < 3) {
      this.nameControl.setValue(rawName);
      this.nameControl.markAsTouched();
      this.nameControl.setErrors({ minlength: true });
      return;
    }

    this.isCreating.set(true);

    try {
      const request: CreateInfrastructureConfigRequest = { name: rawName };
      const createdConfig = await this.infraConfigService.create(request);
      this.dialogRef.close(createdConfig);
    } catch (error) {
      this.createError.set(
        this.extractErrorMessage(error, 'HOME.DIALOG.CREATE_ERROR_FALLBACK')
      );
    } finally {
      this.isCreating.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }

  private extractErrorMessage(error: unknown, fallbackKey: string): string {
    if (typeof error !== 'object' || error === null) {
      return this.languageService.translate(fallbackKey);
    }

    const maybeResponse = error as {
      response?: {
        data?: {
          title?: string;
          detail?: string;
          errors?: Record<string, string[]>;
        };
      };
      message?: string;
    };

    const details = maybeResponse.response?.data;

    if (details?.errors) {
      const validationMessages = Object.values(details.errors).flat();
      if (validationMessages.length > 0) {
        return validationMessages[0] ?? this.languageService.translate(fallbackKey);
      }
    }

    return (
      details?.detail ||
      details?.title ||
      maybeResponse.message ||
      this.languageService.translate(fallbackKey)
    );
  }
}
