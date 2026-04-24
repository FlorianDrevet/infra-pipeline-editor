import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent, DsTextFieldComponent, DsTextareaComponent } from '../../../shared/components/ds';
import { MatCardModule } from '@angular/material/card';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectService } from '../../../shared/services/project.service';
import {
  MultiRepoPushResponse,
  RepoPushResult,
} from '../../../shared/interfaces/multi-repo-push.interface';

export interface MultiRepoPushDialogData {
  projectId: string;
  infraAlias: string;
  codeAlias: string;
}

type DialogState = 'form' | 'pushing' | 'success' | 'partial' | 'error';

/**
 * Dual-repo push dialog for SplitInfraCode projects.
 * Backend always returns 200; per-repo results may be partial.
 *
 * v1 limitation: branch list is not fetched per repo (each project repo can have its own
 * branches, but listBranches() is project-scoped). The branch input is a free-text field
 * prefilled from localStorage. Users can type any valid branch name.
 */
@Component({
  selector: 'app-multi-repo-push-dialog',
  standalone: true,
  imports: [
    MatAutocompleteModule,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
    DsTextareaComponent,
  ],
  templateUrl: './multi-repo-push-dialog.component.html',
  styleUrl: './multi-repo-push-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MultiRepoPushDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<MultiRepoPushDialogComponent>);
  protected readonly data: MultiRepoPushDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  protected readonly state = signal<DialogState>('form');
  protected readonly infraResult = signal<RepoPushResult | null>(null);
  protected readonly codeResult = signal<RepoPushResult | null>(null);
  protected readonly errorKey = signal('');

  private readonly infraBranchKey = `ifs-push-branch-multi-${this.data.projectId}-${this.data.infraAlias}`;
  private readonly codeBranchKey = `ifs-push-branch-multi-${this.data.projectId}-${this.data.codeAlias}`;

  protected readonly infraForm = new FormGroup({
    branch: new FormControl<string>(localStorage.getItem(this.infraBranchKey) ?? 'main', { nonNullable: true, validators: [Validators.required] }),
    commit: new FormControl<string>('', { nonNullable: true }),
  });

  protected readonly codeForm = new FormGroup({
    branch: new FormControl<string>(localStorage.getItem(this.codeBranchKey) ?? 'main', { nonNullable: true, validators: [Validators.required] }),
    commit: new FormControl<string>('', { nonNullable: true }),
  });

  protected readonly canPush = computed(() => {
    const isPushing = this.state() === 'pushing';
    return !isPushing && this.infraForm.valid && this.codeForm.valid;
  });

  protected async onPush(): Promise<void> {
    if (this.state() === 'pushing') return;
    if (this.infraForm.invalid || this.codeForm.invalid) return;
    this.state.set('pushing');
    this.errorKey.set('');

    const infraBranch = this.infraForm.controls.branch.value;
    const codeBranch = this.codeForm.controls.branch.value;

    localStorage.setItem(this.infraBranchKey, infraBranch);
    localStorage.setItem(this.codeBranchKey, codeBranch);

    try {
      const response: MultiRepoPushResponse = await this.projectService.pushProjectArtifactsToMultiRepo(
        this.data.projectId,
        {
          infra: {
            alias: this.data.infraAlias,
            branchName: infraBranch,
            commitMessage: this.infraForm.controls.commit.value,
          },
          code: {
            alias: this.data.codeAlias,
            branchName: codeBranch,
            commitMessage: this.codeForm.controls.commit.value,
          },
        },
      );

      const infra = response.results.find(r => r.alias === this.data.infraAlias) ?? null;
      const code = response.results.find(r => r.alias === this.data.codeAlias) ?? null;
      this.infraResult.set(infra);
      this.codeResult.set(code);

      const infraOk = infra?.success === true;
      const codeOk = code?.success === true;
      if (infraOk && codeOk) this.state.set('success');
      else if (!infraOk && !codeOk) this.state.set('error');
      else this.state.set('partial');
    } catch {
      this.errorKey.set('PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_TITLE');
      this.state.set('error');
    }
  }

  protected onRetry(): void {
    this.state.set('form');
    this.infraResult.set(null);
    this.codeResult.set(null);
    this.errorKey.set('');
  }

  protected onClose(): void {
    this.dialogRef.close();
  }

  protected async copyCommitSha(sha: string | null): Promise<void> {
    if (!sha) return;
    try {
      await navigator.clipboard.writeText(sha);
      this.snackBar.open(
        this.translate.instant('PROJECT_DETAIL.MULTI_REPO_PUSH.COMMIT_COPIED'),
        this.translate.instant('COMMON.CLOSE'),
        { duration: 2500 },
      );
    } catch {
      // Silent: clipboard may be unavailable in non-secure contexts.
    }
  }
}
