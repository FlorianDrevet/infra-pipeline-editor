import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  DsAutocompleteComponent,
  DsAutocompleteOption,
  DsButtonComponent,
  DsTextareaComponent,
} from '../../../shared/components/ds';
import { MatCardModule } from '@angular/material/card';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { startWith } from 'rxjs';
import { ProjectService } from '../../../shared/services/project.service';
import {
  MultiRepoPushMode,
  MultiRepoPushRequest,
  MultiRepoPushResponse,
  RepoPushResult,
} from '../../../shared/interfaces/multi-repo-push.interface';

export interface MultiRepoPushDialogData {
  projectId: string;
  infraRepositoryId: string;
  codeRepositoryId: string;
  infraRepositoryLabel: string;
  codeRepositoryLabel: string;
  mode?: MultiRepoPushMode;
}

type DialogState = 'form' | 'pushing' | 'success' | 'partial' | 'error';

const DEFAULT_GIT_BRANCH_NAME = 'main';

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
 * Branch suggestions are loaded with the project-scoped branch endpoint and reused for both
 * repo cards. Each repo may have a different remote state, but the API currently exposes a
 * single branch list at project scope.
 */
@Component({
  selector: 'app-multi-repo-push-dialog',
  standalone: true,
  imports: [
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    DsSpinnerComponent,
    MatTooltipModule,
    ReactiveFormsModule,
    TranslateModule,
    DsAutocompleteComponent,
    DsButtonComponent,
    DsTextareaComponent,
  ],
  templateUrl: './multi-repo-push-dialog.component.html',
  styleUrl: './multi-repo-push-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MultiRepoPushDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<MultiRepoPushDialogComponent>);
  protected readonly data: MultiRepoPushDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  protected readonly state = signal<DialogState>('form');
  protected readonly infraResult = signal<RepoPushResult | null>(null);
  protected readonly codeResult = signal<RepoPushResult | null>(null);
  protected readonly errorKey = signal('');
  protected readonly allInfraBranches = signal<string[]>([]);
  protected readonly allCodeBranches = signal<string[]>([]);
  protected readonly filteredInfraBranches = signal<string[]>([]);
  protected readonly filteredCodeBranches = signal<string[]>([]);
  protected readonly branchesLoading = signal(true);
  protected readonly infraBranchOptions = computed<DsAutocompleteOption<string>[]>(() =>
    this.filteredInfraBranches().map((branch) => ({ value: branch, label: branch })),
  );
  protected readonly codeBranchOptions = computed<DsAutocompleteOption<string>[]>(() =>
    this.filteredCodeBranches().map((branch) => ({ value: branch, label: branch })),
  );

  private readonly infraBranchKey = `ifs-push-branch-multi-${this.data.projectId}-${this.data.infraRepositoryId}`;
  private readonly codeBranchKey = `ifs-push-branch-multi-${this.data.projectId}-${this.data.codeRepositoryId}`;

  protected readonly infraForm = new FormGroup({
    branch: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    commit: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected readonly codeForm = new FormGroup({
    branch: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    commit: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
  });
  private readonly infraFormStatus = toSignal(
    this.infraForm.statusChanges.pipe(startWith(this.infraForm.status)),
    { initialValue: this.infraForm.status },
  );
  private readonly codeFormStatus = toSignal(
    this.codeForm.statusChanges.pipe(startWith(this.codeForm.status)),
    { initialValue: this.codeForm.status },
  );

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
    const areBranchesLoading = this.branchesLoading();
    const infraFormStatus = this.infraFormStatus();
    const codeFormStatus = this.codeFormStatus();

    return !isPushing
      && !areBranchesLoading
      && (!this.showsInfraCard() || infraFormStatus === 'VALID')
      && (!this.showsCodeCard() || codeFormStatus === 'VALID');
  });

  public constructor() {
    this.infraForm.controls.branch.valueChanges
      .pipe(startWith(this.infraForm.controls.branch.value), takeUntilDestroyed())
      .subscribe(value => {
        this.filterInfraBranches(value ?? '');
      });
    this.codeForm.controls.branch.valueChanges
      .pipe(startWith(this.codeForm.controls.branch.value), takeUntilDestroyed())
      .subscribe(value => {
        this.filterCodeBranches(value ?? '');
      });
  }

  public ngOnInit(): void {
    void this.loadBranches();
  }

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
        repositoryId: this.data.infraRepositoryId,
        branchName: infraBranch,
        commitMessage: this.infraForm.controls.commit.value,
      };
    }

    if (this.showsCodeCard()) {
      const codeBranch = this.codeForm.controls.branch.value;
      localStorage.setItem(this.codeBranchKey, codeBranch);
      request.code = {
        repositoryId: this.data.codeRepositoryId,
        branchName: codeBranch,
        commitMessage: this.codeForm.controls.commit.value,
      };
    }

    try {
      const response: MultiRepoPushResponse = await this.projectService.pushProjectArtifactsToMultiRepo(
        this.data.projectId,
        request,
      );

      const infra = response.results.find(r => r.repositoryId === this.data.infraRepositoryId) ?? null;
      const code = response.results.find(r => r.repositoryId === this.data.codeRepositoryId) ?? null;
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

  protected showAllInfraBranches(): void {
    this.filterInfraBranches('');
  }

  protected showAllCodeBranches(): void {
    this.filterCodeBranches('');
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

  private async loadBranches(): Promise<void> {
    this.branchesLoading.set(true);
    try {
      const [infraBranches, codeBranches] = await Promise.all([
        this.showsInfraCard()
          ? this.projectService.listBranches(this.data.projectId)
          : Promise.resolve([]),
        this.showsCodeCard()
          ? this.projectService.listCodeBranches(this.data.projectId)
          : Promise.resolve([]),
      ]);
      this.allInfraBranches.set(infraBranches.map(b => b.name));
      this.allCodeBranches.set(codeBranches.map(b => b.name));
      this.applyPreferredBranches();
    } catch {
      this.allInfraBranches.set([]);
      this.allCodeBranches.set([]);
      this.filteredInfraBranches.set([]);
      this.filteredCodeBranches.set([]);
    } finally {
      this.branchesLoading.set(false);
    }
  }

  private applyPreferredBranches(): void {
    this.infraForm.controls.branch.setValue(this.readPreferredBranch(this.infraBranchKey));
    this.codeForm.controls.branch.setValue(this.readPreferredBranch(this.codeBranchKey));
  }

  private readPreferredBranch(storageKey: string): string {
    return localStorage.getItem(storageKey) ?? DEFAULT_GIT_BRANCH_NAME;
  }

  private filterInfraBranches(search: string): void {
    this.filteredInfraBranches.set(this.filterBranchesFrom(this.allInfraBranches(), search));
  }

  private filterCodeBranches(search: string): void {
    this.filteredCodeBranches.set(this.filterBranchesFrom(this.allCodeBranches(), search));
  }

  private filterBranchesFrom(branches: string[], search: string): string[] {
    const normalizedSearch = search.trim().toLowerCase();
    if (normalizedSearch === '') {
      return branches;
    }

    return branches.filter(branch => branch.toLowerCase().includes(normalizedSearch));
  }
}
