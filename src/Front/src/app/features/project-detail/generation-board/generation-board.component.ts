import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import {
  ProjectLayoutPreset,
  ProjectRepositoryResponse,
  RepositoryContentKind,
} from '../../../shared/interfaces/project-repository.interface';
import { ProjectService } from '../../../shared/services/project.service';
import {
  DsButtonComponent,
  DsCardComponent,
  DsChipComponent,
  DsEmptyStateComponent,
  DsPageHeaderComponent,
  DsSectionHeaderComponent,
} from '../../../shared/components/ds';
import {
  PushToGitDialogComponent,
  PushToGitDialogData,
} from '../../config-detail/push-to-git-dialog/push-to-git-dialog.component';
import {
  MultiRepoPushDialogComponent,
  MultiRepoPushDialogData,
} from '../multi-repo-push-dialog/multi-repo-push-dialog.component';

type BoardTopology = 'single' | 'split' | 'mixed' | 'empty' | 'split-infra-code';

interface AliasGroup {
  readonly alias: string;
  readonly repo: ProjectRepositoryResponse | null;
  readonly configs: readonly InfrastructureConfigResponse[];
  readonly resourceGroupCount: number;
  readonly resourceCount: number;
}

const DEFAULT_ALIAS = 'default';
const ALL_IN_ONE_LAYOUT: ProjectLayoutPreset = 'AllInOne';
const MULTI_REPO_LAYOUT: ProjectLayoutPreset = 'MultiRepo';
const SPLIT_INFRA_CODE_LAYOUT: ProjectLayoutPreset = 'SplitInfraCode';
const INFRASTRUCTURE_CONTENT_KIND: RepositoryContentKind = 'Infrastructure';
const APPLICATION_CODE_CONTENT_KIND: RepositoryContentKind = 'ApplicationCode';

const LAYOUT_PRESET_LABEL_KEYS: Record<ProjectLayoutPreset, string> = {
  [ALL_IN_ONE_LAYOUT]: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE',
  [MULTI_REPO_LAYOUT]: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO',
  [SPLIT_INFRA_CODE_LAYOUT]: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE',
};

function isProjectLayoutPreset(value: string | null | undefined): value is ProjectLayoutPreset {
  return value === ALL_IN_ONE_LAYOUT || value === MULTI_REPO_LAYOUT || value === SPLIT_INFRA_CODE_LAYOUT;
}

@Component({
  selector: 'app-generation-board',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    DsButtonComponent,
    DsCardComponent,
    DsChipComponent,
    DsEmptyStateComponent,
    DsPageHeaderComponent,
    DsSectionHeaderComponent,
  ],
  templateUrl: './generation-board.component.html',
  styleUrl: './generation-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenerationBoardComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly projectId = input<string>();

  private resolvedProjectId(): string {
    return this.projectId() || this.route.snapshot.paramMap.get('id') || '';
  }

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(true);

  protected readonly groupedByAlias = computed<AliasGroup[]>(() => {
    const configs = this.configs();
    const project = this.project();
    const repos = project?.repositories ?? [];
    const repoByAlias = new Map(repos.map((r) => [r.alias, r]));
    const isMultiRepo = project?.layoutPreset === MULTI_REPO_LAYOUT;

    const byAlias = new Map<string, InfrastructureConfigResponse[]>();
    for (const config of configs) {
      // MultiRepo: bucket by the first config-level repo alias (or fallback to config name).
      // Mono-repo layouts: all configs share project-level repos, bucket by first project repo alias.
      const alias = isMultiRepo
        ? (config.repositories?.[0]?.alias ?? config.name ?? DEFAULT_ALIAS)
        : (repos[0]?.alias ?? DEFAULT_ALIAS);
      const bucket = byAlias.get(alias);
      if (bucket) {
        bucket.push(config);
      } else {
        byAlias.set(alias, [config]);
      }
    }

    return Array.from(byAlias.entries())
      .map<AliasGroup>(([alias, items]) => {
        const resourceGroupCount = items.reduce((total, item) => total + item.resourceGroupCount, 0);
        const resourceCount = items.reduce((total, item) => total + item.resourceCount, 0);

        return {
          alias,
          repo: repoByAlias.get(alias)
            ?? (isMultiRepo
              ? (items[0]?.repositories?.find((r) => r.alias === alias) ?? null)
              : null),
          configs: items,
          resourceGroupCount,
          resourceCount,
        };
      })
      .sort((a, b) => a.alias.localeCompare(b.alias));
  });

  protected readonly topology = computed<BoardTopology>(() => {
    const project = this.project();
    if (project?.layoutPreset === SPLIT_INFRA_CODE_LAYOUT) return 'split-infra-code';
    const groups = this.groupedByAlias();
    if (groups.length === 0) return 'empty';
    if (groups.length === 1) return 'single';
    return groups.every((g) => g.configs.length === 1) ? 'split' : 'mixed';
  });

  protected readonly hasProjectLevelRepositories = computed(() => (this.project()?.repositories?.length ?? 0) > 0);

  protected readonly canGenerateProject = computed(() => this.canGenerateAll() && this.hasProjectLevelRepositories());

  protected readonly repositoryCount = computed(() => Math.max(
    this.groupedByAlias().length,
    this.project()?.repositories?.length ?? 0,
  ));

  protected readonly totalConfigCount = computed(() => this.configs().length);

  protected readonly totalResourceGroupCount = computed(() => this.configs()
    .reduce((total, config) => total + config.resourceGroupCount, 0));

  protected readonly totalResourceCount = computed(() => this.configs()
    .reduce((total, config) => total + config.resourceCount, 0));

  protected readonly layoutPresetLabelKey = computed(() => {
    const layoutPreset = this.project()?.layoutPreset;
    if (!isProjectLayoutPreset(layoutPreset)) {
      return 'PROJECT_DETAIL.BOARD.LAYOUT_PRESET_UNKNOWN';
    }

    return LAYOUT_PRESET_LABEL_KEYS[layoutPreset];
  });

  protected readonly canGenerateAll = computed(() => {
    const t = this.topology();
    return t === 'single' || t === 'empty' || t === 'split-infra-code';
  });

  ngOnInit(): void {
    void this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const id = this.resolvedProjectId();
      const [project, configs] = await Promise.all([
        this.projectService.getProject(id),
        this.projectService.getProjectConfigs(id),
      ]);
      this.project.set(project);
      this.configs.set(configs);
    } catch {
      this.showError('PROJECT_DETAIL.BOARD.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onGenerateAll(): void {
    const project = this.project();
    const hasRepositories = this.hasProjectLevelRepositories();
    if (!project || !hasRepositories) {
      this.showError('PROJECT_DETAIL.BOARD.NO_GIT_CONFIG');
      return;
    }

    if (project.layoutPreset === SPLIT_INFRA_CODE_LAYOUT) {
      const repos = project.repositories ?? [];
      const infraRepo = repos.find((r) => r.contentKinds?.includes(INFRASTRUCTURE_CONTENT_KIND));
      const codeRepo = repos.find((r) => r.contentKinds?.includes(APPLICATION_CODE_CONTENT_KIND));
      if (!infraRepo || !codeRepo) {
        this.showError('PROJECT_DETAIL.MULTI_REPO_PUSH.MISSING_SLOTS');
        return;
      }
      const data: MultiRepoPushDialogData = {
        projectId: project.id,
        infraAlias: infraRepo.alias,
        codeAlias: codeRepo.alias,
      };
      this.dialog.open(MultiRepoPushDialogComponent, {
        width: '68rem',
        maxWidth: '96vw',
        panelClass: 'ifs-multi-repo-push-dialog',
        data,
      });
      return;
    }

    const data: PushToGitDialogData = {
      configId: '',
      projectId: project.id,
      isProjectLevel: true,
      isCombinedProjectPush: true,
    };
    this.dialog.open(PushToGitDialogComponent, { width: '480px', data });
  }

  protected openProjectDetail(): void {
    const projectId = this.project()?.id || this.resolvedProjectId();
    if (!projectId) {
      return;
    }

    void this.router.navigate(['/projects', projectId]);
  }

  protected openConfiguration(configId: string): void {
    void this.router.navigate(['/config', configId]);
  }

  private showError(key: string): void {
    const message = this.translate.instant(key);
    this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), {
      duration: 5000,
      panelClass: 'error-snackbar',
    });
  }
}
