import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { GitConfigResponse, ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { GIT_PROVIDER_TYPE_OPTIONS } from '../enums/git-provider-type.enum';

export interface GitConfigDialogData {
  projectId: string;
  existing?: GitConfigResponse | null;
}

@Component({
  selector: 'app-git-config-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './git-config-dialog.component.html',
  styleUrl: './git-config-dialog.component.scss',
})
export class GitConfigDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<GitConfigDialogComponent>);
  private readonly data: GitConfigDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly isEditMode = !!this.data.existing;
  protected readonly providerOptions = GIT_PROVIDER_TYPE_OPTIONS;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.group({
    providerType: [this.data.existing?.providerType ?? '', Validators.required],
    repositoryUrl: [this.data.existing?.repositoryUrl ?? '', [Validators.required]],
    defaultBranch: [this.data.existing?.defaultBranch ?? 'main', Validators.required],
    basePath: [this.data.existing?.basePath ?? ''],
    personalAccessToken: ['', Validators.required],
  });

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const value = this.form.getRawValue();
      const updated = await this.projectService.setGitConfig(this.data.projectId, {
        providerType: value.providerType!,
        repositoryUrl: value.repositoryUrl!,
        defaultBranch: value.defaultBranch!,
        basePath: value.basePath || null,
        personalAccessToken: value.personalAccessToken!,
      });
      this.dialogRef.close(updated);
    } catch {
      this.errorKey.set('PROJECT_DETAIL.GIT_CONFIG.DIALOG_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}
