import { computed, inject, Injectable, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { saveAs } from 'file-saver';
import JSZip from 'jszip';
import { firstValueFrom } from 'rxjs';

import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
  GetProjectLatestGenerationResponse,
  ProjectResponse,
} from '../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import {
  ConfigDiagnosticGroup,
  ConfigMissingEnvGroup,
  GenerationDiagnosticsDialogComponent,
  GenerationDiagnosticsDialogData,
  MissingEnvResource,
} from '../../shared/components/generation-diagnostics-dialog/generation-diagnostics-dialog.component';
import {
  PushToGitDialogComponent,
  PushToGitDialogData,
} from '../config-detail/push-to-git-dialog/push-to-git-dialog.component';
import {
  MultiRepoPushDialogComponent,
  MultiRepoPushDialogData,
} from './multi-repo-push-dialog/multi-repo-push-dialog.component';
import { MultiRepoPushMode } from '../../shared/interfaces/multi-repo-push.interface';
import {
  GeneratedArtifactArchiveSourceSpec,
  resolveGeneratedArtifactEntryPath,
} from './project-generated-artifact-paths';
import {
  ensureProjectArchiveEntrySizeWithinLimits,
  ensureProjectArchiveSourceSizeWithinLimits,
  resolveProjectDetailSplitRepoAliases,
  tryGetProjectArchiveEntryUncompressedSize,
} from './project-detail-generation.helper';
import { shouldDeferMonoRepoBatchReveal } from './project-generation-visibility.helper';
import { buildAzureDevOpsNodes, buildProjectBicepNodes } from './project-detail-tree.helpers';
import { BicepFileNode, BicepTreeNode } from '../../shared/components/bicep-file-panel/bicep-file-panel.component';

const MAX_PROJECT_ARCHIVE_ENTRY_COUNT = 500;
const COMBINED_ARCHIVE_LOAD_OPTIONS = {
  checkCRC32: true,
  createFolders: false,
} as const;

interface CombinedArtifactArchiveSource extends GeneratedArtifactArchiveSourceSpec {
  readonly archivePromise: Promise<Blob>;
}

interface CombinedProjectArchiveExtractionState {
  totalExtractedBytes: number;
}

@Injectable()
export class ProjectDetailGenerationWorkflowService {
  private readonly dialog = inject(MatDialog);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly projectService = inject(ProjectService);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  private readonly project = signal<ProjectResponse | null>(null);
  private readonly configs = signal<InfrastructureConfigResponse[]>([]);

  readonly validatingDiagnostics = signal(false);
  readonly projectGenerateAllBatchActive = signal(false);

  readonly projectBicepLoading = signal(false);
  readonly projectBicepResult = signal<GenerateProjectBicepResponse | null>(null);
  readonly projectBicepDownloading = signal(false);
  readonly projectInfraArtifactsDownloading = signal(false);
  readonly projectBicepErrorKey = signal('');
  readonly projectBicepPanelOpen = signal(false);
  readonly projectGenerationPanelCollapsed = signal(false);

  readonly projectPipelineLoading = signal(false);
  readonly projectPipelineResult = signal<GenerateProjectPipelineResponse | null>(null);
  readonly projectPipelineDownloading = signal(false);
  readonly projectCodeArtifactsDownloading = signal(false);
  readonly projectPipelineErrorKey = signal('');
  readonly projectPipelinePanelOpen = signal(false);

  readonly projectBootstrapLoading = signal(false);
  readonly projectBootstrapResult = signal<GenerateProjectBootstrapPipelineResponse | null>(null);
  readonly projectBootstrapDownloading = signal(false);
  readonly projectBootstrapErrorKey = signal('');
  readonly projectBootstrapPanelOpen = signal(false);

  readonly lastGenerationLoading = signal(false);
  readonly lastGenerationAvailable = signal<boolean | null>(null);
  readonly lastGenerationErrorKey = signal('');
  readonly viewingHistoricalGeneration = signal(false);
  readonly displayedHistoricalGenerationAt = signal<string | null>(null);

  readonly canPushAllProjectArtifacts = computed(
    () => this.projectBicepResult() !== null
      && this.projectPipelineResult() !== null
      && this.projectBootstrapResult() !== null,
  );

  readonly isSplitInfraCodeLayout = computed(() => this.project()?.layoutPreset === 'SplitInfraCode');

  readonly projectBicepNodes = computed<BicepTreeNode[]>(() => {
    const result = this.projectBicepResult();
    return result
      ? buildProjectBicepNodes(result.commonFileUris, result.configFileUris)
      : [];
  });

  readonly loadProjectBicepFile = (filePath: string): Promise<string> => {
    const projectId = this.project()?.id ?? '';
    return this.projectService.getProjectBicepFileContent(projectId, filePath);
  };

  readonly projectPipelineNodes = computed<BicepTreeNode[]>(() => {
    const result = this.projectPipelineResult();
    if (!result) return [];
    return buildAzureDevOpsNodes(result.commonFileUris, result.configFileUris);
  });

  readonly loadProjectPipelineFile = (filePath: string): Promise<string> => {
    const projectId = this.project()?.id ?? '';
    return this.projectService.getProjectPipelineFileContent(projectId, filePath);
  };

  readonly projectBootstrapNodes = computed<BicepTreeNode[]>(() => {
    const result = this.projectBootstrapResult();
    if (!result) return [];
    return Object.keys(result.fileUris).map((fileName) => ({
      kind: 'file',
      path: fileName,
      displayName: fileName,
      type: 'generic',
      uri: fileName,
      depth: 0,
      parentFolderKey: '',
    } satisfies BicepFileNode));
  });

  readonly loadProjectBootstrapFile = (filePath: string): Promise<string> => {
    const projectId = this.project()?.id ?? '';
    return this.projectService.getProjectBootstrapPipelineFileContent(projectId, filePath);
  };

  readonly anyProjectGenerationLoading = computed(
    () => this.projectBicepLoading() || this.projectPipelineLoading() || this.projectBootstrapLoading(),
  );

  readonly projectGenerateAllLoading = computed(
    () => this.validatingDiagnostics() || this.anyProjectGenerationLoading(),
  );

  readonly deferMonoRepoBatchReveal = computed(
    () => shouldDeferMonoRepoBatchReveal({
      isGenerateAllBatchActive: this.projectGenerateAllBatchActive(),
      isAnyGenerationLoading: this.anyProjectGenerationLoading(),
    }),
  );

  readonly projectGenerationPanelOpen = computed(
    () => this.projectBicepPanelOpen() || this.projectPipelinePanelOpen() || this.projectBootstrapPanelOpen() || this.anyProjectGenerationLoading(),
  );

  setProject(project: ProjectResponse | null): void {
    this.project.set(project);
  }

  setConfigs(configs: InfrastructureConfigResponse[]): void {
    this.configs.set(configs);
  }

  readonly checkLastGenerationAvailable = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const result = await this.projectService.getProjectLatestGeneration(projectId);
    this.lastGenerationAvailable.set(result !== null);
  };

  readonly loadLastGeneration = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.lastGenerationLoading()) return;

    this.lastGenerationLoading.set(true);
    this.lastGenerationErrorKey.set('');

    try {
      const result = await this.projectService.getProjectLatestGeneration(projectId);

      if (!result) {
        this.lastGenerationErrorKey.set('PROJECT_DETAIL.BOARD.LAST_GENERATION_EXPIRED');
        return;
      }

      this.applyHistoricalGenerationContext(result);

      if (result.bicep) {
        this.projectBicepResult.set({
          commonFileUris: result.bicep.commonFileUris,
          configFileUris: result.bicep.configFileUris,
        });
        this.projectBicepPanelOpen.set(true);
      }

      if (result.pipeline) {
        this.projectPipelineResult.set({
          commonFileUris: result.pipeline.commonFileUris,
          configFileUris: result.pipeline.configFileUris,
          infraCommonFileUris: result.pipeline.infraCommonFileUris,
          appCommonFileUris: result.pipeline.appCommonFileUris,
          infraConfigFileUris: result.pipeline.infraConfigFileUris,
          appConfigFileUris: result.pipeline.appConfigFileUris,
        });
        this.projectPipelinePanelOpen.set(true);
      }

      if (result.bootstrap) {
        this.projectBootstrapResult.set({
          fileUris: result.bootstrap.fileUris,
          infraFileUris: result.bootstrap.infraFileUris,
          appFileUris: result.bootstrap.appFileUris,
        });
        this.projectBootstrapPanelOpen.set(true);
      }
    } catch {
      this.lastGenerationErrorKey.set('PROJECT_DETAIL.BOARD.LAST_GENERATION_ERROR');
    } finally {
      this.lastGenerationLoading.set(false);
    }
  };

  readonly generateProjectBicep = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBicepLoading()) return;

    const shouldContinue = await this.checkProjectDiagnostics();
    if (!shouldContinue) return;

    await this.doGenerateProjectBicep();
  };

  readonly generateProjectPipeline = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectPipelineLoading()) return;

    const shouldContinue = await this.checkProjectDiagnostics();
    if (!shouldContinue) return;

    await this.doGenerateProjectPipeline();
  };

  readonly generateProjectBootstrap = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBootstrapLoading()) return;

    await this.doGenerateProjectBootstrap();
  };

  readonly generateAll = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectGenerateAllLoading()) return;

    this.validatingDiagnostics.set(true);
    try {
      const shouldContinue = await this.checkProjectDiagnostics();
      if (!shouldContinue) return;
    } finally {
      this.validatingDiagnostics.set(false);
    }

    this.projectGenerateAllBatchActive.set(true);
    try {
      await Promise.all([
        this.doGenerateProjectBicep(),
        this.doGenerateProjectPipeline(),
        this.doGenerateProjectBootstrap(),
      ]);
    } finally {
      this.projectGenerateAllBatchActive.set(false);
    }
  };

  readonly toggleProjectGenerationPanelCollapsed = (): void => {
    this.projectGenerationPanelCollapsed.update((collapsed) => !collapsed);
  };

  readonly downloadProjectBicepFiles = async (): Promise<void> => {
    const project = this.project();
    const result = this.projectBicepResult();

    if (!project?.id || !result || this.projectBicepDownloading()) return;

    this.projectBicepDownloading.set(true);
    try {
      const blob = await this.projectService.downloadProjectZip(project.id);
      saveAs(blob, `${project.name ?? 'project'}-bicep.zip`);
    } finally {
      this.projectBicepDownloading.set(false);
    }
  };

  readonly downloadProjectInfraArtifacts = async (): Promise<void> => {
    const project = this.project();
    const bicepResult = this.projectBicepResult();
    const pipelineResult = this.projectPipelineResult();
    const bootstrapResult = this.projectBootstrapResult();

    if (!project?.id || !bicepResult || !pipelineResult || !bootstrapResult || this.projectInfraArtifactsDownloading()) {
      return;
    }

    this.projectInfraArtifactsDownloading.set(true);
    try {
      const blob = await this.buildCombinedProjectArchive([
        { archivePromise: this.projectService.downloadProjectZip(project.id), archiveKind: 'bicep' },
        { archivePromise: this.projectService.downloadProjectPipelineZip(project.id), archiveKind: 'pipeline', filterPrefix: 'infra' },
        { archivePromise: this.projectService.downloadProjectBootstrapPipelineZip(project.id), archiveKind: 'bootstrap', filterPrefix: 'infra' },
      ]);

      saveAs(blob, `${project.name ?? 'project'}-infra-artifacts.zip`);
    } catch (error) {
      console.error('Failed to assemble project infrastructure artifacts archive.', error);
      this.showProjectActionError('PROJECT_DETAIL.SWITCHER.DOWNLOAD_ARCHIVE_ERROR');
    } finally {
      this.projectInfraArtifactsDownloading.set(false);
    }
  };

  readonly downloadProjectCodeArtifacts = async (): Promise<void> => {
    const project = this.project();
    const pipelineResult = this.projectPipelineResult();
    const bootstrapResult = this.projectBootstrapResult();

    if (!project?.id || !pipelineResult || !bootstrapResult || this.projectCodeArtifactsDownloading()) {
      return;
    }

    this.projectCodeArtifactsDownloading.set(true);
    try {
      const blob = await this.buildCombinedProjectArchive([
        { archivePromise: this.projectService.downloadProjectPipelineZip(project.id), archiveKind: 'pipeline', filterPrefix: 'app' },
        { archivePromise: this.projectService.downloadProjectBootstrapPipelineZip(project.id), archiveKind: 'bootstrap', filterPrefix: 'app' },
      ]);

      saveAs(blob, `${project.name ?? 'project'}-code-artifacts.zip`);
    } catch (error) {
      console.error('Failed to assemble project code artifacts archive.', error);
      this.showProjectActionError('PROJECT_DETAIL.SWITCHER.DOWNLOAD_ARCHIVE_ERROR');
    } finally {
      this.projectCodeArtifactsDownloading.set(false);
    }
  };

  readonly openProjectPushAllToGitDialog = (): void => {
    const project = this.project();
    if (project?.layoutPreset === 'SplitInfraCode' || !project?.repositories?.length) return;

    const data: PushToGitDialogData = {
      configId: '',
      projectId: project.id,
      isProjectLevel: true,
      isCombinedProjectPush: true,
    };
    this.dialog.open(PushToGitDialogComponent, { width: '480px', data });
  };

  readonly openProjectMultiRepoPushDialog = (mode: MultiRepoPushMode): void => {
    const project = this.project();
    const aliases = project ? resolveProjectDetailSplitRepoAliases(project) : null;
    if (!project || !aliases) {
      this.showProjectActionError('PROJECT_DETAIL.MULTI_REPO_PUSH.MISSING_SLOTS');
      return;
    }

    const data: MultiRepoPushDialogData = {
      projectId: project.id,
      infraAlias: aliases.infraAlias,
      codeAlias: aliases.codeAlias,
      mode,
    };

    this.dialog.open(MultiRepoPushDialogComponent, {
      width: mode === 'both' ? '68rem' : '38rem',
      maxWidth: '96vw',
      panelClass: 'ifs-multi-repo-push-dialog',
      data,
    });
  };

  readonly downloadProjectPipelineFiles = async (): Promise<void> => {
    const project = this.project();
    const result = this.projectPipelineResult();

    if (!project?.id || !result || this.projectPipelineDownloading()) return;

    this.projectPipelineDownloading.set(true);
    try {
      const blob = await this.projectService.downloadProjectPipelineZip(project.id);
      saveAs(blob, `${project.name ?? 'project'}-pipeline.zip`);
    } finally {
      this.projectPipelineDownloading.set(false);
    }
  };

  readonly downloadProjectBootstrapFiles = async (): Promise<void> => {
    const project = this.project();
    const result = this.projectBootstrapResult();

    if (!project?.id || !result || this.projectBootstrapDownloading()) return;

    this.projectBootstrapDownloading.set(true);
    try {
      const blob = await this.projectService.downloadProjectBootstrapPipelineZip(project.id);
      saveAs(blob, `${project.name ?? 'project'}-bootstrap.zip`);
    } finally {
      this.projectBootstrapDownloading.set(false);
    }
  };

  private readonly doGenerateProjectBicep = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBicepLoading()) return;

    this.clearHistoricalGenerationContext();
    this.projectBicepLoading.set(true);
    this.projectBicepErrorKey.set('');
    this.projectBicepResult.set(null);
    this.projectGenerationPanelCollapsed.set(false);
    this.projectBicepPanelOpen.set(true);

    try {
      const result = await this.projectService.generateProjectBicep(projectId);
      this.projectBicepResult.set(result);
    } catch {
      this.projectBicepErrorKey.set('PROJECT_DETAIL.BICEP.GENERATE_ERROR');
    } finally {
      this.projectBicepLoading.set(false);
    }
  };

  private readonly doGenerateProjectPipeline = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectPipelineLoading()) return;

    this.clearHistoricalGenerationContext();
    this.projectPipelineLoading.set(true);
    this.projectPipelineErrorKey.set('');
    this.projectPipelineResult.set(null);
    this.projectGenerationPanelCollapsed.set(false);
    this.projectPipelinePanelOpen.set(true);

    try {
      const result = await this.projectService.generateProjectPipeline(projectId);
      this.projectPipelineResult.set(result);
    } catch {
      this.projectPipelineErrorKey.set('PROJECT_DETAIL.PIPELINE.GENERATE_ERROR');
    } finally {
      this.projectPipelineLoading.set(false);
    }
  };

  private readonly doGenerateProjectBootstrap = async (): Promise<void> => {
    const projectId = this.project()?.id;
    if (!projectId || this.projectBootstrapLoading()) return;

    this.clearHistoricalGenerationContext();
    this.projectBootstrapLoading.set(true);
    this.projectBootstrapErrorKey.set('');
    this.projectBootstrapResult.set(null);
    this.projectGenerationPanelCollapsed.set(false);
    this.projectBootstrapPanelOpen.set(true);

    try {
      const result = await this.projectService.generateProjectBootstrapPipeline(projectId);
      this.projectBootstrapResult.set(result);
    } catch {
      this.projectBootstrapErrorKey.set('PROJECT_DETAIL.BOOTSTRAP.GENERATE_ERROR');
    } finally {
      this.projectBootstrapLoading.set(false);
    }
  };

  private readonly checkProjectDiagnostics = async (): Promise<boolean> => {
    const allConfigs = this.configs();
    if (allConfigs.length === 0) return true;

    const allEnvNames = (this.project()?.environmentDefinitions ?? [])
      .sort((left, right) => left.order - right.order)
      .map((environment) => environment.name);

    const excludedEnvironmentTypes = new Set(['UserAssignedIdentity']);

    const results = await Promise.all(
      allConfigs.map(async (config) => {
        try {
          const [diagnosticResult, resourceGroups] = await Promise.all([
            this.infraConfigService.getDiagnostics(config.id),
            this.infraConfigService.getResourceGroups(config.id),
          ]);

          const groupResources = await Promise.all(
            resourceGroups.map((resourceGroup) => this.resourceGroupService.getResources(resourceGroup.id).catch(() => [] as AzureResourceResponse[])),
          );

          const missingEnvResources: MissingEnvResource[] = [];
          for (const resources of groupResources) {
            for (const resource of resources) {
              if (resource.isExisting) continue;
              if (excludedEnvironmentTypes.has(resource.resourceType)) continue;
              const configured = new Set(resource.configuredEnvironments ?? []);
              const missingEnvironments = allEnvNames.filter((environmentName) => !configured.has(environmentName));
              if (missingEnvironments.length > 0) {
                missingEnvResources.push({
                  resourceId: resource.id,
                  resourceName: resource.name,
                  resourceType: resource.resourceType,
                  missingEnvironments,
                });
              }
            }
          }

          return { config, diagnostics: diagnosticResult.diagnostics, missingEnvResources };
        } catch {
          return { config, diagnostics: [], missingEnvResources: [] as MissingEnvResource[] };
        }
      }),
    );

    const configsWithIssues: ConfigDiagnosticGroup[] = results
      .filter((result) => result.diagnostics.length > 0)
      .map((result) => ({
        configId: result.config.id,
        configName: result.config.name,
        diagnostics: result.diagnostics,
      }));

    const configsWithMissingEnvs: ConfigMissingEnvGroup[] = results
      .filter((result) => result.missingEnvResources.length > 0)
      .map((result) => ({
        configId: result.config.id,
        configName: result.config.name,
        resources: result.missingEnvResources,
      }));

    if (configsWithIssues.length === 0 && configsWithMissingEnvs.length === 0) {
      return true;
    }

    const dialogRef = this.dialog.open(GenerationDiagnosticsDialogComponent, {
      data: {
        configDiagnostics: configsWithIssues,
        missingEnvConfigs: configsWithMissingEnvs.length > 0 ? configsWithMissingEnvs : undefined,
      } satisfies GenerationDiagnosticsDialogData,
      width: '640px',
      maxHeight: '80vh',
    });

    const result = await firstValueFrom(dialogRef.afterClosed());
    return result === true;
  };

  private applyHistoricalGenerationContext(result: GetProjectLatestGenerationResponse): void {
    const hasHistoricalArtifacts = result.bicep !== null || result.pipeline !== null || result.bootstrap !== null;

    this.viewingHistoricalGeneration.set(hasHistoricalArtifacts);
    this.displayedHistoricalGenerationAt.set(hasHistoricalArtifacts ? result.generatedAt : null);
  }

  private clearHistoricalGenerationContext(): void {
    this.viewingHistoricalGeneration.set(false);
    this.displayedHistoricalGenerationAt.set(null);
  }

  private async buildCombinedProjectArchive(sources: CombinedArtifactArchiveSource[]): Promise<Blob> {
    const archive = new JSZip();
    const extractionState: CombinedProjectArchiveExtractionState = { totalExtractedBytes: 0 };

    for (const source of sources) {
      const sourceBlob = await source.archivePromise;
      await this.appendArchiveEntries(archive, sourceBlob, source, extractionState);
    }

    return archive.generateAsync({ type: 'blob' });
  }

  private async appendArchiveEntries(
    targetArchive: JSZip,
    sourceArchiveBlob: Blob,
    source: CombinedArtifactArchiveSource,
    extractionState: CombinedProjectArchiveExtractionState,
  ): Promise<void> {
    const sourceArchive = await this.loadCombinedArchiveSafely(sourceArchiveBlob);
    const fileEntries = Object.values(sourceArchive.files).filter((entry) => !entry.dir);
    if (fileEntries.length > MAX_PROJECT_ARCHIVE_ENTRY_COUNT) {
      throw new Error('Generated artifact archive contains too many files.');
    }

    for (const entry of fileEntries) {
      const entryPathResolution = resolveGeneratedArtifactEntryPath(entry.name, source);
      if (entryPathResolution.status === 'unsafe') {
        throw new Error(`Generated artifact archive contains an unsafe path: ${entry.name}`);
      }

      if (entryPathResolution.status !== 'resolved') {
        continue;
      }

      const entryPath = entryPathResolution.path;
      if (!entryPath) {
        throw new Error(`Generated artifact archive resolved an empty target path for entry: ${entry.name}`);
      }

      const expectedEntrySize = tryGetProjectArchiveEntryUncompressedSize(entry);
      if (expectedEntrySize !== null) {
        ensureProjectArchiveEntrySizeWithinLimits(expectedEntrySize, extractionState.totalExtractedBytes);
      }

      const entryBytes = await entry.async('uint8array');
      ensureProjectArchiveEntrySizeWithinLimits(entryBytes.byteLength, extractionState.totalExtractedBytes);

      extractionState.totalExtractedBytes += entryBytes.byteLength;
      targetArchive.file(entryPath, entryBytes);
    }
  }

  private async loadCombinedArchiveSafely(sourceArchiveBlob: Blob): Promise<JSZip> {
    ensureProjectArchiveSourceSizeWithinLimits(sourceArchiveBlob.size);
    return JSZip.loadAsync(sourceArchiveBlob, COMBINED_ARCHIVE_LOAD_OPTIONS);
  }

  private showProjectActionError(messageKey: string): void {
    this.snackBar.open(
      this.translate.instant(messageKey),
      this.translate.instant('COMMON.CLOSE'),
      {
        duration: 5000,
        panelClass: 'error-snackbar',
      },
    );
  }
}