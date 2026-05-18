import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import {
  SetProjectGitPatRequest,
} from '../../../../shared/interfaces/project.interface';
import {
  DsAlertComponent,
  DsButtonComponent,
  DsTextFieldComponent,
} from '../../../../shared/components/ds';
import { ProjectService } from '../../../../shared/services/project.service';
import { extractLayoutRepositoriesApiErrorMessage } from '../layout-repositories-api-error';

const GITHUB_PROVIDER = 'GitHub';
const AZURE_DEVOPS_PROVIDER = 'AzureDevOps';

type PatHelpProvider = typeof GITHUB_PROVIDER | typeof AZURE_DEVOPS_PROVIDER;

const PROVIDER_HELP_KEYS: Record<PatHelpProvider, string> = {
  [AZURE_DEVOPS_PROVIDER]: 'PROJECT_DETAIL.GIT_CONFIG.FORM.PAT_HELP_AZUREDEVOPS',
  [GITHUB_PROVIDER]: 'PROJECT_DETAIL.GIT_CONFIG.FORM.PAT_HELP_GITHUB',
};

type ProjectGitPatDialogForm = FormGroup<{
  personalAccessToken: FormControl<string>;
}>;

export interface ProjectGitPatDialogData {
  projectId: string;
  repositoryId: string;
  providerTypes: string[];
}

@Component({
  selector: 'app-project-git-pat-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
    MatDialogModule,
    DsAlertComponent,
    DsButtonComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './project-git-pat-dialog.component.html',
  styleUrl: './project-git-pat-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectGitPatDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ProjectGitPatDialogComponent>);
  private readonly data: ProjectGitPatDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly translate = inject(TranslateService);

  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal('');
  protected readonly providerTypes = [...new Set(this.data.providerTypes)];
  protected readonly form: ProjectGitPatDialogForm = new FormGroup({
    personalAccessToken: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });
  protected readonly helpProviders = computed<PatHelpProvider[]>(() => {
    return this.providerTypes.filter(isPatHelpProvider);
  });

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const request: SetProjectGitPatRequest = {
      personalAccessToken: this.form.controls.personalAccessToken.value.trim(),
    };

    try {
      await this.projectService.setGitPat(this.data.projectId, this.data.repositoryId, request);
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(
        extractLayoutRepositoriesApiErrorMessage(
          error,
          this.translate.instant('PROJECT_DETAIL.LAYOUT.AUTH.SAVE_ERROR'),
        )
        ?? this.translate.instant('PROJECT_DETAIL.LAYOUT.AUTH.SAVE_ERROR'),
      );
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }

  protected getProviderHelpKey(provider: PatHelpProvider): string {
    return PROVIDER_HELP_KEYS[provider];
  }
}

function isPatHelpProvider(provider: string): provider is PatHelpProvider {
  return provider === GITHUB_PROVIDER || provider === AZURE_DEVOPS_PROVIDER;
}