import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
} from '@angular/forms';
import {
  DsAutocompleteComponent,
  DsAutocompleteOption,
  DsButtonComponent,
  DsSelectComponent,
  DsTextFieldComponent,
} from '../../../../shared/components/ds';
import { MatCheckboxModule } from '@angular/material/checkbox';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import {
  AddProjectRepositoryRequest,
  ProjectRepositoryResponse,
  RepositoryContentKind,
  UpdateProjectRepositoryRequest,
  VerifiedGitBranchResponse,
  VerifyProjectRepositoryRequest,
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

type RepositoryVerificationState = 'idle' | 'checking' | 'verified' | 'failed';

@Component({
  selector: 'app-repository-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatCheckboxModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsAutocompleteComponent,
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
  protected readonly verificationState = signal<RepositoryVerificationState>('idle');
  protected readonly verifiedBranches = signal<VerifiedGitBranchResponse[]>([]);
  protected readonly branchQuery = signal('');
  protected readonly verifiedOwner = signal<string | null>(null);
  protected readonly verifiedRepositoryName = signal<string | null>(null);

  protected readonly form = buildRepositoryForm(
    this.fb, this.isEditMode, this.data.existing, this.lockedKinds
  );

  protected readonly branchOptions = computed<ReadonlyArray<DsAutocompleteOption<string>>>(() => {
    const query = this.branchQuery().trim().toLocaleLowerCase();

    return this.verifiedBranches()
      .filter((branch) => !query || branch.name.toLocaleLowerCase().includes(query))
      .map((branch) => ({
        value: branch.name,
        label: branch.name,
        icon: branch.isProtected ? 'verified_user' : 'lan',
      }));
  });

  protected readonly verificationStatusKey = computed(() => {
    switch (this.verificationState()) {
      case 'checking':
        return 'PROJECT_DETAIL.LAYOUT.FORM.VERIFY_CHECKING';
      case 'verified':
        return 'PROJECT_DETAIL.LAYOUT.FORM.VERIFY_SUCCESS';
      case 'failed':
        return 'PROJECT_DETAIL.LAYOUT.FORM.VERIFY_FAILED';
      default:
        return '';
    }
  });

  protected readonly branchErrorKey = computed(() => {
    const control = this.form.controls.defaultBranch;
    if (this.verificationState() !== 'verified' || !control.touched || this.isVerifiedBranch(control.value)) {
      return '';
    }

    return 'PROJECT_DETAIL.LAYOUT.FORM.DEFAULT_BRANCH_UNVERIFIED';
  });

  public constructor() {
    this.form.controls.defaultBranch.disable({ emitEvent: false });

    this.form.controls.providerType.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.resetVerification());
    this.form.controls.repositoryUrl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.resetVerification());
    this.form.controls.personalAccessToken.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.resetVerification());
    this.form.controls.defaultBranch.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.branchQuery.set(value));
  }

  protected get contentKindsArray(): FormArray<FormControl<boolean>> {
    return this.form.controls.contentKinds;
  }

  protected async onSubmit(): Promise<void> {
    if (!this.canSave()) return;

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
        const personalAccessToken = raw.personalAccessToken.trim();
        if (personalAccessToken) {
          req.personalAccessToken = personalAccessToken;
        }
        await this.projectService.updateRepository(
          this.data.projectId,
          this.data.existing.id,
          req
        );
        this.dialogRef.close({ updated: true });
      } else {
        const req: AddProjectRepositoryRequest = {
          providerType: raw.providerType,
          repositoryUrl: raw.repositoryUrl,
          defaultBranch: raw.defaultBranch,
          personalAccessToken: raw.personalAccessToken.trim(),
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

  protected canVerify(): boolean {
    const raw = this.form.getRawValue();
    const hasConnectionFields = raw.providerType.trim().length > 0
      && raw.repositoryUrl.trim().length > 0;
    const hasRequiredPat = this.isEditMode || raw.personalAccessToken.trim().length > 0;

    return hasConnectionFields
      && hasRequiredPat
      && this.verificationState() !== 'checking'
      && !this.isSubmitting();
  }

  protected canSave(): boolean {
    return this.form.valid
      && this.verificationState() === 'verified'
      && this.isVerifiedBranch(this.form.controls.defaultBranch.value)
      && !this.isSubmitting();
  }

  protected async verifyConnection(): Promise<void> {
    if (!this.canVerify()) return;

    this.verificationState.set('checking');
    this.errorKey.set('');
    this.form.controls.defaultBranch.disable({ emitEvent: false });

    const raw = this.form.getRawValue();
    const request: VerifyProjectRepositoryRequest = {
      providerType: raw.providerType,
      repositoryUrl: raw.repositoryUrl.trim(),
    };
    const personalAccessToken = raw.personalAccessToken.trim();
    if (personalAccessToken) {
      request.personalAccessToken = personalAccessToken;
    }

    try {
      const result = await this.projectService.verifyRepositoryConnection(
        this.data.projectId,
        request,
        this.data.existing?.id,
      );
      this.verifiedBranches.set(result.branches);
      this.verifiedOwner.set(result.owner);
      this.verifiedRepositoryName.set(result.repositoryName);

      const defaultBranch = result.defaultBranchCandidate
        ?? this.data.existing?.defaultBranch
        ?? result.branches[0]?.name
        ?? '';
      this.form.controls.defaultBranch.enable({ emitEvent: false });
      this.form.controls.defaultBranch.setValue(defaultBranch, { emitEvent: false });
      this.branchQuery.set(defaultBranch);
      this.form.controls.defaultBranch.markAsTouched();
      this.verificationState.set('verified');
    } catch {
      this.verifiedBranches.set([]);
      this.verifiedOwner.set(null);
      this.verifiedRepositoryName.set(null);
      this.form.controls.defaultBranch.setValue('', { emitEvent: false });
      this.branchQuery.set('');
      this.verificationState.set('failed');
    }
  }

  protected onBranchSelected(option: DsAutocompleteOption<unknown>): void {
    const branchName = String(option.value);
    this.form.controls.defaultBranch.setValue(branchName);
    this.branchQuery.set(branchName);
  }

  protected onBranchSearchChanged(query: string): void {
    this.branchQuery.set(query);
  }

  private resetVerification(): void {
    if (this.verificationState() === 'checking') {
      return;
    }

    this.verificationState.set('idle');
    this.verifiedBranches.set([]);
    this.verifiedOwner.set(null);
    this.verifiedRepositoryName.set(null);
    this.errorKey.set('');
    this.form.controls.defaultBranch.disable({ emitEvent: false });
    this.form.controls.defaultBranch.setValue('', { emitEvent: false });
    this.branchQuery.set('');
  }

  private isVerifiedBranch(branchName: string): boolean {
    const normalized = branchName.trim();
    return this.verifiedBranches().some((branch) => branch.name === normalized);
  }
}
