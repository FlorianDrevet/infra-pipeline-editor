import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
} from '@angular/forms';
import { DsButtonComponent, DsTextFieldComponent, DsSelectComponent } from '../../../../shared/components/ds';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import {
  AddProjectRepositoryRequest,
  ProjectRepositoryResponse,
  RepositoryContentKind,
  UpdateProjectRepositoryRequest,
} from '../../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../../shared/services/project.service';
import {
  buildRepositoryForm,
  CONTENT_KINDS,
  getSelectedContentKinds,
  PROVIDER_OPTIONS,
} from '../../../../shared/utils/repository-dialog.utils';

export interface RepositoryDialogData {
  projectId: string;
  mode: 'create' | 'edit';
  existing?: ProjectRepositoryResponse;
  /**
   * When set, the listed content kinds are pre-selected and the toggles are locked.
   * Used for slotted layouts (AllInOne / SplitInfraCode) so the user cannot mismatch the slot.
   */
  lockedKinds?: RepositoryContentKind[];
}

@Component({
  selector: 'app-repository-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
    DsSelectComponent,
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
  protected readonly lockedKinds: ReadonlyArray<RepositoryContentKind> = this.data.lockedKinds ?? [];
  protected readonly hasLockedKinds = this.lockedKinds.length > 0;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly form = buildRepositoryForm(
    this.fb, this.isEditMode, this.data.existing, this.lockedKinds
  );

  protected get contentKindsArray(): FormArray<FormControl<boolean>> {
    return this.form.controls.contentKinds;
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const raw = this.form.getRawValue();
    const selectedKinds = getSelectedContentKinds(raw.contentKinds, this.lockedKinds);

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
