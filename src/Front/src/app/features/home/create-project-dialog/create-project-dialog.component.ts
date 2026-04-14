import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import {
  CreateProjectRequest,
  ProjectResponse,
} from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { LanguageService } from '../../../shared/services/language.service';

@Component({
  selector: 'app-create-project-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonToggleModule, MatIconModule, TranslateModule],
  templateUrl: './create-project-dialog.component.html',
  styleUrl: './create-project-dialog.component.scss',
})
export class CreateProjectDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreateProjectDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly languageService = inject(LanguageService);

  protected readonly isCreating = signal(false);
  protected readonly createError = signal('');

  protected readonly createForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(80)]],
    description: [''],
    repositoryMode: ['MultiRepo'],
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
      const request: CreateProjectRequest = {
        name: rawName,
        description: this.createForm.controls.description.getRawValue().trim() || undefined,
      };
      const created = await this.projectService.createProject(request);
      const selectedMode = this.createForm.controls.repositoryMode.getRawValue();
      if (selectedMode !== 'MultiRepo') {
        await this.projectService.setRepositoryMode(created.id, { repositoryMode: selectedMode });
        created.repositoryMode = selectedMode;
      }
      this.dialogRef.close(created);
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
