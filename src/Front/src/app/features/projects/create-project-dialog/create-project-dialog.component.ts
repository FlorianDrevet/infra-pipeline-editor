import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { CreateProjectRequest, ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';

@Component({
  selector: 'app-create-project-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, TranslateModule],
  templateUrl: './create-project-dialog.component.html',
  styleUrl: './create-project-dialog.component.scss',
})
export class CreateProjectDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreateProjectDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);

  protected readonly isCreating = signal(false);
  protected readonly createErrorKey = signal('');

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(80)]],
    description: [''],
  });

  protected get nameControl() {
    return this.form.controls.name;
  }

  protected async onSubmit(): Promise<void> {
    this.createErrorKey.set('');

    if (this.form.invalid || this.isCreating()) {
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

    this.isCreating.set(true);

    try {
      const request: CreateProjectRequest = {
        name: rawName,
        description: this.form.controls.description.getRawValue().trim() || undefined,
      };
      const created = await this.projectService.create(request);
      this.dialogRef.close(created);
    } catch {
      this.createErrorKey.set('PROJECTS.DIALOG.CREATE_ERROR');
    } finally {
      this.isCreating.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}
