import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import {
  AddProjectRepositoryRequest,
  ProjectRepositoryResponse,
  RepositoryContentKind,
  UpdateProjectRepositoryRequest,
} from '../../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../../shared/services/project.service';

export interface RepositoryDialogData {
  projectId: string;
  mode: 'create' | 'edit';
  existing?: ProjectRepositoryResponse;
}

const PROVIDER_OPTIONS: ReadonlyArray<{ value: string; label: string }> = [
  { value: 'AzureDevOps', label: 'Azure DevOps' },
  { value: 'GitHub', label: 'GitHub' },
  { value: 'GitLab', label: 'GitLab' },
  { value: 'Bitbucket', label: 'Bitbucket' },
];

const CONTENT_KINDS: ReadonlyArray<RepositoryContentKind> = [
  'Infrastructure',
  'ApplicationCode',
  'Pipelines',
];

@Component({
  selector: 'app-repository-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './repository-dialog.component.html',
  styleUrl: './repository-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RepositoryDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<RepositoryDialogComponent>);
  private readonly data: RepositoryDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly isEditMode = this.data.mode === 'edit';
  protected readonly providerOptions = PROVIDER_OPTIONS;
  protected readonly contentKinds = CONTENT_KINDS;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = this.fb.group({
    alias: new FormControl<string>(
      { value: this.data.existing?.alias ?? '', disabled: this.isEditMode },
      {
        nonNullable: true,
        validators: [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)],
      }
    ),
    providerType: new FormControl<string>(
      this.data.existing?.providerType ?? 'AzureDevOps',
      { nonNullable: true, validators: [Validators.required] }
    ),
    repositoryUrl: new FormControl<string>(
      this.data.existing?.repositoryUrl ?? '',
      { nonNullable: true, validators: [Validators.required] }
    ),
    defaultBranch: new FormControl<string>(
      this.data.existing?.defaultBranch ?? 'main',
      { nonNullable: true, validators: [Validators.required] }
    ),
    contentKinds: this.fb.array<FormControl<boolean>>(
      CONTENT_KINDS.map(
        (kind) =>
          new FormControl<boolean>(
            this.data.existing?.contentKinds?.includes(kind) ?? false,
            { nonNullable: true }
          )
      ),
      [atLeastOneChecked()]
    ),
  });

  protected get contentKindsArray(): FormArray<FormControl<boolean>> {
    return this.form.controls.contentKinds;
  }

  protected isContentKindChecked(index: number): boolean {
    return this.contentKindsArray.at(index).value;
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const raw = this.form.getRawValue();
    const selectedKinds: RepositoryContentKind[] = CONTENT_KINDS.filter(
      (_, idx) => raw.contentKinds[idx]
    );

    try {
      if (this.isEditMode && this.data.existing) {
        const req: UpdateProjectRepositoryRequest = {
          providerType: raw.providerType,
          repositoryUrl: raw.repositoryUrl,
          defaultBranch: raw.defaultBranch,
          contentKinds: selectedKinds,
        };
        await this.projectService.updateRepository(
          this.data.projectId,
          this.data.existing.id,
          req
        );
        this.dialogRef.close({ updated: true });
      } else {
        const req: AddProjectRepositoryRequest = {
          alias: raw.alias,
          providerType: raw.providerType,
          repositoryUrl: raw.repositoryUrl,
          defaultBranch: raw.defaultBranch,
          contentKinds: selectedKinds,
        };
        const result = await this.projectService.addRepository(
          this.data.projectId,
          req
        );
        this.dialogRef.close({ created: true, id: result.id });
      }
    } catch {
      this.errorKey.set('PROJECT_DETAIL.LAYOUT.DIALOG_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}

function atLeastOneChecked() {
  return (control: AbstractControl) => {
    const arr = control as FormArray<FormControl<boolean>>;
    const any = arr.controls.some((c) => c.value === true);
    return any ? null : { required: true };
  };
}
