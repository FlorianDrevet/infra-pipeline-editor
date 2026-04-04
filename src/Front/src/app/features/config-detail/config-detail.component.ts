import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import {
  InfrastructureConfigResponse,
  ResourceNamingTemplateResponse,
  SetInfraConfigTagsRequest,
  TagRequest,
} from '../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse, AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddResourceGroupDialogComponent, AddResourceGroupDialogData } from './add-resource-group-dialog/add-resource-group-dialog.component';
import { AddResourceDialogComponent, AddResourceDialogData } from './add-resource-dialog/add-resource-dialog.component';
import {
  AddNamingTemplateDialogComponent,
  AddNamingTemplateDialogData,
  AddNamingTemplateDialogResult,
} from './add-naming-template-dialog/add-naming-template-dialog.component';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { KeyVaultService } from '../../shared/services/key-vault.service';
import { RedisCacheService } from '../../shared/services/redis-cache.service';
import { StorageAccountService } from '../../shared/services/storage-account.service';
import { UserAssignedIdentityService } from '../../shared/services/user-assigned-identity.service';
import { AppServicePlanService } from '../../shared/services/app-service-plan.service';
import { WebAppService } from '../../shared/services/web-app.service';
import { FunctionAppService } from '../../shared/services/function-app.service';
import { AppConfigurationService } from '../../shared/services/app-configuration.service';
import { ContainerAppEnvironmentService } from '../../shared/services/container-app-environment.service';
import { ContainerAppService } from '../../shared/services/container-app.service';
import { LogAnalyticsWorkspaceService } from '../../shared/services/log-analytics-workspace.service';
import { ApplicationInsightsService } from '../../shared/services/application-insights.service';
import { CosmosDbService } from '../../shared/services/cosmos-db.service';
import { SqlServerService } from '../../shared/services/sql-server.service';
import { SqlDatabaseService } from '../../shared/services/sql-database.service';
import { ServiceBusNamespaceService } from '../../shared/services/service-bus-namespace.service';
import { ContainerRegistryService } from '../../shared/services/container-registry.service';
import { ProjectService } from '../../shared/services/project.service';
import { BicepGeneratorService } from '../../shared/services/bicep-generator.service';
import { PipelineGeneratorService } from '../../shared/services/pipeline-generator.service';
import { CascadeDeleteDialogComponent, CascadeDeleteDialogData } from '../../shared/components/cascade-delete-dialog/cascade-delete-dialog.component';
import { DependentResourceResponse } from '../../shared/interfaces/dependent-resource.interface';
import { GenerateBicepResponse, ResourceDiagnosticResponse } from '../../shared/interfaces/bicep-generator.interface';
import { GeneratePipelineResponse } from '../../shared/interfaces/pipeline-generator.interface';
import { saveAs } from 'file-saver';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
import { ProjectResponse, TestGitConnectionResponse } from '../../shared/interfaces/project.interface';
import { RESOURCE_TYPE_ABBREVIATIONS, RESOURCE_TYPE_ICONS, RESOURCE_TYPE_OPTIONS, PARENT_CHILD_RESOURCE_TYPES, CHILD_RESOURCE_TYPES } from './enums/resource-type.enum';
import { FormsModule, FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { BicepFilePanelComponent, BicepFileNode, BicepFolderNode, BicepTreeNode } from '../../shared/components/bicep-file-panel/bicep-file-panel.component';
import { StorageAccountResponse } from '../../shared/interfaces/storage-account.interface';
import { AddStorageServiceDialogComponent, AddStorageServiceDialogData, AddStorageServiceDialogResult } from './add-storage-service-dialog/add-storage-service-dialog.component';
import { PushToGitDialogComponent, PushToGitDialogData } from './push-to-git-dialog/push-to-git-dialog.component';
import { GitConfigDialogComponent, GitConfigDialogData } from '../project-detail/git-config-dialog/git-config-dialog.component';
import {
  CrossConfigReferenceResponse,
  IncomingCrossConfigReferenceResponse,
} from '../../shared/interfaces/cross-config-reference.interface';
import { ProjectPipelineVariableGroupResponse } from '../../shared/interfaces/project.interface';
import { AddVariableGroupDialogComponent } from './add-variable-group-dialog/add-variable-group-dialog.component';
import { DiagnosticPopoverComponent } from '../../shared/components/diagnostic-popover/diagnostic-popover.component';
import {
  GenerationDiagnosticsDialogComponent,
  GenerationDiagnosticsDialogData,
  MissingEnvResource,
} from '../../shared/components/generation-diagnostics-dialog/generation-diagnostics-dialog.component';
import { firstValueFrom } from 'rxjs';

interface ResourceDisplayItem {
  resource: AzureResourceResponse;
  children?: AzureResourceResponse[];
  incomingChildren?: IncomingCrossConfigReferenceResponse[];
  isParent: boolean;
  crossConfigRef?: CrossConfigReferenceResponse;
}


@Component({
  selector: 'app-config-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTooltipModule,
    BicepFilePanelComponent,
    DiagnosticPopoverComponent,
  ],
  templateUrl: './config-detail.component.html',
  styleUrl: './config-detail.component.scss',
})
export class ConfigDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly keyVaultService = inject(KeyVaultService);
  private readonly redisCacheService = inject(RedisCacheService);
  private readonly storageAccountService = inject(StorageAccountService);
  private readonly userAssignedIdentityService = inject(UserAssignedIdentityService);
  private readonly appServicePlanService = inject(AppServicePlanService);
  private readonly webAppService = inject(WebAppService);
  private readonly functionAppService = inject(FunctionAppService);
  private readonly appConfigurationService = inject(AppConfigurationService);
  private readonly containerAppEnvironmentService = inject(ContainerAppEnvironmentService);
  private readonly containerAppService = inject(ContainerAppService);
  private readonly logAnalyticsWorkspaceService = inject(LogAnalyticsWorkspaceService);
  private readonly applicationInsightsService = inject(ApplicationInsightsService);
  private readonly cosmosDbService = inject(CosmosDbService);
  private readonly sqlServerService = inject(SqlServerService);
  private readonly sqlDatabaseService = inject(SqlDatabaseService);
  private readonly serviceBusNamespaceService = inject(ServiceBusNamespaceService);
  private readonly containerRegistryService = inject(ContainerRegistryService);
  private readonly projectService = inject(ProjectService);
  private readonly bicepService = inject(BicepGeneratorService);
  private readonly pipelineService = inject(PipelineGeneratorService);
  private readonly authService = inject(AuthenticationService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);

  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly resourceGroups = signal<ResourceGroupResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly namingActionKey = signal<string | null>(null);
  protected readonly namingErrorKey = signal('');
  protected readonly rgErrorKey = signal('');
  protected readonly expandedRgId = signal<string | null>(null);
  protected readonly rgResources = signal<{ [rgId: string]: AzureResourceResponse[] | undefined }>({});
  protected readonly rgResourcesLoading = signal<string | null>(null);
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;
  protected readonly resourceTypeOptions = RESOURCE_TYPE_OPTIONS;

  // ─── Parent-child resource grouping ───
  protected readonly expandedParentResources = signal<Set<string>>(new Set<string>());

  // ─── Storage Account sub-resources ───
  protected readonly storageAccountDetails = signal<Record<string, StorageAccountResponse | undefined>>({});
  protected readonly storageDetailsLoading = signal<string | null>(null);

  // ─── Bicep Generation ───
  protected readonly bicepLoading = signal(false);
  protected readonly bicepResult = signal<GenerateBicepResponse | null>(null);
  protected readonly bicepErrorKey = signal('');
  protected readonly bicepPanelOpen = signal(false);
  protected readonly bicepPanelCollapsed = signal(false);
  protected readonly bicepDownloading = signal(false);

  // ─── Bicep File Panel ───

  protected readonly configBicepNodes = computed<BicepTreeNode[]>(() => {
    const result = this.bicepResult();
    if (!result) return [];
    const nodes: BicepTreeNode[] = [];

    // Root-level files — uri is the file path, passed to getFileContent() by loadConfigBicepFile
    nodes.push({ kind: 'file', path: 'types.bicep', displayName: 'types.bicep', type: 'types', uri: 'types.bicep', depth: 0, parentFolderKey: '' });
    nodes.push({ kind: 'file', path: 'functions.bicep', displayName: 'functions.bicep', type: 'functions', uri: 'functions.bicep', depth: 0, parentFolderKey: '' });
    if (result.constantsBicepUri) {
      nodes.push({ kind: 'file', path: 'constants.bicep', displayName: 'constants.bicep', type: 'constants', uri: 'constants.bicep', depth: 0, parentFolderKey: '' });
    }
    nodes.push({ kind: 'file', path: 'main.bicep', displayName: 'main.bicep', type: 'entry-point', uri: 'main.bicep', depth: 0, parentFolderKey: '' });

    // parameters/ folder
    const params = Object.entries(result.parameterFileUris ?? {});
    if (params.length > 0) {
      nodes.push({ kind: 'folder', key: 'parameters', name: 'parameters/', folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
      for (const [name, uri] of params) {
        nodes.push({ kind: 'file', path: name, displayName: name.split('/').at(-1)!, type: 'params', uri, depth: 1, parentFolderKey: 'parameters' });
      }
    }

    // modules/ folder (with per-resource sub-folders)
    if (result.moduleUris && Object.keys(result.moduleUris).length > 0) {
      const folderMap = new Map<string, { name: string; files: Array<{ path: string; displayName: string; uri: string }> }>();
      for (const [filePath, uri] of Object.entries(result.moduleUris)) {
        const parts = filePath.split('/');
        if (parts.length < 3) continue;
        const folderName = parts[1];
        const displayName = parts[2];
        const folderKey = `modules/${folderName}`;
        if (!folderMap.has(folderKey)) folderMap.set(folderKey, { name: folderName, files: [] });
        folderMap.get(folderKey)!.files.push({ path: filePath, displayName, uri });
      }
      if (folderMap.size > 0) {
        nodes.push({ kind: 'folder', key: 'modules', name: 'modules/', folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
        for (const [folderKey, folder] of folderMap) {
          nodes.push({ kind: 'folder', key: folderKey, name: `${folder.name}/`, folderIcon: 'folder', depth: 1, parentFolderKey: 'modules' } satisfies BicepFolderNode);
          for (const file of folder.files) {
            const type =
              file.displayName === 'types.bicep' ? 'types'
              : file.displayName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
              : 'module-type';
            nodes.push({ kind: 'file', path: file.path, displayName: file.displayName, type, uri: file.uri, depth: 2, parentFolderKey: folderKey } satisfies BicepFileNode);
          }
        }
      }
    }

    return nodes;
  });

  protected readonly loadConfigBicepFile = (filePath: string): Promise<string> => {
    const configId = this.config()?.id ?? '';
    return this.bicepService.getFileContent(configId, filePath);
  };

  // ─── Pipeline Generation ───
  protected readonly pipelineLoading = signal(false);
  protected readonly pipelineResult = signal<GeneratePipelineResponse | null>(null);
  protected readonly pipelineErrorKey = signal('');
  protected readonly pipelinePanelOpen = signal(false);
  protected readonly pipelinePanelCollapsed = signal(false);
  protected readonly pipelineDownloading = signal(false);

  protected readonly configPipelineNodes = computed<BicepTreeNode[]>(() => {
    const result = this.pipelineResult();
    if (!result) return [];
    const nodes: BicepTreeNode[] = [];

    const folderMap = new Map<string, { name: string; files: Array<{ path: string; displayName: string; uri: string }> }>();

    for (const [filePath, uri] of Object.entries(result.fileUris)) {
      const parts = filePath.split('/');
      if (parts.length >= 2) {
        const folderName = parts.slice(0, -1).join('/');
        const displayName = parts[parts.length - 1];
        if (!folderMap.has(folderName)) folderMap.set(folderName, { name: folderName, files: [] });
        folderMap.get(folderName)!.files.push({ path: filePath, displayName, uri });
      } else {
        nodes.push({ kind: 'file', path: filePath, displayName: filePath, type: 'generic', uri, depth: 0, parentFolderKey: '' });
      }
    }

    for (const [folderKey, folder] of folderMap) {
      nodes.push({ kind: 'folder', key: folderKey, name: `${folder.name}/`, folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
      for (const file of folder.files) {
        nodes.push({ kind: 'file', path: file.path, displayName: file.displayName, type: 'generic', uri: file.uri, depth: 1, parentFolderKey: folderKey } satisfies BicepFileNode);
      }
    }

    return nodes;
  });

  protected readonly loadConfigPipelineFile = (filePath: string): Promise<string> => {
    const configId = this.config()?.id ?? '';
    return this.pipelineService.getFileContent(configId, filePath);
  };

  // ─── Inheritance ───
  protected readonly inheritanceLoading = signal(false);

  // ─── Configuration Diagnostics ───
  protected readonly diagnostics = signal<ResourceDiagnosticResponse[]>([]);
  protected readonly diagnosticsLoading = signal(false);
  private readonly diagnosticsByResourceId = computed(() => {
    const map = new Map<string, ResourceDiagnosticResponse[]>();
    for (const d of this.diagnostics()) {
      const existing = map.get(d.resourceId) ?? [];
      existing.push(d);
      map.set(d.resourceId, existing);
    }
    return map;
  });

  // ─── Cross-Config References ───
  protected readonly crossConfigReferences = signal<CrossConfigReferenceResponse[]>([]);
  protected readonly incomingCrossConfigReferences = signal<IncomingCrossConfigReferenceResponse[]>([]);
  protected readonly crossConfigLoading = signal(false);
  protected readonly crossConfigErrorKey = signal('');
  protected readonly crossConfigLoaded = signal(false);

  // ─── Pipeline Variable Groups ───
  protected readonly variableGroups = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoading = signal(false);
  protected readonly vgErrorKey = signal('');
  protected readonly vgLoaded = signal(false);

  // ─── Config Tags ───
  protected readonly isEditingConfigTags = signal(false);
  protected readonly editingConfigTags = signal<TagRequest[]>([]);
  protected readonly configTagsErrorKey = signal('');
  protected readonly configTagsSaving = signal(false);
  protected readonly configTagNameCtrl = new FormControl('', { nonNullable: true });
  protected readonly configTagValueCtrl = new FormControl('', { nonNullable: true });
  protected readonly configTags = computed(() => this.config()?.tags ?? []);

  protected readonly isMultiRepo = computed(() => this.project()?.repositoryMode === 'MultiRepo');

  // ─── Unified generation (multi-repo) ───
  protected readonly generateAllLoading = computed(
    () => this.bicepLoading() || this.pipelineLoading(),
  );
  protected readonly generationPanelOpen = computed(
    () => this.bicepPanelOpen() || this.pipelinePanelOpen() || this.bicepLoading() || this.pipelineLoading(),
  );

  protected async generateAll(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.generateAllLoading()) return;

    const shouldContinue = await this.showDiagnosticsDialog();
    if (!shouldContinue) return;

    await Promise.all([
      this.doGenerateBicep(),
      this.doGeneratePipeline(),
    ]);
  }

  protected closeGenerationPanel(): void {
    this.closeBicepPanel();
    this.closePipelinePanel();
  }

  // ─── Git Config (multi-repo, config-level display) ───
  protected readonly gitTestLoading = signal(false);
  protected readonly gitTestResult = signal<TestGitConnectionResponse | null>(null);
  protected readonly gitActionError = signal('');

  protected readonly useProjectNamingConventions = computed(() => this.config()?.useProjectNamingConventions ?? false);

  protected readonly projectSortedEnvironments = computed(() => {
    const envs = this.project()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
  });

  /** Resource types that have no per-environment settings. */
  private readonly ENV_SETTINGS_EXCLUDED_TYPES = new Set(['UserAssignedIdentity']);

  /**
   * Returns the list of environment names that are defined in the project
   * but not yet configured for the given resource. Returns empty array if
   * the resource type has no environment settings or if all environments are configured.
   */
  protected getMissingEnvironments(resource: AzureResourceResponse): string[] {
    if (this.ENV_SETTINGS_EXCLUDED_TYPES.has(resource.resourceType)) return [];
    const allEnvNames = this.projectSortedEnvironments().map(e => e.name);
    if (allEnvNames.length === 0) return [];
    const configured = new Set(resource.configuredEnvironments ?? []);
    return allEnvNames.filter(name => !configured.has(name));
  }

  /**
   * Returns true if the resource has at least one missing environment configuration.
   */
  protected hasMissingEnvironments(resource: AzureResourceResponse): boolean {
    return this.getMissingEnvironments(resource).length > 0;
  }

  protected getResourceDiagnostics(resourceId: string): ResourceDiagnosticResponse[] {
    return this.diagnosticsByResourceId().get(resourceId) ?? [];
  }

  protected hasResourceDiagnostics(resourceId: string): boolean {
    return this.getResourceDiagnostics(resourceId).length > 0;
  }

  // canWrite defaults to true — access checks are now at project level
  protected readonly canWrite = signal(true);

  protected readonly isOwner = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner';
  });

  protected readonly canAddResourceNamingTemplate = computed(() => {
    const configuredTypes = new Set((this.config()?.resourceNamingTemplates ?? []).map((item) => item.resourceType));
    return this.resourceTypeOptions.some((option) => !configuredTypes.has(option.value));
  });

  protected readonly effectiveEnvironments = computed(() => {
    return this.project()?.environmentDefinitions ?? [];
  });

  protected readonly sortedEnvironments = computed(() => {
    return [...this.effectiveEnvironments()].sort((a, b) => a.order - b.order);
  });

  protected readonly previewEnvId = signal<string | null>(null);

  protected readonly previewEnv = computed(() => {
    const id = this.previewEnvId();
    if (!id) return null;
    return this.effectiveEnvironments().find((e) => e.id === id) ?? null;
  });

  /**
   * Resolves a naming template preview for a resource, replacing placeholders
   * with values from the selected preview environment and the resource metadata.
   */
  protected resolveNamingPreview(resourceName: string, resourceType: string): string | null {
    const env = this.previewEnv();
    if (!env) return null;

    const cfg = this.config();
    if (!cfg) return null;

    // Pick the effective naming templates (project when inherited, config otherwise)
    const proj = this.project();
    const useProjectNaming = cfg.useProjectNamingConventions && proj;
    const namingTemplates = useProjectNaming ? proj.resourceNamingTemplates : cfg.resourceNamingTemplates;
    const defaultTemplate = useProjectNaming ? proj.defaultNamingTemplate : cfg.defaultNamingTemplate;
    const resourceOverride = namingTemplates.find((t) => t.resourceType === resourceType);
    const template = resourceOverride?.template ?? defaultTemplate;
    if (!template) return null;

    const replacements: Record<string, string> = {
      name: resourceName,
      prefix: env.prefix ?? '',
      suffix: env.suffix ?? '',
      env: env.name,
      envShort: env.shortName ?? '',
      resourceType,
      resourceAbbr: RESOURCE_TYPE_ABBREVIATIONS[resourceType] ?? resourceType.toLowerCase(),
      location: env.location,
    };

    return template.replace(/\{(\w+)}/g, (_, key: string) => replacements[key] ?? `{${key}}`);
  }

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('CONFIG_DETAIL.ERROR.NO_ID');
      return;
    }
    await this.loadConfig(id);

    // React to route param changes (e.g. cross-config navigation)
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(async (params) => {
      const newId = params.get('id');
      if (newId && newId !== this.config()?.id) {
        this.resetState();
        await this.loadConfig(newId);
      }
    });
  }

  private resetState(): void {
    this.config.set(null);
    this.project.set(null);
    this.resourceGroups.set([]);
    this.expandedRgId.set(null);
    this.rgResources.set({});
    this.expandedParentResources.set(new Set<string>());
    this.crossConfigReferences.set([]);
    this.incomingCrossConfigReferences.set([]);
    this.crossConfigLoaded.set(false);
    this.crossConfigErrorKey.set('');
    this.bicepResult.set(null);
    this.bicepPanelOpen.set(false);
    this.storageAccountDetails.set({});
    this.previewEnvId.set(null);
    this.diagnostics.set([]);
    this.loadError.set('');
  }

  private async loadConfig(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const [config, resourceGroups] = await Promise.all([
        this.infraConfigService.getById(id),
        this.infraConfigService.getResourceGroups(id),
      ]);
      this.config.set(config);
      this.resourceGroups.set(resourceGroups);

      // Load the parent project for inheritance data
      if (config.projectId) {
        try {
          const project = await this.projectService.getProject(config.projectId);
          this.project.set(project);
        } catch {
          // Non-blocking — project data used only for inheritance display
        }
      }

      // Pre-select the first effective environment (by order) for the naming preview
      const effectiveEnvs = this.project()?.environmentDefinitions ?? [];
      const firstEnv = [...effectiveEnvs].sort((a, b) => a.order - b.order)[0];
      if (firstEnv) {
        this.previewEnvId.set(firstEnv.id);
      }

      // Load cross-config references BEFORE expanding RGs so they appear in resource lists
      await this.loadCrossConfigReferences();
      await this.loadDiagnostics();

      await this.openDefaultResourceGroup(resourceGroups);
      this.recentlyViewedService.trackView({
        id: config.id,
        name: config.name,
        type: 'config',
      });
    } catch {
      this.loadError.set('CONFIG_DETAIL.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openAddResourceGroupDialog(): void {
    const currentConfig = this.config();
    if (!currentConfig) return;

    const dialogRef = this.dialog.open(AddResourceGroupDialogComponent, {
      data: {
        infraConfigId: currentConfig.id,
      } satisfies AddResourceGroupDialogData,
      width: '440px',
    });

    dialogRef.afterClosed().subscribe(async (result: import('../../shared/interfaces/resource-group.interface').ResourceGroupResponse | null) => {
      if (result) {
        try {
          const resourceGroups = await this.infraConfigService.getResourceGroups(currentConfig.id);
          this.resourceGroups.set(resourceGroups);
        } catch {
          this.rgErrorKey.set('CONFIG_DETAIL.RESOURCE_GROUPS.REFRESH_ERROR');
        }
      }
    });
  }

  protected async toggleRgExpand(rgId: string): Promise<void> {
    if (this.expandedRgId() === rgId) {
      this.expandedRgId.set(null);
      return;
    }
    this.expandedRgId.set(rgId);
    await this.loadRgResources(rgId);
  }

  private async loadRgResources(rgId: string): Promise<void> {
    if (this.rgResources()[rgId]) return;
    this.rgResourcesLoading.set(rgId);
    try {
      const resources = await this.resourceGroupService.getResources(rgId);
      this.rgResources.update((prev) => ({ ...prev, [rgId]: resources }));
      // Auto-expand all parent resources by default (local + cross-config virtual)
      const parentIds = resources
        .filter((r) => PARENT_CHILD_RESOURCE_TYPES[r.resourceType])
        .map((r) => r.id);
      const crossConfigParentIds = this.crossConfigReferences()
        .filter((ref) => PARENT_CHILD_RESOURCE_TYPES[ref.targetResourceType] && !parentIds.includes(ref.targetResourceId))
        .map((ref) => ref.targetResourceId);
      const allParentIds = [...parentIds, ...crossConfigParentIds];
      if (allParentIds.length > 0) {
        this.expandedParentResources.update((prev) => {
          const next = new Set(prev);
          allParentIds.forEach((id) => next.add(id));
          return next;
        });
      }
      // Auto-load StorageAccount details for expanded parents
      const storageIds = resources
        .filter((r) => r.resourceType === 'StorageAccount')
        .map((r) => r.id);
      for (const id of storageIds) {
        this.loadStorageAccountDetails(id);
      }
    } catch {
      this.rgResources.update((prev) => ({ ...prev, [rgId]: [] }));
    } finally {
      this.rgResourcesLoading.set(null);
    }
  }

  private async openDefaultResourceGroup(resourceGroups: ResourceGroupResponse[]): Promise<void> {
    if (resourceGroups.length === 0) {
      this.expandedRgId.set(null);
      return;
    }

    const defaultResourceGroup = resourceGroups[0];
    this.expandedRgId.set(defaultResourceGroup.id);
    await this.loadRgResources(defaultResourceGroup.id);
  }

  /**
   * Groups resources into a display-friendly structure: parent resources
   * with their children nested, and standalone resources listed separately.
   */
  protected groupResourcesForRg(rgId: string): ResourceDisplayItem[] {
    const resources = this.rgResources()[rgId] ?? [];
    if (resources.length === 0) return [];

    const parentMap = new Map<string, AzureResourceResponse>();
    const childrenByParent = new Map<string, AzureResourceResponse[]>();
    const standalone: AzureResourceResponse[] = [];

    // Index local parents
    for (const res of resources) {
      if (PARENT_CHILD_RESOURCE_TYPES[res.resourceType]) {
        parentMap.set(res.id, res);
        if (!childrenByParent.has(res.id)) {
          childrenByParent.set(res.id, []);
        }
      }
    }

    // Index cross-config references whose type is a parent type as virtual parents
    const crossConfigParentRefs = new Map<string, CrossConfigReferenceResponse>();
    for (const ref of this.crossConfigReferences()) {
      if (PARENT_CHILD_RESOURCE_TYPES[ref.targetResourceType] && !parentMap.has(ref.targetResourceId)) {
        crossConfigParentRefs.set(ref.targetResourceId, ref);
        if (!childrenByParent.has(ref.targetResourceId)) {
          childrenByParent.set(ref.targetResourceId, []);
        }
      }
    }

    // Assign children to their parents (local or cross-config); fallback to standalone
    for (const res of resources) {
      if (parentMap.has(res.id)) continue; // skip local parents
      if (CHILD_RESOURCE_TYPES.has(res.resourceType) && res.parentResourceId) {
        if (parentMap.has(res.parentResourceId)) {
          childrenByParent.get(res.parentResourceId)!.push(res);
        } else if (crossConfigParentRefs.has(res.parentResourceId)) {
          childrenByParent.get(res.parentResourceId)!.push(res);
        } else {
          standalone.push(res);
        }
      } else {
        standalone.push(res);
      }
    }

    // Build a map of incoming children per target resource (resources from OTHER configs that depend on THIS config's resources)
    const incomingByTarget = new Map<string, IncomingCrossConfigReferenceResponse[]>();
    for (const inc of this.incomingCrossConfigReferences()) {
      const existing = incomingByTarget.get(inc.targetResourceId) ?? [];
      existing.push(inc);
      incomingByTarget.set(inc.targetResourceId, existing);
    }

    const result: ResourceDisplayItem[] = [];

    // Emit local parents with their children + incoming children
    for (const [parentId, parent] of parentMap) {
      result.push({
        resource: parent,
        children: childrenByParent.get(parentId) ?? [],
        incomingChildren: incomingByTarget.get(parentId),
        isParent: true,
      });
    }

    // Emit cross-config parents that have local children
    for (const [refResourceId, ref] of crossConfigParentRefs) {
      const children = childrenByParent.get(refResourceId) ?? [];
      if (children.length > 0) {
        result.push({
          resource: {
            id: ref.targetResourceId,
            name: ref.targetResourceName,
            resourceType: ref.targetResourceType,
            location: '',
          },
          children,
          isParent: true,
          crossConfigRef: ref,
        });
      }
    }

    // Emit standalone resources
    for (const res of standalone) {
      result.push({ resource: res, isParent: false });
    }

    return result;
  }

  /**
   * Returns cross-config references that are NOT already used as parent groupings
   * in the given resource group (to avoid duplication in the standalone section).
   */
  protected getUnparentedCrossConfigRefs(rgId: string): CrossConfigReferenceResponse[] {
    const grouped = this.groupResourcesForRg(rgId);
    const parentedRefIds = new Set(
      grouped.filter((item) => item.crossConfigRef).map((item) => item.crossConfigRef!.referenceId),
    );
    return this.crossConfigReferences().filter((ref) => !parentedRefIds.has(ref.referenceId));
  }

  protected toggleParentExpand(parentId: string): void {
    this.expandedParentResources.update((prev) => {
      const next = new Set(prev);
      if (next.has(parentId)) {
        next.delete(parentId);
      } else {
        next.add(parentId);
      }
      return next;
    });
  }

  protected isParentExpanded(parentId: string): boolean {
    return this.expandedParentResources().has(parentId);
  }

  // ─── Storage Account sub-resource methods ───

  protected getStorageSubResourceCount(storageId: string): number {
    const details = this.storageAccountDetails()[storageId];
    if (!details) return 0;
    return (details.blobContainers?.length ?? 0) + (details.queues?.length ?? 0) + (details.tables?.length ?? 0);
  }

  protected async loadStorageAccountDetails(storageId: string): Promise<void> {
    if (this.storageDetailsLoading() === storageId) return;
    this.storageDetailsLoading.set(storageId);
    try {
      const details = await this.storageAccountService.getById(storageId);
      this.storageAccountDetails.update(prev => ({ ...prev, [storageId]: details }));
    } catch {
      // Non-blocking
    } finally {
      this.storageDetailsLoading.set(null);
    }
  }

  protected toggleStorageParentExpand(parentId: string): void {
    this.toggleParentExpand(parentId);
    if (this.isParentExpanded(parentId)) {
      this.loadStorageAccountDetails(parentId);
    }
  }

  private readonly publicAccessI18nMap: Record<string, string> = {
    None: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_NONE',
    Blob: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_BLOB',
    Container: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_CONTAINER',
  };

  protected publicAccessI18nKey(value: string): string {
    return this.publicAccessI18nMap[value] ?? value;
  }

  protected navigateToStorageTab(storageAccountId: string, tab: 'blob_containers' | 'queues' | 'tables'): void {
    const cfg = this.config();
    if (!cfg) return;
    this.router.navigate(['/config', cfg.id, 'resource', 'StorageAccount', storageAccountId], {
      queryParams: { tab },
    });
  }

  protected openAddStorageSubResourceDialog(storageAccountId: string): void {
    const details = this.storageAccountDetails()[storageAccountId];
    const dialogRef = this.dialog.open(AddStorageServiceDialogComponent, {
      data: {
        storageAccountId,
        existingBlobNames: (details?.blobContainers ?? []).map(b => b.name),
        existingQueueNames: (details?.queues ?? []).map(q => q.name),
        existingTableNames: (details?.tables ?? []).map(t => t.name),
      } satisfies AddStorageServiceDialogData,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe((result?: AddStorageServiceDialogResult) => {
      if (result) {
        this.storageAccountDetails.update(prev => ({
          ...prev,
          [storageAccountId]: result.storageAccountResponse,
        }));
      }
    });
  }

  protected removeStorageSubResource(storageAccountId: string, type: 'BlobContainer' | 'Queue' | 'Table', subResourceId: string, name: string): void {
    const typeLabel = type === 'BlobContainer' ? 'Blob Container' : type;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.STORAGE_SERVICES.REMOVE_CONFIRM_TITLE',
        titleParams: { type: typeLabel },
        messageKey: 'RESOURCE_EDIT.STORAGE_SERVICES.REMOVE_CONFIRM_MESSAGE',
        messageParams: { name, type: typeLabel },
        confirmKey: 'RESOURCE_EDIT.STORAGE_SERVICES.REMOVE_CONFIRM_YES',
        cancelKey: 'RESOURCE_EDIT.STORAGE_SERVICES.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      try {
        switch (type) {
          case 'BlobContainer':
            await this.storageAccountService.removeBlobContainer(storageAccountId, subResourceId);
            break;
          case 'Queue':
            await this.storageAccountService.removeQueue(storageAccountId, subResourceId);
            break;
          case 'Table':
            await this.storageAccountService.removeTable(storageAccountId, subResourceId);
            break;
        }
        const updated = await this.storageAccountService.getById(storageAccountId);
        this.storageAccountDetails.update(prev => ({ ...prev, [storageAccountId]: updated }));
      } catch {
        // Error handling already in snackbar
      }
    });
  }

  protected openAddResourceDialog(rgId: string): void {
    const rg = this.resourceGroups().find((r) => r.id === rgId);
    const envs = this.projectSortedEnvironments();
    const currentConfig = this.config();
    const dialogRef = this.dialog.open(AddResourceDialogComponent, {
      data: {
        resourceGroupId: rgId,
        configId: currentConfig!.id,
        projectId: currentConfig!.projectId,
        location: rg?.location ?? '',
        environments: envs.map(e => ({ name: e.name })),
      } satisfies AddResourceDialogData,
      width: '720px',
      maxHeight: '90vh',
    });

    dialogRef.afterClosed().subscribe(async (created: boolean) => {
      if (created) {
        this.rgResources.update((prev) => {
          const updated = { ...prev };
          delete updated[rgId];
          return updated;
        });
        await this.loadCrossConfigReferences();
        await this.loadRgResources(rgId);
      }
    });
  }

  protected openAddChildResourceDialog(parentResource: AzureResourceResponse, rgId: string): void {
    const rg = this.resourceGroups().find((r) => r.id === rgId);
    const envs = this.projectSortedEnvironments();
    const currentConfig = this.config();
    const dialogRef = this.dialog.open(AddResourceDialogComponent, {
      data: {
        resourceGroupId: rgId,
        configId: currentConfig!.id,
        projectId: currentConfig!.projectId,
        location: rg?.location ?? '',
        environments: envs.map(e => ({ name: e.name })),
        parentResource: {
          id: parentResource.id,
          name: parentResource.name,
          resourceType: parentResource.resourceType,
        },
      } satisfies AddResourceDialogData,
      width: '720px',
      maxHeight: '90vh',
    });

    dialogRef.afterClosed().subscribe(async (created: boolean) => {
      if (created) {
        this.rgResources.update((prev) => {
          const updated = { ...prev };
          delete updated[rgId];
          return updated;
        });
        await this.loadCrossConfigReferences();
        await this.loadRgResources(rgId);
      }
    });
  }

  protected isNamingActionActive(actionKey: string): boolean {
    return this.namingActionKey() === actionKey;
  }

  protected isResourceNamingTemplateBusy(resourceType: string): boolean {
    const actionKey = this.namingActionKey();
    return actionKey === `resource:${resourceType}` || actionKey === `resource-remove:${resourceType}`;
  }

  protected openDefaultNamingTemplateDialog(): void {
    const currentConfig = this.config();
    if (!currentConfig || !this.canWrite()) return;

    const dialogRef = this.dialog.open(AddNamingTemplateDialogComponent, {
      data: {
        mode: 'default',
        isEditMode: !!currentConfig.defaultNamingTemplate,
        template: currentConfig.defaultNamingTemplate,
      } satisfies AddNamingTemplateDialogData,
      width: '460px',
    });

    dialogRef.afterClosed().subscribe(async (result: AddNamingTemplateDialogResult | null) => {
      if (!result) return;
      await this.saveDefaultNamingTemplate(result.template);
    });
  }

  protected openResourceNamingTemplateDialog(existing?: ResourceNamingTemplateResponse): void {
    const currentConfig = this.config();
    if (!currentConfig || !this.canWrite()) return;

    const usedResourceTypes = currentConfig.resourceNamingTemplates
      .map((item) => item.resourceType)
      .filter((resourceType) => resourceType !== existing?.resourceType);

    const availableResourceTypes = this.resourceTypeOptions
      .map((option) => option.value)
      .filter((resourceType) => !usedResourceTypes.includes(resourceType));

    const dialogRef = this.dialog.open(AddNamingTemplateDialogComponent, {
      data: {
        mode: 'resource',
        isEditMode: !!existing,
        template: existing?.template ?? '',
        resourceType: existing?.resourceType,
        availableResourceTypes: existing ? [existing.resourceType] : availableResourceTypes,
      } satisfies AddNamingTemplateDialogData,
      width: '460px',
    });

    dialogRef.afterClosed().subscribe(async (result: AddNamingTemplateDialogResult | null) => {
      if (!result?.resourceType) return;
      await this.saveResourceNamingTemplate(result.resourceType, result.template);
    });
  }

  protected openRemoveResourceNamingTemplateDialog(template: ResourceNamingTemplateResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_TITLE',
        messageKey: 'CONFIG_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_MESSAGE',
        messageParams: { resourceType: template.resourceType },
        confirmKey: 'CONFIG_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.NAMING_TEMPLATES.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (!confirmed) return;
      await this.removeResourceNamingTemplate(template.resourceType);
    });
  }

  private async saveDefaultNamingTemplate(template: string): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.namingActionKey.set('default');
    this.namingErrorKey.set('');

    try {
      await this.infraConfigService.setDefaultNamingTemplate(configId, { template });
      await this.refreshConfig(configId);
    } catch {
      this.namingErrorKey.set('CONFIG_DETAIL.NAMING_TEMPLATES.DEFAULT_SAVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async saveResourceNamingTemplate(resourceType: string, template: string): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.namingActionKey.set(`resource:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.infraConfigService.setResourceNamingTemplate(configId, resourceType, { template });
      await this.refreshConfig(configId);
    } catch {
      this.namingErrorKey.set('CONFIG_DETAIL.NAMING_TEMPLATES.RESOURCE_SAVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async removeResourceNamingTemplate(resourceType: string): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.namingActionKey.set(`resource-remove:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.infraConfigService.removeResourceNamingTemplate(configId, resourceType);
      await this.refreshConfig(configId);
    } catch {
      this.namingErrorKey.set('CONFIG_DETAIL.NAMING_TEMPLATES.RESOURCE_REMOVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  protected async toggleInheritanceNaming(useProject: boolean): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.inheritanceLoading.set(true);
    try {
      await this.infraConfigService.setInheritance(configId, {
        useProjectNamingConventions: useProject,
      });
      await this.refreshConfig(configId);
    } finally {
      this.inheritanceLoading.set(false);
    }
  }

  private async refreshConfig(configId: string): Promise<void> {
    const refreshed = await this.infraConfigService.getById(configId);
    this.config.set(refreshed);
  }

  // ─── Delete Resource Group ───

  protected openDeleteResourceGroupDialog(rg: ResourceGroupResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.RESOURCE_GROUPS.DELETE_CONFIRM_TITLE',
        messageKey: 'CONFIG_DETAIL.RESOURCE_GROUPS.DELETE_CONFIRM_MESSAGE',
        messageParams: { name: rg.name },
        confirmKey: 'CONFIG_DETAIL.RESOURCE_GROUPS.DELETE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.RESOURCE_GROUPS.DELETE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      try {
        await this.resourceGroupService.delete(rg.id);
        const currentConfig = this.config();
        if (currentConfig) {
          const resourceGroups = await this.infraConfigService.getResourceGroups(currentConfig.id);
          this.resourceGroups.set(resourceGroups);
          if (this.expandedRgId() === rg.id) {
            this.expandedRgId.set(null);
          }
        }
      } catch {
        this.rgErrorKey.set('CONFIG_DETAIL.RESOURCE_GROUPS.DELETE_ERROR');
      }
    });
  }

  // ─── Delete Resource ───

  private readonly CASCADE_PARENT_TYPES = new Set(['LogAnalyticsWorkspace', 'AppServicePlan', 'SqlServer']);

  protected openDeleteResourceDialog(resource: AzureResourceResponse, rgId: string): void {
    if (this.CASCADE_PARENT_TYPES.has(resource.resourceType)) {
      this.openCascadeDeleteDialog(resource, rgId);
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.RESOURCES.DELETE_CONFIRM_TITLE',
        messageKey: 'CONFIG_DETAIL.RESOURCES.DELETE_CONFIRM_MESSAGE',
        messageParams: { name: resource.name, type: resource.resourceType },
        confirmKey: 'CONFIG_DETAIL.RESOURCES.DELETE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.RESOURCES.DELETE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.deleteResource(resource, rgId);
    });
  }

  private async openCascadeDeleteDialog(resource: AzureResourceResponse, rgId: string): Promise<void> {
    let dependents: DependentResourceResponse[] = [];
    try {
      switch (resource.resourceType) {
        case 'LogAnalyticsWorkspace':
          dependents = await this.logAnalyticsWorkspaceService.getDependents(resource.id);
          break;
        case 'AppServicePlan':
          dependents = await this.appServicePlanService.getDependents(resource.id);
          break;
        case 'SqlServer':
          dependents = await this.sqlServerService.getDependents(resource.id);
          break;
      }
    } catch {
      this.rgErrorKey.set('CONFIG_DETAIL.RESOURCES.DELETE_ERROR');
      return;
    }

    const dialogRef = this.dialog.open(CascadeDeleteDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.RESOURCES.CASCADE_DELETE_TITLE',
        messageKey: dependents.length > 0
          ? 'CONFIG_DETAIL.RESOURCES.CASCADE_DELETE_MESSAGE_WITH_DEPS'
          : 'CONFIG_DETAIL.RESOURCES.CASCADE_DELETE_MESSAGE_NO_DEPS',
        messageParams: { name: resource.name, type: resource.resourceType, count: dependents.length },
        dependentsHeaderKey: 'CONFIG_DETAIL.RESOURCES.CASCADE_DELETE_DEPENDENTS_HEADER',
        confirmKey: 'CONFIG_DETAIL.RESOURCES.DELETE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.RESOURCES.DELETE_CONFIRM_CANCEL',
        dependents,
        resourceTypeIcons: this.resourceTypeIcons,
      } satisfies CascadeDeleteDialogData,
      width: '480px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.deleteResource(resource, rgId);
    });
  }

  private async deleteResource(resource: AzureResourceResponse, rgId: string): Promise<void> {
    try {
      switch (resource.resourceType) {
        case 'KeyVault':
          await this.keyVaultService.delete(resource.id);
          break;
        case 'RedisCache':
          await this.redisCacheService.delete(resource.id);
          break;
        case 'StorageAccount':
          await this.storageAccountService.delete(resource.id);
          break;
        case 'AppServicePlan':
          await this.appServicePlanService.delete(resource.id);
          break;
        case 'WebApp':
          await this.webAppService.delete(resource.id);
          break;
        case 'FunctionApp':
          await this.functionAppService.delete(resource.id);
          break;
        case 'UserAssignedIdentity':
          await this.userAssignedIdentityService.delete(resource.id);
          break;
        case 'AppConfiguration':
          await this.appConfigurationService.delete(resource.id);
          break;
        case 'ContainerAppEnvironment':
          await this.containerAppEnvironmentService.delete(resource.id);
          break;
        case 'ContainerApp':
          await this.containerAppService.delete(resource.id);
          break;
        case 'LogAnalyticsWorkspace':
          await this.logAnalyticsWorkspaceService.delete(resource.id);
          break;
        case 'ApplicationInsights':
          await this.applicationInsightsService.delete(resource.id);
          break;
        case 'CosmosDb':
          await this.cosmosDbService.delete(resource.id);
          break;
        case 'SqlServer':
          await this.sqlServerService.delete(resource.id);
          break;
        case 'SqlDatabase':
          await this.sqlDatabaseService.delete(resource.id);
          break;
        case 'ServiceBusNamespace':
          await this.serviceBusNamespaceService.delete(resource.id);
          break;
        case 'ContainerRegistry':
          await this.containerRegistryService.delete(resource.id);
          break;
      }
      // Refresh resource list for this resource group
      this.rgResources.update((prev) => {
        const updated = { ...prev };
        delete updated[rgId];
        return updated;
      });
      await this.loadRgResources(rgId);
    } catch {
      this.rgErrorKey.set('CONFIG_DETAIL.RESOURCES.DELETE_ERROR');
    }
  }

  // ─── Delete Config ───

  protected async generateBicep(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.bicepLoading()) return;

    const shouldContinue = await this.showDiagnosticsDialog();
    if (!shouldContinue) return;

    await this.doGenerateBicep();
  }

  private async doGenerateBicep(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.bicepLoading()) return;

    this.bicepLoading.set(true);
    this.bicepErrorKey.set('');
    this.bicepResult.set(null);
    this.bicepPanelOpen.set(true);

    try {
      const result = await this.bicepService.generate({ infrastructureConfigId: configId });
      this.bicepResult.set(result);
    } catch (err: unknown) {
      const axios = await import('axios');
      if (axios.isAxiosError(err)) {
        const status = err.response?.status;
        if (status === 401 || status === 403) {
          this.bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_AUTH_ERROR');
        } else {
          this.bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_ERROR');
        }
      } else if (err instanceof Error && err.message.includes('access token')) {
        this.bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_AUTH_ERROR');
      } else {
        this.bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_ERROR');
      }
    } finally {
      this.bicepLoading.set(false);
    }
  }

  protected closeBicepPanel(): void {
    this.bicepPanelOpen.set(false);
    this.bicepResult.set(null);
    this.bicepErrorKey.set('');
  }

  protected openPushToGitDialog(): void {
    const configId = this.config()?.id;
    const projectId = this.config()?.projectId;
    const gitConfig = this.project()?.gitRepositoryConfiguration;
    if (!configId || !projectId || !gitConfig) return;

    const data: PushToGitDialogData = { configId, projectId, gitConfig };
    this.dialog.open(PushToGitDialogComponent, { width: '480px', data });
  }



  protected async downloadBicepFiles(): Promise<void> {
    const result = this.bicepResult();
    if (!result || this.bicepDownloading()) return;

    this.bicepDownloading.set(true);
    try {
      const configId = this.config()?.id;
      if (!configId) return;

      const blob = await this.bicepService.downloadZip(configId);
      const configName = this.config()?.name ?? 'bicep';
      saveAs(blob, `${configName}-bicep.zip`);
    } finally {
      this.bicepDownloading.set(false);
    }
  }

  // ─── Pipeline Generation ───

  protected async generatePipeline(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.pipelineLoading()) return;

    const shouldContinue = await this.showDiagnosticsDialog();
    if (!shouldContinue) return;

    await this.doGeneratePipeline();
  }

  private async doGeneratePipeline(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.pipelineLoading()) return;

    this.pipelineLoading.set(true);
    this.pipelineErrorKey.set('');
    this.pipelineResult.set(null);
    this.pipelinePanelOpen.set(true);

    try {
      const result = await this.pipelineService.generate({ infrastructureConfigId: configId });
      this.pipelineResult.set(result);
    } catch (err: unknown) {
      const axios = await import('axios');
      if (axios.isAxiosError(err)) {
        const status = err.response?.status;
        if (status === 401 || status === 403) {
          this.pipelineErrorKey.set('CONFIG_DETAIL.PIPELINE.GENERATE_AUTH_ERROR');
        } else {
          this.pipelineErrorKey.set('CONFIG_DETAIL.PIPELINE.GENERATE_ERROR');
        }
      } else {
        this.pipelineErrorKey.set('CONFIG_DETAIL.PIPELINE.GENERATE_ERROR');
      }
    } finally {
      this.pipelineLoading.set(false);
    }
  }

  protected closePipelinePanel(): void {
    this.pipelinePanelOpen.set(false);
    this.pipelineResult.set(null);
    this.pipelineErrorKey.set('');
  }

  protected async downloadPipelineFiles(): Promise<void> {
    const result = this.pipelineResult();
    if (!result || this.pipelineDownloading()) return;

    this.pipelineDownloading.set(true);
    try {
      const configId = this.config()?.id;
      if (!configId) return;

      const blob = await this.pipelineService.downloadZip(configId);
      const configName = this.config()?.name ?? 'pipeline';
      saveAs(blob, `${configName}-pipeline.zip`);
    } finally {
      this.pipelineDownloading.set(false);
    }
  }

  protected openDeleteConfigDialog(): void {
    const currentConfig = this.config();
    if (!currentConfig) return;

    const data: ConfirmDialogData = {
      titleKey: 'CONFIG_DETAIL.DELETE.CONFIRM_TITLE',
      messageKey: 'CONFIG_DETAIL.DELETE.CONFIRM_MESSAGE',
      messageParams: { name: currentConfig.name },
      confirmKey: 'CONFIG_DETAIL.DELETE.CONFIRM_YES',
      cancelKey: 'CONFIG_DETAIL.DELETE.CONFIRM_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      try {
        await this.infraConfigService.delete(currentConfig.id);
        // Navigate back to parent project if available, otherwise home
        const projectId = currentConfig.projectId;
        if (projectId) {
          this.router.navigate(['/projects', projectId]);
        } else {
          this.router.navigate(['/']);
        }
      } catch {
        this.loadError.set('CONFIG_DETAIL.DELETE.ERROR');
      }
    });
  }

  // ─── Generation Diagnostics Dialog ───

  private async showDiagnosticsDialog(): Promise<boolean> {
    const currentDiagnostics = this.diagnostics();
    const configId = this.config()?.id;
    const configName = this.config()?.name ?? '';
    if (!configId) return true;

    // Ensure all RG resources are loaded
    const rgs = this.resourceGroups();
    const allResources = { ...this.rgResources() };
    const rgIdsToLoad = rgs.filter(rg => allResources[rg.id] === undefined).map(rg => rg.id);
    if (rgIdsToLoad.length > 0) {
      const loaded = await Promise.all(
        rgIdsToLoad.map(async (rgId) => {
          try {
            const resources = await this.resourceGroupService.getResources(rgId);
            return { rgId, resources };
          } catch {
            return { rgId, resources: [] as AzureResourceResponse[] };
          }
        }),
      );
      for (const { rgId, resources } of loaded) {
        allResources[rgId] = resources;
      }
      this.rgResources.set(allResources);
    }

    // Collect missing environment settings from loaded resources
    const missingEnvResources: MissingEnvResource[] = [];
    for (const resources of Object.values(allResources)) {
      if (!resources) continue;
      for (const resource of resources) {
        const missing = this.getMissingEnvironments(resource);
        if (missing.length > 0) {
          missingEnvResources.push({
            resourceId: resource.id,
            resourceName: resource.name,
            resourceType: resource.resourceType,
            missingEnvironments: missing,
          });
        }
      }
    }

    const hasDiagnostics = currentDiagnostics.length > 0;
    const hasMissingEnvs = missingEnvResources.length > 0;

    if (!hasDiagnostics && !hasMissingEnvs) return true;

    const dialogData: GenerationDiagnosticsDialogData = {
      configDiagnostics: hasDiagnostics ? [{
        configId,
        configName,
        diagnostics: currentDiagnostics,
      }] : [],
      missingEnvConfigs: hasMissingEnvs ? [{
        configId,
        configName,
        resources: missingEnvResources,
      }] : undefined,
    };
    const dialogRef = this.dialog.open(GenerationDiagnosticsDialogComponent, {
      data: dialogData,
      width: '640px',
      maxHeight: '80vh',
    });
    const result = await firstValueFrom(dialogRef.afterClosed());
    return result === true;
  }

  // ─── Configuration Diagnostics ───

  private async loadDiagnostics(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;
    this.diagnosticsLoading.set(true);
    try {
      const result = await this.infraConfigService.getDiagnostics(configId);
      this.diagnostics.set(result.diagnostics);
    } catch {
      // Non-blocking — diagnostics are informational
    } finally {
      this.diagnosticsLoading.set(false);
    }
  }

  // ─── Cross-Config References ───

  protected async onTabChange(index: number): Promise<void> {
    // Tab 3 (0-indexed) is cross-config references — lazy load on first visit
    if (index === 3 && !this.crossConfigLoaded()) {
      await this.loadCrossConfigReferences();
    }
    // Tab 4 is pipeline variable groups — lazy load on first visit
    if (index === 4 && !this.vgLoaded()) {
      await this.loadVariableGroups();
    }
  }

  protected async loadCrossConfigReferences(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.crossConfigLoading.set(true);
    this.crossConfigErrorKey.set('');
    try {
      const [refs, incomingRefs] = await Promise.all([
        this.infraConfigService.getCrossConfigReferences(configId),
        this.infraConfigService.getIncomingCrossConfigReferences(configId),
      ]);
      this.crossConfigReferences.set(refs);
      this.incomingCrossConfigReferences.set(incomingRefs);
      this.crossConfigLoaded.set(true);
    } catch {
      this.crossConfigErrorKey.set('CONFIG_DETAIL.CROSS_CONFIG_REFS.LOAD_ERROR');
    } finally {
      this.crossConfigLoading.set(false);
    }
  }



  // ─── Git Configuration (multi-repo) ───

  protected openGitConfigDialog(): void {
    const proj = this.project();
    if (!proj) return;

    const data: GitConfigDialogData = {
      projectId: proj.id,
      existing: proj.gitRepositoryConfiguration,
    };

    const dialogRef = this.dialog.open(GitConfigDialogComponent, {
      width: '520px',
      data,
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.project.set(result);
        this.gitTestResult.set(null);
        this.gitActionError.set('');
      }
    });
  }

  protected async testGitConnection(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.gitTestLoading.set(true);
    this.gitTestResult.set(null);
    this.gitActionError.set('');

    try {
      const result = await this.projectService.testGitConnection(projectId);
      this.gitTestResult.set(result);
    } catch {
      this.gitActionError.set('PROJECT_DETAIL.GIT_CONFIG.TEST_FAILED');
    } finally {
      this.gitTestLoading.set(false);
    }
  }

  protected openRemoveGitConfigDialog(): void {
    const proj = this.project();
    if (!proj) return;

    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.GIT_CONFIG.REMOVE_CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.GIT_CONFIG.REMOVE_CONFIRM_MESSAGE',
      confirmKey: 'PROJECT_DETAIL.GIT_CONFIG.REMOVE_CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.GIT_CONFIG.REMOVE_CONFIRM_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      this.gitActionError.set('');
      try {
        await this.projectService.removeGitConfig(proj.id);
        this.project.update((p) => {
          if (!p) return p;
          return { ...p, gitRepositoryConfiguration: null };
        });
        this.gitTestResult.set(null);
      } catch {
        this.gitActionError.set('PROJECT_DETAIL.GIT_CONFIG.REMOVE_ERROR');
      }
    });
  }

  // ─── Pipeline Variable Groups ───

  private async loadVariableGroups(): Promise<void> {
    const projectId = this.config()?.projectId;
    if (!projectId) return;

    this.vgLoading.set(true);
    this.vgErrorKey.set('');
    try {
      const groups = await this.projectService.getPipelineVariableGroups(projectId);
      this.variableGroups.set(groups.map(g => ({ ...g, variables: g.variables ?? [] })));
      this.vgLoaded.set(true);
    } catch {
      this.vgErrorKey.set('CONFIG_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
    } finally {
      this.vgLoading.set(false);
    }
  }

  protected openAddVariableGroupDialog(): void {
    const dialogRef = this.dialog.open(AddVariableGroupDialogComponent, {
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (groupName?: string) => {
      if (!groupName) return;
      const projectId = this.config()?.projectId;
      if (!projectId) return;

      this.vgErrorKey.set('');
      try {
        const newGroup = await this.projectService.addPipelineVariableGroup(projectId, { groupName });
        this.variableGroups.update(groups => [...groups, { ...newGroup, variables: newGroup.variables ?? [] }]);
      } catch {
        this.vgErrorKey.set('CONFIG_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
      }
    });
  }

  protected openRemoveVariableGroupDialog(group: ProjectPipelineVariableGroupResponse): void {
    const data: ConfirmDialogData = {
      titleKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      messageKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.CONFIRM_DELETE_GROUP',
      confirmKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      cancelKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.DIALOG_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      const projectId = this.config()?.projectId;
      if (!projectId) return;

      this.vgErrorKey.set('');
      try {
        await this.projectService.removePipelineVariableGroup(projectId, group.id);
        this.variableGroups.update(groups => groups.filter(g => g.id !== group.id));
      } catch {
        this.vgErrorKey.set('CONFIG_DETAIL.PIPELINE_VARIABLES.ERROR_REMOVE_GROUP');
      }
    });
  }

  // ─── Config Tags ───

  protected startEditConfigTags(): void {
    this.editingConfigTags.set(this.configTags().map(t => ({ name: t.name, value: t.value })));
    this.configTagNameCtrl.reset();
    this.configTagValueCtrl.reset();
    this.configTagsErrorKey.set('');
    this.isEditingConfigTags.set(true);
  }

  protected addConfigTag(): void {
    const name = this.configTagNameCtrl.value.trim();
    const value = this.configTagValueCtrl.value.trim();
    if (!name) return;
    this.editingConfigTags.update(tags => [
      ...tags.filter(t => t.name !== name),
      { name, value },
    ]);
    this.configTagNameCtrl.reset();
    this.configTagValueCtrl.reset();
  }

  protected removeConfigTag(name: string): void {
    this.editingConfigTags.update(tags => tags.filter(t => t.name !== name));
  }

  protected cancelConfigTagsEdit(): void {
    this.isEditingConfigTags.set(false);
    this.editingConfigTags.set([]);
    this.configTagsErrorKey.set('');
  }

  protected async saveConfigTags(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.configTagsSaving()) return;
    this.configTagsSaving.set(true);
    this.configTagsErrorKey.set('');
    try {
      await this.infraConfigService.setTags(configId, { tags: this.editingConfigTags() });
      this.config.update(c => c ? { ...c, tags: this.editingConfigTags() } : c);
      this.isEditingConfigTags.set(false);
      this.editingConfigTags.set([]);
    } catch {
      this.configTagsErrorKey.set('CONFIG_DETAIL.TAGS.SAVE_ERROR');
    } finally {
      this.configTagsSaving.set(false);
    }
  }
}
