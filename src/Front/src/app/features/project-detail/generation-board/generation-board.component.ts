import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  Signal,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { SidebarContextService } from '../../../core/layouts/sidebar/sidebar-context.service';
import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  ProjectResponse,
} from '../../../shared/interfaces/project.interface';
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
  DsEmptyStateComponent,
  DsPanelActionButtonComponent,
  DsPageHeaderComponent,
} from '../../../shared/components/ds';
import {
  BicepTreeNode,
  BicepFilePanelComponent,
} from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';

import { SplitGenerationSwitcherComponent } from '../split-generation-switcher/split-generation-switcher.component';
import { ProjectDetailGenerationWorkflowService } from '../project-detail-generation-workflow.service';

type BoardTopology = 'single' | 'split' | 'mixed' | 'empty' | 'split-infra-code';
type MonoRepoTabId = 'bicep' | 'pipeline' | 'bootstrap';
type MonoRepoArtifactResult =
  | GenerateProjectBicepResponse
  | GenerateProjectPipelineResponse
  | GenerateProjectBootstrapPipelineResponse;
type MonoRepoLoadFile = (uri: string) => Promise<string>;

type RepositoryMetricLabelKey =
  | 'PROJECT_DETAIL.BOARD.SUMMARY_CONFIGS'
  | 'PROJECT_DETAIL.BOARD.SUMMARY_RESOURCE_GROUPS'
  | 'PROJECT_DETAIL.BOARD.SUMMARY_RESOURCES'
  | 'PROJECT_DETAIL.BOARD.SUMMARY_APPLICATIONS'
  | 'PROJECT_DETAIL.BOARD.SUMMARY_PIPELINE_SCOPES'
  | 'PROJECT_DETAIL.BOARD.SUMMARY_SHARED_PIPELINES';

interface RepositoryMetric {
  readonly labelKey: RepositoryMetricLabelKey;
  readonly value: number;
}

interface ConfigSummary {
  readonly resourceGroupCount: number;
  readonly resourceCount: number;
  readonly sharedPipelineCount: number;
}

interface AliasGroup {
  readonly alias: string;
  readonly repo: ProjectRepositoryResponse | null;
  readonly configs: readonly InfrastructureConfigResponse[];
  readonly resourceGroupCount: number;
  readonly resourceCount: number;
  readonly metrics: readonly RepositoryMetric[];
}

interface MonoRepoTabDefinition {
  readonly id: MonoRepoTabId;
  readonly tabIcon: string;
  readonly tabLabelKey: string;
  readonly terminalTitle: string;
  readonly generatingLabelKey: string;
  readonly downloadLabelKey: string;
  readonly downloadBusyLabelKey: string;
  readonly retryLabelKey: string;
  readonly artifactsLabelKey: string;
  readonly terminalDoneKey: string;
  readonly fileLoadingKey: string;
  readonly fileErrorKey: string;
}

interface MonoRepoTab extends MonoRepoTabDefinition {
  readonly showBootstrapGuide: boolean;
  readonly result: Signal<MonoRepoArtifactResult | null>;
  readonly nodes: Signal<BicepTreeNode[]>;
  readonly isLoading: Signal<boolean>;
  readonly isDownloading: Signal<boolean>;
  readonly errorKey: Signal<string>;
  readonly loadFile: MonoRepoLoadFile;
  readonly download: () => Promise<void>;
  readonly retry: () => Promise<void>;
}

const DEFAULT_ALIAS = 'default';
const ALL_IN_ONE_LAYOUT: ProjectLayoutPreset = 'AllInOne';
const MULTI_REPO_LAYOUT: ProjectLayoutPreset = 'MultiRepo';
const SPLIT_INFRA_CODE_LAYOUT: ProjectLayoutPreset = 'SplitInfraCode';
const INFRASTRUCTURE_CONTENT_KIND: RepositoryContentKind = 'Infrastructure';
const APPLICATION_CODE_CONTENT_KIND: RepositoryContentKind = 'ApplicationCode';
const COMBINED_APP_PIPELINE_MODE = 'Combined';

const REPOSITORY_METRIC_LABEL_KEYS = {
  configs: 'PROJECT_DETAIL.BOARD.SUMMARY_CONFIGS',
  resourceGroups: 'PROJECT_DETAIL.BOARD.SUMMARY_RESOURCE_GROUPS',
  resources: 'PROJECT_DETAIL.BOARD.SUMMARY_RESOURCES',
  applications: 'PROJECT_DETAIL.BOARD.SUMMARY_APPLICATIONS',
  pipelineScopes: 'PROJECT_DETAIL.BOARD.SUMMARY_PIPELINE_SCOPES',
  sharedPipelines: 'PROJECT_DETAIL.BOARD.SUMMARY_SHARED_PIPELINES',
} as const satisfies Record<string, RepositoryMetricLabelKey>;

const LAYOUT_PRESET_LABEL_KEYS: Record<ProjectLayoutPreset, string> = {
  [ALL_IN_ONE_LAYOUT]: 'PROJECT_DETAIL.LAYOUT.PRESET_ALL_IN_ONE',
  [MULTI_REPO_LAYOUT]: 'PROJECT_DETAIL.LAYOUT.PRESET_MULTI_REPO',
  [SPLIT_INFRA_CODE_LAYOUT]: 'PROJECT_DETAIL.LAYOUT.PRESET_SPLIT_INFRA_CODE',
};

const MONO_REPO_TAB_DEFINITIONS = [
  {
    id: 'bicep',
    tabIcon: 'terminal',
    tabLabelKey: 'PROJECT_DETAIL.GENERATION.TAB_BICEP',
    terminalTitle: 'bicep-generator',
    generatingLabelKey: 'PROJECT_DETAIL.BICEP.TERMINAL_GENERATING',
    downloadLabelKey: 'PROJECT_DETAIL.BICEP.DOWNLOAD',
    downloadBusyLabelKey: 'PROJECT_DETAIL.BICEP.DOWNLOADING',
    retryLabelKey: 'PROJECT_DETAIL.BICEP.RETRY',
    artifactsLabelKey: 'PROJECT_DETAIL.BICEP.ARTIFACTS',
    terminalDoneKey: 'PROJECT_DETAIL.BICEP.TERMINAL_DONE',
    fileLoadingKey: 'PROJECT_DETAIL.BICEP.FILE_LOADING',
    fileErrorKey: 'PROJECT_DETAIL.BICEP.FILE_ERROR',
  },
  {
    id: 'pipeline',
    tabIcon: 'account_tree',
    tabLabelKey: 'PROJECT_DETAIL.GENERATION.TAB_PIPELINE',
    terminalTitle: 'pipeline-generator',
    generatingLabelKey: 'PROJECT_DETAIL.PIPELINE.TERMINAL_GENERATING',
    downloadLabelKey: 'PROJECT_DETAIL.PIPELINE.DOWNLOAD',
    downloadBusyLabelKey: 'PROJECT_DETAIL.PIPELINE.DOWNLOADING',
    retryLabelKey: 'PROJECT_DETAIL.PIPELINE.RETRY',
    artifactsLabelKey: 'PROJECT_DETAIL.PIPELINE.ARTIFACTS',
    terminalDoneKey: 'PROJECT_DETAIL.PIPELINE.TERMINAL_DONE',
    fileLoadingKey: 'PROJECT_DETAIL.PIPELINE.FILE_LOADING',
    fileErrorKey: 'PROJECT_DETAIL.PIPELINE.FILE_ERROR',
  },
  {
    id: 'bootstrap',
    tabIcon: 'rocket_launch',
    tabLabelKey: 'PROJECT_DETAIL.GENERATION.TAB_BOOTSTRAP',
    terminalTitle: 'bootstrap-generator',
    generatingLabelKey: 'PROJECT_DETAIL.BOOTSTRAP.GENERATING',
    downloadLabelKey: 'PROJECT_DETAIL.BOOTSTRAP.DOWNLOAD',
    downloadBusyLabelKey: 'PROJECT_DETAIL.BOOTSTRAP.DOWNLOADING',
    retryLabelKey: 'PROJECT_DETAIL.BOOTSTRAP.RETRY',
    artifactsLabelKey: 'PROJECT_DETAIL.BOOTSTRAP.ARTIFACTS',
    terminalDoneKey: 'PROJECT_DETAIL.BOOTSTRAP.TERMINAL_DONE',
    fileLoadingKey: 'PROJECT_DETAIL.BOOTSTRAP.FILE_LOADING',
    fileErrorKey: 'PROJECT_DETAIL.BOOTSTRAP.FILE_ERROR',
  },
] as const satisfies readonly MonoRepoTabDefinition[];

function isProjectLayoutPreset(value: string | null | undefined): value is ProjectLayoutPreset {
  return value === ALL_IN_ONE_LAYOUT || value === MULTI_REPO_LAYOUT || value === SPLIT_INFRA_CODE_LAYOUT;
}

function summarizeConfigs(configs: readonly InfrastructureConfigResponse[]): ConfigSummary {
  return configs.reduce<ConfigSummary>((summary, config) => ({
    resourceGroupCount: summary.resourceGroupCount + config.resourceGroupCount,
    resourceCount: summary.resourceCount + config.resourceCount,
    sharedPipelineCount: summary.sharedPipelineCount + (config.appPipelineMode === COMBINED_APP_PIPELINE_MODE ? 1 : 0),
  }), {
    resourceGroupCount: 0,
    resourceCount: 0,
    sharedPipelineCount: 0,
  });
}

function buildInfrastructureMetrics(configCount: number, summary: ConfigSummary): readonly RepositoryMetric[] {
  return [
    { labelKey: REPOSITORY_METRIC_LABEL_KEYS.configs, value: configCount },
    { labelKey: REPOSITORY_METRIC_LABEL_KEYS.resourceGroups, value: summary.resourceGroupCount },
    { labelKey: REPOSITORY_METRIC_LABEL_KEYS.resources, value: summary.resourceCount },
  ];
}

function buildApplicationCodeMetrics(configCount: number, summary: ConfigSummary): readonly RepositoryMetric[] {
  return [
    { labelKey: REPOSITORY_METRIC_LABEL_KEYS.applications, value: configCount },
    { labelKey: REPOSITORY_METRIC_LABEL_KEYS.pipelineScopes, value: configCount },
    { labelKey: REPOSITORY_METRIC_LABEL_KEYS.sharedPipelines, value: summary.sharedPipelineCount },
  ];
}

function isApplicationCodeRepository(repo: ProjectRepositoryResponse): boolean {
  return repo.contentKinds.includes(APPLICATION_CODE_CONTENT_KIND)
    && !repo.contentKinds.includes(INFRASTRUCTURE_CONTENT_KIND);
}

@Component({
  selector: 'app-generation-board',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    MatTabsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    BicepFilePanelComponent,
    SplitGenerationSwitcherComponent,
    DsButtonComponent,
    DsCardComponent,
    DsEmptyStateComponent,
    DsPanelActionButtonComponent,
    DsPageHeaderComponent,
  ],
  templateUrl: './generation-board.component.html',
  styleUrl: './generation-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ProjectDetailGenerationWorkflowService],
})
export class GenerationBoardComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sidebarContextService = inject(SidebarContextService);
  private readonly generationWorkflow = inject(ProjectDetailGenerationWorkflowService);

  readonly projectId = input<string>();

  private resolvedProjectId(): string {
    return this.projectId() || this.route.snapshot.paramMap.get('id') || '';
  }

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(true);

  protected readonly validatingDiagnostics = this.generationWorkflow.validatingDiagnostics;
  protected readonly projectGenerateAllLoading = this.generationWorkflow.projectGenerateAllLoading;
  protected readonly projectBicepLoading = this.generationWorkflow.projectBicepLoading;
  protected readonly projectBicepResult = this.generationWorkflow.projectBicepResult;
  protected readonly projectBicepDownloading = this.generationWorkflow.projectBicepDownloading;
  protected readonly projectInfraArtifactsDownloading = this.generationWorkflow.projectInfraArtifactsDownloading;
  protected readonly projectBicepErrorKey = this.generationWorkflow.projectBicepErrorKey;
  protected readonly projectGenerationPanelCollapsed = this.generationWorkflow.projectGenerationPanelCollapsed;
  protected readonly projectPipelineLoading = this.generationWorkflow.projectPipelineLoading;
  protected readonly projectPipelineResult = this.generationWorkflow.projectPipelineResult;
  protected readonly projectPipelineDownloading = this.generationWorkflow.projectPipelineDownloading;
  protected readonly projectCodeArtifactsDownloading = this.generationWorkflow.projectCodeArtifactsDownloading;
  protected readonly projectPipelineErrorKey = this.generationWorkflow.projectPipelineErrorKey;
  protected readonly projectBootstrapLoading = this.generationWorkflow.projectBootstrapLoading;
  protected readonly projectBootstrapResult = this.generationWorkflow.projectBootstrapResult;
  protected readonly projectBootstrapDownloading = this.generationWorkflow.projectBootstrapDownloading;
  protected readonly projectBootstrapErrorKey = this.generationWorkflow.projectBootstrapErrorKey;
  protected readonly canPushAllProjectArtifacts = this.generationWorkflow.canPushAllProjectArtifacts;
  protected readonly isSplitInfraCodeLayout = this.generationWorkflow.isSplitInfraCodeLayout;
  protected readonly projectBicepNodes = this.generationWorkflow.projectBicepNodes;
  protected readonly loadProjectBicepFile = this.generationWorkflow.loadProjectBicepFile;
  protected readonly projectPipelineNodes = this.generationWorkflow.projectPipelineNodes;
  protected readonly loadProjectPipelineFile = this.generationWorkflow.loadProjectPipelineFile;
  protected readonly projectBootstrapNodes = this.generationWorkflow.projectBootstrapNodes;
  protected readonly loadProjectBootstrapFile = this.generationWorkflow.loadProjectBootstrapFile;
  protected readonly deferMonoRepoBatchReveal = this.generationWorkflow.deferMonoRepoBatchReveal;
  protected readonly projectGenerationPanelOpen = this.generationWorkflow.projectGenerationPanelOpen;
  protected readonly toggleProjectGenerationPanelCollapsed = this.generationWorkflow.toggleProjectGenerationPanelCollapsed;
  protected readonly downloadProjectBicepFiles = this.generationWorkflow.downloadProjectBicepFiles;
  protected readonly downloadProjectInfraArtifacts = this.generationWorkflow.downloadProjectInfraArtifacts;
  protected readonly downloadProjectCodeArtifacts = this.generationWorkflow.downloadProjectCodeArtifacts;
  protected readonly downloadProjectPipelineFiles = this.generationWorkflow.downloadProjectPipelineFiles;
  protected readonly downloadProjectBootstrapFiles = this.generationWorkflow.downloadProjectBootstrapFiles;
  protected readonly generateProjectBicep = this.generationWorkflow.generateProjectBicep;
  protected readonly generateProjectPipeline = this.generationWorkflow.generateProjectPipeline;
  protected readonly generateProjectBootstrap = this.generationWorkflow.generateProjectBootstrap;
  protected readonly openProjectPushAllToGitDialog = this.generationWorkflow.openProjectPushAllToGitDialog;
  protected readonly openProjectMultiRepoPushDialog = this.generationWorkflow.openProjectMultiRepoPushDialog;
  protected readonly lastGenerationLoading = this.generationWorkflow.lastGenerationLoading;
  protected readonly lastGenerationAvailable = this.generationWorkflow.lastGenerationAvailable;
  protected readonly lastGenerationErrorKey = this.generationWorkflow.lastGenerationErrorKey;
  protected readonly loadLastGeneration = this.generationWorkflow.loadLastGeneration;
  protected readonly monoRepoTabs: readonly MonoRepoTab[] = [
    {
      ...MONO_REPO_TAB_DEFINITIONS[0],
      showBootstrapGuide: false,
      result: this.projectBicepResult,
      nodes: this.projectBicepNodes,
      isLoading: this.projectBicepLoading,
      isDownloading: this.projectBicepDownloading,
      errorKey: this.projectBicepErrorKey,
      loadFile: this.loadProjectBicepFile,
      download: this.downloadProjectBicepFiles,
      retry: this.generateProjectBicep,
    },
    {
      ...MONO_REPO_TAB_DEFINITIONS[1],
      showBootstrapGuide: false,
      result: this.projectPipelineResult,
      nodes: this.projectPipelineNodes,
      isLoading: this.projectPipelineLoading,
      isDownloading: this.projectPipelineDownloading,
      errorKey: this.projectPipelineErrorKey,
      loadFile: this.loadProjectPipelineFile,
      download: this.downloadProjectPipelineFiles,
      retry: this.generateProjectPipeline,
    },
    {
      ...MONO_REPO_TAB_DEFINITIONS[2],
      showBootstrapGuide: true,
      result: this.projectBootstrapResult,
      nodes: this.projectBootstrapNodes,
      isLoading: this.projectBootstrapLoading,
      isDownloading: this.projectBootstrapDownloading,
      errorKey: this.projectBootstrapErrorKey,
      loadFile: this.loadProjectBootstrapFile,
      download: this.downloadProjectBootstrapFiles,
      retry: this.generateProjectBootstrap,
    },
  ];

  protected readonly groupedByAlias = computed<AliasGroup[]>(() => {
    const configs = this.configs();
    const project = this.project();
    const repos = project?.repositories ?? [];

    if (project?.layoutPreset === SPLIT_INFRA_CODE_LAYOUT) {
      const summary = summarizeConfigs(configs);

      return repos.map((repo) => ({
        alias: repo.alias,
        repo,
        configs,
        resourceGroupCount: summary.resourceGroupCount,
        resourceCount: summary.resourceCount,
        metrics: isApplicationCodeRepository(repo)
          ? buildApplicationCodeMetrics(configs.length, summary)
          : buildInfrastructureMetrics(configs.length, summary),
      }));
    }

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
        const summary = summarizeConfigs(items);

        return {
          alias,
          repo: repoByAlias.get(alias)
            ?? (isMultiRepo
              ? (items[0]?.repositories?.find((r) => r.alias === alias) ?? null)
              : null),
          configs: items,
          resourceGroupCount: summary.resourceGroupCount,
          resourceCount: summary.resourceCount,
          metrics: buildInfrastructureMetrics(items.length, summary),
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
    this.runTask(this.load());
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
      this.sidebarContextService.setProjectContext(project.id, project.name);
      this.generationWorkflow.setProject(project);
      this.generationWorkflow.setConfigs(configs);
      this.runTask(this.generationWorkflow.checkLastGenerationAvailable());
    } catch {
      this.showError('PROJECT_DETAIL.BOARD.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onGenerateAll(): void {
    this.runTask(this.generationWorkflow.generateAll());
  }

  protected openProjectDetail(): void {
    const projectId = this.project()?.id || this.resolvedProjectId();
    if (!projectId) {
      return;
    }

    this.runNavigation(this.router.navigate(['/projects', projectId]));
  }

  protected navigateToConfig(): void {
    const projectId = this.project()?.id || this.resolvedProjectId();
    if (!projectId) {
      return;
    }

    this.runNavigation(this.router.navigate(['/projects', projectId, 'generate', 'config']));
  }

  private runNavigation(navigationPromise: Promise<boolean>): void {
    navigationPromise.catch(() => undefined);
  }

  private runTask(taskPromise: Promise<void>): void {
    taskPromise.catch(() => undefined);
  }

  private showError(key: string): void {
    const message = this.translate.instant(key);
    this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), {
      duration: 5000,
      panelClass: 'error-snackbar',
    });
  }
}
