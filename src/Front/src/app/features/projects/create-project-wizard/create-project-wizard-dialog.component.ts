import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnInit,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  DsAlertComponent,
  DsButtonComponent,
  DsIconButtonComponent,
} from '../../../shared/components/ds';
import {
  CreateProjectWithSetupRequest,
  EnvironmentSetupRequest,
  ProjectResponse,
  RepositorySetupRequest,
} from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  CreateProjectWizardDraft,
  EMPTY_DRAFT,
  EnvironmentDraft,
  LayoutPreset,
  RepositoryDraft,
  createEmptyEnvironment,
  createEmptyRepository,
  isDraftMeaningful,
} from './create-project-wizard.types';
import { CreateProjectWizardDraftService } from './create-project-wizard-draft.service';
import { EnvironmentStepComponent } from './steps/environment-step.component';
import { IdentityStepComponent } from './steps/identity-step.component';
import { LayoutStepComponent } from './steps/layout-step.component';
import { RepositoriesStepComponent } from './steps/repositories-step.component';
import { ReviewStepComponent } from './steps/review-step.component';

const STEP_IDENTITY = 0;
const STEP_LAYOUT = 1;
const STEP_ENVIRONMENTS = 2;
const STEP_REPOSITORIES = 3;
// Review step index = 3 when MultiRepo (no repositories step), 4 otherwise.

/**
 * Multi-step "Create project" wizard. Uses a Material stepper, persists the in-flight draft
 * to sessionStorage, and finally calls POST /projects/with-setup on submit.
 */
@Component({
  selector: 'app-create-project-wizard-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    MatStepperModule,
    TranslateModule,
    DsAlertComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    IdentityStepComponent,
    LayoutStepComponent,
    EnvironmentStepComponent,
    RepositoriesStepComponent,
    ReviewStepComponent,
  ],
  templateUrl: './create-project-wizard-dialog.component.html',
  styleUrl: './create-project-wizard-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateProjectWizardDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<CreateProjectWizardDialogComponent, ProjectResponse>);
  private readonly draftService = inject(CreateProjectWizardDraftService);
  private readonly projectService = inject(ProjectService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  protected readonly draft = signal<CreateProjectWizardDraft>({ ...EMPTY_DRAFT });
  protected readonly isSubmitting = signal(false);
  protected readonly submitErrorKey = signal<string>('');
  protected readonly submitErrorText = signal<string>('');

  protected readonly identityValid = signal(false);
  protected readonly layoutValid = signal(false);
  protected readonly environmentsValid = signal(false);
  protected readonly repositoriesValid = signal(false);

  protected readonly resumePromptVisible = signal(false);
  protected readonly abandonPromptVisible = signal(false);

  protected readonly stepperRef = viewChild<MatStepper>('stepper');
  protected readonly activeStepIndex = signal(0);

  private saveTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly needsRepositoriesStep = computed(
    () => this.draft().layoutPreset !== '' && this.draft().layoutPreset !== 'MultiRepo',
  );

  protected readonly reviewStepIndex = computed(() => (this.needsRepositoriesStep() ? 4 : 3));

  protected readonly isCurrentStepValid = computed(() => {
    switch (this.activeStepIndex()) {
      case STEP_IDENTITY:
        return this.identityValid();
      case STEP_LAYOUT:
        return this.layoutValid();
      case STEP_ENVIRONMENTS:
        return this.environmentsValid();
      case STEP_REPOSITORIES:
        return this.repositoriesValid();
      default:
        return true;
    }
  });

  public constructor() {
    // Persist the draft to sessionStorage 300ms after the last change.
    effect(() => {
      const snapshot = this.draft();
      if (this.saveTimer) {
        clearTimeout(this.saveTimer);
      }
      // Avoid persisting an empty draft (e.g. just after clear()).
      if (!isDraftMeaningful(snapshot)) {
        return;
      }
      this.saveTimer = setTimeout(() => this.draftService.save(snapshot), 300);
    });
  }

  public ngOnInit(): void {
    const existing = this.draftService.load();
    if (existing && existing.name.trim().length > 0) {
      // Show resume banner without overwriting the user's draft until they decide.
      this.resumePromptVisible.set(true);
      this.draft.set(existing);
    }
    this.dialogRef.disableClose = true;
  }

  // ─── Resume / discard ─────────────────────────────────────────────────────

  protected resumeDraft(): void {
    this.resumePromptVisible.set(false);
  }

  protected discardDraft(): void {
    this.draftService.clear();
    this.draft.set({ ...EMPTY_DRAFT });
    this.resumePromptVisible.set(false);
    this.identityValid.set(false);
    this.layoutValid.set(false);
    this.environmentsValid.set(false);
    this.repositoriesValid.set(false);
    this.stepperRef()?.reset();
  }

  // ─── Stepper validity ─────────────────────────────────────────────────────

  protected onIdentityValidity(valid: boolean): void {
    this.identityValid.set(valid);
  }

  protected onLayoutValidity(valid: boolean): void {
    this.layoutValid.set(valid);
  }

  protected onEnvironmentsValidity(valid: boolean): void {
    this.environmentsValid.set(valid);
  }

  protected onRepositoriesValidity(valid: boolean): void {
    this.repositoriesValid.set(valid);
  }

  // ─── Stepper navigation ─────────────────────────────────────────────────

  protected onStepChange(event: StepperSelectionEvent): void {
    this.activeStepIndex.set(event.selectedIndex);
  }

  protected goNext(): void {
    this.stepperRef()?.next();
  }

  protected goPrevious(): void {
    this.stepperRef()?.previous();
  }

  // ─── Quick start ──────────────────────────────────────────────────────────

  protected onQuickStart(): void {
    this.draft.update((d) => {
      const env: EnvironmentDraft = {
        ...createEmptyEnvironment(0),
        name: 'Development',
        shortName: 'dev',
      };
      const repo: RepositoryDraft = {
        ...createEmptyRepository(['Infrastructure', 'ApplicationCode']),
        alias: 'main-repo',
      };
      return {
        ...d,
        layoutPreset: 'AllInOne' as LayoutPreset,
        environments: [env],
        repositories: [repo],
      };
    });
    // Mark all intermediate steps as valid so the stepper can advance.
    this.layoutValid.set(true);
    this.environmentsValid.set(true);
    this.repositoriesValid.set(true);
    queueMicrotask(() => {
      const stepper = this.stepperRef();
      if (stepper) {
        stepper.selectedIndex = this.reviewStepIndex();
      }
    });
  }

  // ─── Submit ───────────────────────────────────────────────────────────────

  protected async onSubmit(): Promise<void> {
    if (this.isSubmitting()) {
      return;
    }
    if (!this.canSubmit()) {
      return;
    }
    this.isSubmitting.set(true);
    this.submitErrorKey.set('');
    this.submitErrorText.set('');

    try {
      const request = this.toRequest(this.draft());
      const created = await this.projectService.createProjectWithSetup(request);
      this.draftService.clear();
      this.snackBar.open(this.translate.instant('PROJECT_CREATE.SUCCESS_SNACKBAR'), '', {
        duration: 3000,
      });
      this.dialogRef.close(created);
    } catch (error) {
      const message = this.extractErrorMessage(error);
      this.submitErrorText.set(message);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected canSubmit(): boolean {
    if (!this.identityValid() || !this.layoutValid() || !this.environmentsValid()) {
      return false;
    }
    if (this.needsRepositoriesStep() && !this.repositoriesValid()) {
      return false;
    }
    return true;
  }

  // ─── Cancel / abandon ─────────────────────────────────────────────────────

  @HostListener('keydown.escape', ['$event'])
  protected onEscape(event: Event): void {
    event.preventDefault();
    this.attemptCancel();
  }

  protected attemptCancel(): void {
    if (isDraftMeaningful(this.draft())) {
      this.abandonPromptVisible.set(true);
      return;
    }
    this.dialogRef.close();
  }

  protected confirmAbandon(): void {
    this.draftService.clear();
    this.abandonPromptVisible.set(false);
    this.dialogRef.close();
  }

  protected keepDraftAndClose(): void {
    // Keep the draft in sessionStorage and close.
    this.abandonPromptVisible.set(false);
    this.dialogRef.close();
  }

  protected dismissAbandonPrompt(): void {
    this.abandonPromptVisible.set(false);
  }

  // ─── Mapping ──────────────────────────────────────────────────────────────

  private toRequest(draft: CreateProjectWizardDraft): CreateProjectWithSetupRequest {
    const environments: EnvironmentSetupRequest[] = draft.environments.map((env) => ({
      name: env.name.trim(),
      shortName: env.shortName.trim(),
      prefix: env.prefix.trim() ? env.prefix.trim() : undefined,
      suffix: env.suffix.trim() ? env.suffix.trim() : undefined,
      location: env.location,
      subscriptionId: env.subscriptionId.trim() ? env.subscriptionId.trim() : undefined,
      order: env.order,
      requiresApproval: env.requiresApproval,
    }));

    const repositories: RepositorySetupRequest[] = draft.repositories.map((repo) => {
      const provider = repo.providerType ? repo.providerType : undefined;
      const url = repo.repositoryUrl.trim() ? repo.repositoryUrl.trim() : undefined;
      const branch = repo.defaultBranch.trim() ? repo.defaultBranch.trim() : undefined;
      return {
        alias: repo.alias.trim(),
        contentKinds: repo.contentKinds,
        providerType: provider,
        repositoryUrl: url,
        defaultBranch: branch,
      };
    });

    return {
      name: draft.name.trim(),
      description: draft.description.trim() ? draft.description.trim() : undefined,
      layoutPreset: draft.layoutPreset as LayoutPreset,
      environments,
      repositories,
    };
  }

  private extractErrorMessage(error: unknown): string {
    if (typeof error !== 'object' || error === null) {
      return this.translate.instant('PROJECT_CREATE.ERROR.GENERIC');
    }
    const maybe = error as {
      response?: {
        data?: {
          title?: string;
          detail?: string;
          errors?: Record<string, string[]>;
        };
      };
      message?: string;
    };
    const details = maybe.response?.data;
    if (details?.errors) {
      const messages = Object.values(details.errors).flat();
      if (messages.length > 0 && messages[0]) {
        return messages[0];
      }
    }
    return (
      details?.detail ||
      details?.title ||
      maybe.message ||
      this.translate.instant('PROJECT_CREATE.ERROR.GENERIC')
    );
  }
}
