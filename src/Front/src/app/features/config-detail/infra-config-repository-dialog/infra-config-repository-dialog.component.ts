import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
} from '@angular/forms';
import { DsButtonComponent, DsTextFieldComponent, DsSelectComponent } from '../../../shared/components/ds';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { RepositoryContentKind } from '../../../shared/interfaces/project-repository.interface';
import {
  AddInfraConfigRepositoryRequest,
  InfraConfigRepositoryResponse,
  UpdateInfraConfigRepositoryRequest,
} from '../../../shared/interfaces/infra-config-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  buildRepositoryForm,
  CONTENT_KINDS,
  getSelectedContentKinds,
  PROVIDER_OPTIONS,
} from '../../../shared/utils/repository-dialog.utils';

export interface InfraConfigRepositoryDialogData {
  projectId: string;
  configId: string;
  mode: 'create' | 'edit';
  existing?: InfraConfigRepositoryResponse;
  /** Pre-selected and locked content kinds for slotted layouts. */
  lockedKinds?: RepositoryContentKind[];
}

@Component({
  selector: 'app-infra-config-repository-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatCheckboxModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
    DsSelectComponent,
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

  protected readonly form = buildRepositoryForm(
    this.fb, this.isEditMode, this.data.existing, this.lockedKinds
  );

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
    const selectedKinds = getSelectedContentKinds(raw.contentKinds, this.lockedKinds);

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
