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
import { DsButtonComponent } from '../../../shared/components/ds';
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
import { RepositoryContentKind } from '../../../shared/interfaces/project-repository.interface';
import {
  AddInfraConfigRepositoryRequest,
  InfraConfigRepositoryResponse,
  UpdateInfraConfigRepositoryRequest,
} from '../../../shared/interfaces/infra-config-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';

export interface InfraConfigRepositoryDialogData {
  projectId: string;
  configId: string;
  mode: 'create' | 'edit';
  existing?: InfraConfigRepositoryResponse;
  /** Pre-selected and locked content kinds for slotted layouts. */
  lockedKinds?: RepositoryContentKind[];
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
];

@Component({
  selector: 'app-infra-config-repository-dialog',
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
      DsButtonComponent,
  ],
  templateUrl: './infra-config-repository-dialog.component.html',
  styleUrl: './infra-config-repository-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InfraConfigRepositoryDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<InfraConfigRepositoryDialogComponent>);
  private readonly data: InfraConfigRepositoryDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly isEditMode = this.data.mode === 'edit';
  protected readonly providerOptions = PROVIDER_OPTIONS;
  protected readonly contentKinds = CONTENT_KINDS;
  protected readonly lockedKinds: ReadonlyArray<RepositoryContentKind> = this.data.lockedKinds ?? [];
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
      CONTENT_KINDS.map((kind) => {
        const isLocked = this.lockedKinds.includes(kind);
        const initialChecked = isLocked
          ? true
          : (this.data.existing?.contentKinds?.includes(kind) ?? false);
        return new FormControl<boolean>(
          { value: initialChecked, disabled: isLocked },
          { nonNullable: true }
        );
      }),
      [atLeastOneChecked()]
    ),
  });

  protected get contentKindsArray(): FormArray<FormControl<boolean>> {
    return this.form.controls.contentKinds;
  }

  protected isContentKindLocked(kind: RepositoryContentKind): boolean {
    return this.lockedKinds.includes(kind);
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const raw = this.form.getRawValue();
    const selectedKinds: RepositoryContentKind[] = CONTENT_KINDS.filter(
      (kind, idx) => raw.contentKinds[idx] || this.lockedKinds.includes(kind)
    );

    try {
      if (this.isEditMode && this.data.existing) {
        const req: UpdateInfraConfigRepositoryRequest = {
          providerType: raw.providerType,
          repositoryUrl: raw.repositoryUrl,
          defaultBranch: raw.defaultBranch,
          contentKinds: selectedKinds,
        };
        await this.projectService.updateConfigRepository(
          this.data.projectId,
          this.data.configId,
          this.data.existing.id,
          req
        );
        this.dialogRef.close({ updated: true });
      } else {
        const req: AddInfraConfigRepositoryRequest = {
          alias: raw.alias,
          providerType: raw.providerType,
          repositoryUrl: raw.repositoryUrl,
          defaultBranch: raw.defaultBranch,
          contentKinds: selectedKinds,
        };
        const result = await this.projectService.addConfigRepository(
          this.data.projectId,
          this.data.configId,
          req
        );
        this.dialogRef.close({ created: true, id: result.id });
      }
    } catch {
      this.errorKey.set('CONFIG_DETAIL.REPOSITORIES.DIALOG.SAVE_ERROR');
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
