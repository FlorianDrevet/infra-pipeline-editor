import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse, ProjectMemberResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { PROJECT_ROLE_OPTIONS } from '../enums/project-role.enum';

export interface AddProjectMemberDialogData {
  projectId: string;
  existingMembers: ProjectMemberResponse[];
}

@Component({
  selector: 'app-add-member-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    TranslateModule,
  ],
  templateUrl: './add-member-dialog.component.html',
  styleUrl: './add-member-dialog.component.scss',
})
export class AddProjectMemberDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddProjectMemberDialogComponent>);
  private readonly data = inject<AddProjectMemberDialogData>(MAT_DIALOG_DATA);
  private readonly formBuilder = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);

  protected readonly roleOptions = PROJECT_ROLE_OPTIONS;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.formBuilder.nonNullable.group({
    userId: ['', [Validators.required]],
    role: ['Contributor', [Validators.required]],
  });

  protected async onSubmit(): Promise<void> {
    this.errorKey.set('');

    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    try {
      const result = await this.projectService.addMember(this.data.projectId, {
        userId: this.form.controls.userId.getRawValue().trim(),
        role: this.form.controls.role.getRawValue(),
      });
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set('PROJECTS.MEMBERS.ADD_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}
