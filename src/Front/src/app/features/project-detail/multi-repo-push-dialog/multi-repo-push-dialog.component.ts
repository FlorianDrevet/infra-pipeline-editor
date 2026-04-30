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
  MultiRepoPushMode,
  MultiRepoPushRequest,
  MultiRepoPushResponse,
  RepoPushResult,
} from '../../../shared/interfaces/multi-repo-push.interface';

export interface MultiRepoPushDialogData {
  projectId: string;
  infraAlias: string;
  codeAlias: string;
  mode?: MultiRepoPushMode;
}

type DialogState = 'form' | 'pushing' | 'success' | 'partial' | 'error';

interface MultiRepoPushModeContent {
  titleIcon: string;
  titleKey: string;
  subtitleKey: string;
  pushActionKey: string;
  pushingKey: string;
  successTitleKey: string;
  errorTitleKey: string;
  errorDescKey: string;
}

const MULTI_REPO_PUSH_MODE_CONTENT: Record<MultiRepoPushMode, MultiRepoPushModeContent> = {
  both: {
    titleIcon: 'cloud_upload',
    titleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.TITLE',
    subtitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.SUBTITLE',
    pushActionKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.CTA_PUSH_BOTH',
    pushingKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.PUSHING',
    successTitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.SUCCESS_TITLE',
    errorTitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_TITLE',
    errorDescKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_DESC',
  },
  infra: {
    titleIcon: 'dns',
    titleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.TITLE_INFRA',
    subtitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.SUBTITLE_INFRA',
    pushActionKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.CTA_PUSH_INFRA',
    pushingKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.PUSHING_INFRA',
    successTitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.SUCCESS_TITLE_INFRA',
    errorTitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_TITLE_INFRA',
    errorDescKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_DESC_INFRA',
  },
  code: {
    titleIcon: 'code',
    titleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.TITLE_CODE',
    subtitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.SUBTITLE_CODE',
    pushActionKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.CTA_PUSH_CODE',
    pushingKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.PUSHING_CODE',
    successTitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.SUCCESS_TITLE_CODE',
    errorTitleKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_TITLE_CODE',
    errorDescKey: 'PROJECT_DETAIL.MULTI_REPO_PUSH.ERROR_DESC_CODE',
  },
};

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

  protected readonly mode = computed<MultiRepoPushMode>(() => this.data.mode ?? 'both');
  protected readonly modeContent = computed(() => MULTI_REPO_PUSH_MODE_CONTENT[this.mode()]);
  protected readonly isBothMode = computed(() => this.mode() === 'both');
  protected readonly showsInfraCard = computed(() => this.mode() !== 'code');
  protected readonly showsCodeCard = computed(() => this.mode() !== 'infra');
  protected readonly titleIcon = computed(() => this.modeContent().titleIcon);
  protected readonly titleKey = computed(() => this.modeContent().titleKey);
  protected readonly subtitleKey = computed(() => this.modeContent().subtitleKey);
  protected readonly pushActionKey = computed(() => this.modeContent().pushActionKey);
  protected readonly pushingKey = computed(() => this.modeContent().pushingKey);
  protected readonly successTitleKey = computed(() => this.modeContent().successTitleKey);
  protected readonly errorTitleKey = computed(() => this.modeContent().errorTitleKey);
  protected readonly errorDescKey = computed(() => this.modeContent().errorDescKey);

  protected readonly canPush = computed(() => {
    const isPushing = this.state() === 'pushing';
    return !isPushing
      && (!this.showsInfraCard() || this.infraForm.valid)
      && (!this.showsCodeCard() || this.codeForm.valid);
  });

  protected async onPush(): Promise<void> {
    if (this.state() === 'pushing') return;
    if (!this.canPush()) return;
    this.state.set('pushing');
    this.errorKey.set('');

    const request: MultiRepoPushRequest = {};
    if (this.showsInfraCard()) {
      const infraBranch = this.infraForm.controls.branch.value;
      localStorage.setItem(this.infraBranchKey, infraBranch);
      request.infra = {
        alias: this.data.infraAlias,
        branchName: infraBranch,
        commitMessage: this.infraForm.controls.commit.value,
      };
    }

    if (this.showsCodeCard()) {
      const codeBranch = this.codeForm.controls.branch.value;
      localStorage.setItem(this.codeBranchKey, codeBranch);
      request.code = {
        alias: this.data.codeAlias,
        branchName: codeBranch,
        commitMessage: this.codeForm.controls.commit.value,
      };
    }

    try {
      const response: MultiRepoPushResponse = await this.projectService.pushProjectArtifactsToMultiRepo(
        this.data.projectId,
        request,
      );

      const infra = response.results.find(r => r.alias === this.data.infraAlias) ?? null;
      const code = response.results.find(r => r.alias === this.data.codeAlias) ?? null;
      this.infraResult.set(infra);
      this.codeResult.set(code);

      const expectsInfra = this.showsInfraCard();
      const expectsCode = this.showsCodeCard();
      const infraOk = !expectsInfra || infra?.success === true;
      const codeOk = !expectsCode || code?.success === true;
      const infraFailed = expectsInfra && infra?.success === false;
      const codeFailed = expectsCode && code?.success === false;

      if (infraOk && codeOk) {
        this.state.set('success');
      } else if (this.isBothMode() && ((infraOk && codeFailed) || (codeOk && infraFailed))) {
        this.state.set('partial');
      } else {
        this.state.set('error');
      }
    } catch {
      this.errorKey.set(this.errorTitleKey());
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
