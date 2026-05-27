import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AxiosError } from 'axios';
import {
  ProjectResponse,
} from '../../../shared/interfaces/project.interface';
import {
  ProjectLayoutPreset,
  ProjectRepositoryResponse,
  RepositoryContentKind,
} from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  RepositoryDialogComponent,
  RepositoryDialogData,
} from './repository-dialog/repository-dialog.component';
import {
  DsButtonComponent,
  DsOptionCardComponent,
} from '../../../shared/components/ds';
import { extractLayoutRepositoriesApiErrorMessage } from './layout-repositories-api-error';

interface PresetOption {
  value: ProjectLayoutPreset;
  labelKey: string;
  descriptionKey: string;
  icon: string;
}

interface RepoSlot {
  readonly kind: RepositoryContentKind;
  readonly labelKey: string;
  readonly repo: ProjectRepositoryResponse | null;
}

const DEFAULT_LAYOUT_PRESET: ProjectLayoutPreset = 'AllInOne';

const LAYOUT_PRESETS: ReadonlyArray<PresetOption> = [
  {
    value: 'AllInOne',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE_DESC',
    icon: 'inventory_2',
  },
  {
    value: 'SplitInfraCode',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE_DESC',
    icon: 'call_split',
  },
  {
    value: 'MultiRepo',
    labelKey: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO',
    descriptionKey: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO_DESC',
    icon: 'hub',
  },
];

function normalizeLayoutPreset(preset?: string): ProjectLayoutPreset {
  if (preset === 'SplitInfraCode' || preset === 'MultiRepo') {
    return preset;
  }

  return DEFAULT_LAYOUT_PRESET;
}

@Component({
  selector: 'app-layout-repositories',
  standalone: true,
  imports: [
    TranslateModule,
    MatDialogModule,
    MatIconModule,
    DsSpinnerComponent,
    DsButtonComponent,
    DsOptionCardComponent,
  ],
  templateUrl: './layout-repositories.component.html',
  styleUrl: './layout-repositories.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutRepositoriesComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  readonly projectId = input.required<string>();
  readonly presetChanged = output<ProjectLayoutPreset>();
  readonly projectChanged = output<ProjectResponse>();

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly presetSaving = signal(false);
  protected readonly repoActionId = signal<string | null>(null);
  protected readonly testingRepoId = signal<string | null>(null);
  protected readonly testResultMap = signal<Record<string, 'success' | 'failure'>>({});
  protected readonly testErrorMap = signal<Record<string, string>>({});
  protected readonly optimisticPreset = signal<ProjectLayoutPreset | null>(null);

  protected readonly layoutPresets = LAYOUT_PRESETS;

  protected readonly currentPreset = computed<ProjectLayoutPreset>(() => {
    const optimisticPreset = this.optimisticPreset();
    if (optimisticPreset) {
      return optimisticPreset;
    }

    return normalizeLayoutPreset(this.project()?.layoutPreset);
  });

  protected readonly repositories = computed<ProjectRepositoryResponse[]>(
    () => this.project()?.repositories ?? [],
  );

  /** Repo currently filling the AllInOne slot (must hold both kinds). */
  protected readonly allInOneRepo = computed<ProjectRepositoryResponse | null>(() => {
    return this.repositories()[0] ?? null;
  });

  /** Slots for the SplitInfraCode preset. */
  protected readonly splitSlots = computed<RepoSlot[]>(() => {
    const repos = this.repositories();
    return [
      {
        kind: 'Infrastructure',
        labelKey: 'PROJECT_DETAIL.LAYOUT.SLOT_INFRASTRUCTURE',
        repo: repos.find((r) => r.contentKinds.includes('Infrastructure')) ?? null,
      },
      {
        kind: 'ApplicationCode',
        labelKey: 'PROJECT_DETAIL.LAYOUT.SLOT_APPLICATION_CODE',
        repo: repos.find((r) => r.contentKinds.includes('ApplicationCode')) ?? null,
      },
    ];
  });

  ngOnInit(): void {
    void this.load();
  }

  private async load(emitProjectChanged = false): Promise<void> {
    this.isLoading.set(true);
    try {
      const project = await this.projectService.getProject(this.projectId());
      this.project.set(project);
      if (emitProjectChanged) {
        this.projectChanged.emit(project);
      }
    } catch {
      this.showError('PROJECT_DETAIL.LAYOUT.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  private async refreshProjectState(): Promise<void> {
    this.projectService.invalidateProjectCache(this.projectId());
    await this.load(true);
  }

  protected async onPresetChange(preset: ProjectLayoutPreset): Promise<void> {
    if (preset === this.currentPreset()) return;

    const previousPreset = this.currentPreset();

    this.optimisticPreset.set(preset);
    this.presetChanged.emit(preset);
    this.presetSaving.set(true);

    try {
      await this.projectService.setLayoutPreset(this.projectId(), preset);

      this.project.update((project) => {
        if (!project) {
          return project;
        }

        return {
          ...project,
          layoutPreset: preset,
        };
      });

      await this.refreshProjectState();
    } catch (error) {
      this.presetChanged.emit(previousPreset);
      this.showError(this.mapError(error, 'PROJECT_DETAIL.LAYOUT.PRESET_ERROR'));
    } finally {
      this.optimisticPreset.set(null);
      this.presetSaving.set(false);
    }
  }

  protected openAllInOneDialog(): void {
    const existing = this.allInOneRepo();
    const data: RepositoryDialogData = {
      projectId: this.projectId(),
      mode: existing ? 'edit' : 'create',
      existing: existing ?? undefined,
      lockedKinds: ['Infrastructure', 'ApplicationCode'],
    };
    const ref = this.dialog.open(RepositoryDialogComponent, { data, width: '560px' });
    ref.afterClosed().subscribe(async (result) => {
      if (result) {
        await this.refreshProjectState();
      }
    });
  }

  protected openSlotDialog(slot: RepoSlot): void {
    const data: RepositoryDialogData = {
      projectId: this.projectId(),
      mode: slot.repo ? 'edit' : 'create',
      existing: slot.repo ?? undefined,
      lockedKinds: [slot.kind],
    };
    const ref = this.dialog.open(RepositoryDialogComponent, { data, width: '560px' });
    ref.afterClosed().subscribe(async (result) => {
      if (result) {
        await this.refreshProjectState();
      }
    });
  }

  protected openRemoveRepoDialog(repo: ProjectRepositoryResponse): void {
    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_MESSAGE',
      messageParams: { repositoryName: this.repositoryDisplayName(repo) },
      confirmKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.LAYOUT.DELETE_CONFIRM_CANCEL',
    };
    const ref = this.dialog.open(ConfirmDialogComponent, { data });
    ref.afterClosed().subscribe(async (confirmed) => {
      if (!confirmed) return;
      this.repoActionId.set(repo.id);
      try {
        await this.projectService.removeRepository(this.projectId(), repo.id);
        await this.refreshProjectState();
      } catch (error) {
        if (this.isConflict(error)) {
          this.showError('PROJECT_DETAIL.LAYOUT.REPO_DELETE_IN_USE');
        } else {
          this.showError('PROJECT_DETAIL.LAYOUT.DELETE_ERROR');
        }
      } finally {
        this.repoActionId.set(null);
      }
    });
  }

  protected async testRepositoryConnection(repo: ProjectRepositoryResponse): Promise<void> {
    this.repoActionId.set(repo.id);
    this.testingRepoId.set(repo.id);

    // Clear previous result for this repo
    this.testResultMap.update((map) => {
      const next = { ...map };
      delete next[repo.id];
      return next;
    });
    this.testErrorMap.update((map) => {
      const next = { ...map };
      delete next[repo.id];
      return next;
    });

    try {
      const response = await this.projectService.testRepositoryConnection(this.projectId(), repo.id);

      if (response.success) {
        this.testResultMap.update((map) => ({ ...map, [repo.id]: 'success' }));
      } else {
        this.testResultMap.update((map) => ({ ...map, [repo.id]: 'failure' }));
        if (response.errorMessage) {
          this.testErrorMap.update((map) => ({ ...map, [repo.id]: response.errorMessage! }));
        }
      }
    } catch (error) {
      this.testResultMap.update((map) => ({ ...map, [repo.id]: 'failure' }));
      const errorMessage = this.extractConnectionErrorMessage(error);
      if (errorMessage) {
        this.testErrorMap.update((map) => ({ ...map, [repo.id]: errorMessage }));
      }
    } finally {
      this.testingRepoId.set(null);
      this.repoActionId.set(null);
    }
  }

  protected repositoryDisplayName(repo: ProjectRepositoryResponse): string {
    if (repo.owner && repo.repositoryName) {
      return `${repo.owner}/${repo.repositoryName}`;
    }

    return repo.repositoryName ?? repo.repositoryUrl ?? repo.id;
  }

  protected repositoryProviderLabel(repo: ProjectRepositoryResponse): string {
    return repo.providerType ?? this.translate.instant('PROJECT_DETAIL.LAYOUT.REPOSITORY_NOT_CONFIGURED');
  }

  private isConflict(error: unknown): boolean {
    return error instanceof AxiosError && error.response?.status === 409;
  }

  private extractConnectionErrorMessage(error: unknown): string | null {
    return extractLayoutRepositoriesApiErrorMessage(
      error,
      this.translate.instant('PROJECT_DETAIL.LAYOUT.AUTH.CONNECTION_ERROR'),
    );
  }

  private mapError(error: unknown, fallbackKey: string): string {
    if (error instanceof AxiosError && error.response?.status === 400) {
      return 'PROJECT_DETAIL.LAYOUT.INVALID_VALUE';
    }
    return fallbackKey;
  }

  private showError(key: string): void {
    const message = this.translate.instant(key);
    this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), {
      duration: 5000,
      panelClass: 'error-snackbar',
    });
  }
}
