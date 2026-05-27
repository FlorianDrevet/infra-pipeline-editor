import { Component, DestroyRef, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsTabsComponent } from '../../shared/components/ds/ds-tabs/ds-tabs.component';
import { DsTabDefinition } from '../../shared/components/ds/ds-tabs/ds-tabs.types';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  InfrastructureConfigResponse,
  ResourceNamingTemplateResponse,
} from '../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse, AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  DsSelectOption,
  DsButtonComponent,
} from '../../shared/components/ds';
import {
  EditAbbreviationDialogComponent,
  EditAbbreviationDialogData,
  EditAbbreviationDialogResult,
} from '../../shared/components/edit-abbreviation-dialog/edit-abbreviation-dialog.component';
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
import { CustomDomainDiagnosticsService } from '../../shared/services/custom-domain-diagnostics.service';
import { CascadeDeleteDialogComponent, CascadeDeleteDialogData } from '../../shared/components/cascade-delete-dialog/cascade-delete-dialog.component';
import { DependentResourceResponse } from '../../shared/interfaces/dependent-resource.interface';
import { ResourceDiagnosticResponse } from '../../shared/interfaces/bicep-generator.interface';
import { PendingCustomDomainIssue } from '../../shared/interfaces/pending-custom-domain-issue.interface';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import {
  CHILD_RESOURCE_TYPES,
  PARENT_CHILD_RESOURCE_TYPES,
  RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS,
  RESOURCE_TYPE_ABBREVIATIONS,
  RESOURCE_TYPE_ICONS,
  RESOURCE_TYPE_OPTIONS,
} from '../../shared/resource-metadata/resource-type.metadata';
import { MatChipsModule } from '@angular/material/chips';
import { StorageAccountSubResourcesResponse } from '../../shared/interfaces/storage-account.interface';
import { AddStorageServiceDialogComponent, AddStorageServiceDialogData, AddStorageServiceDialogResult } from './add-storage-service-dialog/add-storage-service-dialog.component';
import { PushToGitDialogComponent, PushToGitDialogData } from './push-to-git-dialog/push-to-git-dialog.component';
import {
  CrossConfigReferenceResponse,
  IncomingCrossConfigReferenceResponse,
} from '../../shared/interfaces/cross-config-reference.interface';
import {
  GenerationDiagnosticsDialogComponent,
  GenerationDiagnosticsDialogData,
  MissingEnvResource,
} from '../../shared/components/generation-diagnostics-dialog/generation-diagnostics-dialog.component';
import { firstValueFrom } from 'rxjs';
import { sortResourceTypesAlphabetically } from './resource-type-sort.helpers';
import { getMissingEnvironmentNames, resolveNamingPreview as resolveNamingPreviewFromContext } from './helpers/config-detail-naming.helpers';
import {
  ResourceDisplayItem,
  buildGroupedResourcesForResourceGroup,
  getUnparentedCrossConfigReferences,
} from './helpers/config-detail-resource-grouping.helpers';
import { ConfigDetailGenerationSectionComponent } from './sections/generation/config-detail-generation-section.component';
import { createConfigDetailGenerationSectionController } from './sections/generation/config-detail-generation-section.controller';
import { ConfigDetailGitSectionComponent } from './sections/git/config-detail-git-section.component';
import { createConfigDetailGitSectionController } from './sections/git/config-detail-git-section.controller';
import { ConfigDetailNamingSectionComponent } from './sections/naming/config-detail-naming-section.component';
import { ConfigDetailNamingSectionViewModel } from './sections/naming/config-detail-naming-section.view-model';
import { ConfigDetailResourcesSectionComponent } from './sections/resources/config-detail-resources-section.component';
import { ConfigDetailResourcesSectionViewModel } from './sections/resources/config-detail-resources-section.view-model';
import { ConfigDetailTagsSectionComponent } from './sections/tags/config-detail-tags-section.component';
import { createConfigDetailTagsSectionController } from './sections/tags/config-detail-tags-section.controller';
import { ConfigDetailVariableGroupsSectionComponent } from './sections/variable-groups/config-detail-variable-groups-section.component';
import { createConfigDetailVariableGroupsSectionController } from './sections/variable-groups/config-detail-variable-groups-section.controller';
import {
  CONFIG_DETAIL_ROUTE_TABS,
  CONFIG_DETAIL_TAB_IDS,
  ConfigDetailTabId,
  getConfigDetailQueryFromTabId,
  getConfigDetailTabIdFromQuery,
} from '../../shared/enums/detail-route-tabs';
import { LanguageService } from '../../shared/services/language.service';

type ResourceGroupResourcesById = { [rgId: string]: AzureResourceResponse[] | undefined };


@Component({
  selector: 'app-config-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    MatIconModule,
    DsSpinnerComponent,
    DsTabsComponent,
    MatTooltipModule,
    ConfigDetailGenerationSectionComponent,
    ConfigDetailGitSectionComponent,
    ConfigDetailNamingSectionComponent,
    ConfigDetailResourcesSectionComponent,
    ConfigDetailTagsSectionComponent,
    ConfigDetailVariableGroupsSectionComponent,
    DsButtonComponent,
  ],
  templateUrl: './config-detail.component.html',
  styleUrl: './config-detail.component.scss',
})
export class ConfigDetailComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly routeQueryParamMap = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap,
  });
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
  private readonly customDomainDiagnosticsService = inject(CustomDomainDiagnosticsService);
  private readonly authService = inject(AuthenticationService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);
  private readonly pageContextService = inject(PageContextService);
  private readonly sidebarContextService = inject(SidebarContextService);

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
  protected readonly storageAccountDetails = signal<Record<string, StorageAccountSubResourcesResponse | undefined>>({});
  protected readonly storageDetailsLoading = signal<string | null>(null);

  protected readonly generationSection = createConfigDetailGenerationSectionController({
    getConfig: () => this.config(),
    isProjectMultiRepo: () => this.isProjectMultiRepo(),
    showDiagnosticsDialog: () => this.showDiagnosticsDialog(),
  });

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

  protected readonly tagsSection = createConfigDetailTagsSectionController({
    getConfigId: () => this.config()?.id ?? null,
    getConfigTags: () => this.config()?.tags ?? [],
    updateConfigTags: (tags) => {
      this.config.update((currentConfig) => currentConfig ? { ...currentConfig, tags } : currentConfig);
    },
  });

  protected readonly variableGroupsSection = createConfigDetailVariableGroupsSectionController({
    getProjectId: () => this.config()?.projectId ?? null,
    getConfigName: () => this.config()?.name ?? '',
  });

  /** Project layout preset narrowed to the values used by this component. */
  protected readonly projectLayoutPreset = computed<'AllInOne' | 'SplitInfraCode' | 'MultiRepo'>(() => {
    const preset = this.project()?.layoutPreset;
    if (preset === 'SplitInfraCode' || preset === 'MultiRepo') return preset;
    return 'AllInOne';
  });

  protected readonly isProjectMultiRepo = computed(() => this.projectLayoutPreset() === 'MultiRepo');

  protected readonly gitSection = createConfigDetailGitSectionController({
    getConfig: () => this.config(),
    canWrite: () => this.canWrite(),
    updateConfig: (config) => this.config.set(config),
  });

  protected readonly useProjectNamingConventions = computed(() => this.config()?.useProjectNamingConventions ?? false);

  protected readonly projectSortedEnvironments = computed(() => {
    const envs = this.project()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
  });

  /**
   * Returns the list of environment names that are defined in the project
   * but not yet configured for the given resource. Returns empty array if
   * the resource type has no environment settings or if all environments are configured.
   */
  protected getMissingEnvironments(resource: AzureResourceResponse): string[] {
    return getMissingEnvironmentNames(resource, this.projectSortedEnvironments(), RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS);
  }

  /**
   * Returns true if the resource has at least one missing environment configuration.
   */
  protected hasMissingEnvironments(resource: AzureResourceResponse): boolean {
    if (resource.isExisting) return false;
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

  private readonly translate = inject(TranslateService);
  private readonly languageService = inject(LanguageService);
  private readonly previewNoneLabel = this.translate.instant('CONFIG_DETAIL.RESOURCE_GROUPS.PREVIEW_NONE');

  private readonly breadcrumbEffect = effect(() => {
    const project = this.project();
    const config = this.config();
    const projectsLabel = this.translate.instant('NAV.BREADCRUMB.PROJECTS') as string;
    const segments: { label: string; routerLink?: string }[] = [
      { label: projectsLabel, routerLink: '/' },
    ];
    if (project) {
      segments.push({ label: project.name, routerLink: `/projects/${project.id}` });
    }
    if (config) {
      segments.push({ label: config.name });
    }
    this.pageContextService.setBreadcrumb(segments);
  });
  protected readonly previewEnvDsOptions = computed<DsSelectOption[]>(() => [
    { value: null, label: this.previewNoneLabel },
    ...this.sortedEnvironments().map((env) => ({ value: env.id, label: env.name })),
  ]);

  protected readonly previewEnv = computed(() => {
    const id = this.previewEnvId();
    if (!id) return null;
    return this.effectiveEnvironments().find((e) => e.id === id) ?? null;
  });
  private readonly currentTabQuery = computed(() => this.routeQueryParamMap().get('tab'));
  private readonly normalizeUnavailableGitTabEffect = effect(() => {
    const project = this.project();

    if (!project || this.currentTabQuery() !== CONFIG_DETAIL_ROUTE_TABS.git || this.isProjectMultiRepo()) {
      return;
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    }).catch(() => undefined);
  });
  protected readonly activeConfigTabId = computed<ConfigDetailTabId>(() => {
    const tabId = getConfigDetailTabIdFromQuery(this.currentTabQuery());
    if (tabId === 'git' && !this.isProjectMultiRepo()) {
      return 'resource-groups';
    }

    return tabId;
  });
  protected readonly configDetailTabs = computed<readonly DsTabDefinition[]>(() => {
    this.languageService.currentLanguage();
    const tabs: DsTabDefinition[] = [
      { id: 'resource-groups', label: this.translate.instant('CONFIG_DETAIL.TABS.RESOURCE_GROUPS'), icon: 'dns', badge: String(this.resourceGroups().length) },
      { id: 'tags', label: this.translate.instant('CONFIG_DETAIL.TABS.TAGS'), icon: 'label_important', badge: String(this.tagsSection.configTags().length) },
      { id: 'naming', label: this.translate.instant('CONFIG_DETAIL.TABS.NAMING_TEMPLATES'), icon: 'label', badge: String(this.config()?.resourceNamingTemplates.length ?? 0) },
      { id: 'cross-config-refs', label: this.translate.instant('CONFIG_DETAIL.TABS.CROSS_CONFIG_REFS'), icon: 'link', badge: String(this.crossConfigReferences().length) },
      { id: 'variables', label: this.translate.instant('CONFIG_DETAIL.TABS.PIPELINE_VARIABLES'), icon: 'library_books', badge: String(this.variableGroupsSection.configVariableGroups().length) },
    ];
    if (this.isProjectMultiRepo()) {
      tabs.push({ id: 'git', label: this.translate.instant('CONFIG_DETAIL.TABS.GIT'), icon: 'code' });
    }
    return tabs;
  });
  protected async onConfigTabIdChange(tabId: string): Promise<void> {
    const typedTabId = CONFIG_DETAIL_TAB_IDS.includes(tabId as ConfigDetailTabId)
      ? (tabId as ConfigDetailTabId)
      : 'resource-groups';
    const tabQuery = getConfigDetailQueryFromTabId(typedTabId);
    if (tabQuery !== this.currentTabQuery()) {
      await this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { tab: tabQuery },
        queryParamsHandling: 'merge',
      });
    }

    if (typedTabId === 'cross-config-refs' && !this.crossConfigLoaded()) {
      await this.loadCrossConfigReferences();
    }
  }
  protected readonly resourcesSectionViewModel = computed<ConfigDetailResourcesSectionViewModel | null>(() => {
    const config = this.config();
    if (!config) {
      return null;
    }

    return {
      configId: config.id,
      canWrite: this.canWrite(),
      resourceGroups: this.resourceGroups(),
      sortedEnvironments: this.sortedEnvironments(),
      previewEnvOptions: this.previewEnvDsOptions(),
      previewEnvId: this.previewEnvId(),
      rgErrorKey: this.rgErrorKey(),
      expandedRgId: this.expandedRgId(),
      rgResources: this.rgResources(),
      rgResourcesLoading: this.rgResourcesLoading(),
      resourceTypeIcons: this.resourceTypeIcons,
      storageAccountDetails: this.storageAccountDetails(),
      storageDetailsLoading: this.storageDetailsLoading(),
      onSetPreviewEnvId: (environmentId) => this.previewEnvId.set(environmentId),
      onOpenAddResourceGroupDialog: () => this.openAddResourceGroupDialog(),
      onToggleRgExpand: (resourceGroupId) => void this.toggleRgExpand(resourceGroupId),
      getGroupedResources: (resourceGroupId) => this.groupResourcesForRg(resourceGroupId),
      resolveNamingPreview: (resourceName, resourceType) => this.resolveNamingPreview(resourceName, resourceType),
      hasMissingEnvironments: (resource) => this.hasMissingEnvironments(resource),
      getMissingEnvironments: (resource) => this.getMissingEnvironments(resource),
      hasResourceDiagnostics: (resourceId) => this.hasResourceDiagnostics(resourceId),
      getResourceDiagnostics: (resourceId) => this.getResourceDiagnostics(resourceId),
      onOpenAddResourceDialog: (resourceGroupId) => this.openAddResourceDialog(resourceGroupId),
      onOpenDeleteResourceGroupDialog: (resourceGroup) => this.openDeleteResourceGroupDialog(resourceGroup),
      onOpenDeleteResourceDialog: (resource, resourceGroupId) => {
        this.openDeleteResourceDialog(resource, resourceGroupId);
      },
      isParentExpanded: (parentId) => this.isParentExpanded(parentId),
      onToggleParentExpand: (parentId) => this.toggleParentExpand(parentId),
      onToggleStorageParentExpand: (parentId) => this.toggleStorageParentExpand(parentId),
      getStorageSubResourceCount: (storageAccountId) => this.getStorageSubResourceCount(storageAccountId),
      publicAccessI18nKey: (value) => this.publicAccessI18nKey(value),
      onNavigateToStorageTab: (storageAccountId, tab) => this.navigateToStorageTab(storageAccountId, tab),
      onOpenAddStorageSubResourceDialog: (storageAccountId) => this.openAddStorageSubResourceDialog(storageAccountId),
      onOpenAddChildResourceDialog: (parentResource, resourceGroupId) => this.openAddChildResourceDialog(parentResource, resourceGroupId),
      getUnparentedCrossConfigRefs: (resourceGroupId) => this.getUnparentedCrossConfigRefs(resourceGroupId),
    };
  });
  protected readonly namingSectionViewModel = computed<ConfigDetailNamingSectionViewModel | null>(() => {
    const config = this.config();
    if (!config) {
      return null;
    }

    return {
      config,
      project: this.project(),
      canWrite: this.canWrite(),
      useProjectNamingConventions: this.useProjectNamingConventions(),
      inheritanceLoading: this.inheritanceLoading(),
      namingActionKey: this.namingActionKey(),
      namingErrorKey: this.namingErrorKey(),
      canAddResourceNamingTemplate: this.canAddResourceNamingTemplate(),
      resourceTypeIcons: this.resourceTypeIcons,
      abbreviationDisplayItems: this.abbreviationDisplayItems(),
      isNamingActionActive: (actionKey) => this.isNamingActionActive(actionKey),
      isResourceNamingTemplateBusy: (resourceType) => this.isResourceNamingTemplateBusy(resourceType),
      isAbbreviationBusy: (resourceType) => this.isAbbreviationBusy(resourceType),
      onToggleInheritanceNaming: (useProject) => void this.toggleInheritanceNaming(useProject),
      onOpenDefaultNamingTemplateDialog: () => this.openDefaultNamingTemplateDialog(),
      onOpenResourceNamingTemplateDialog: (existing) => this.openResourceNamingTemplateDialog(existing),
      onOpenRemoveResourceNamingTemplateDialog: (template) => this.openRemoveResourceNamingTemplateDialog(template),
      onOpenEditAbbreviationDialog: (item) => this.openEditAbbreviationDialog(item),
      onOpenResetAbbreviationDialog: (item) => this.openResetAbbreviationDialog(item),
    };
  });

  /**
   * Resolves a naming template preview for a resource, replacing placeholders
   * with values from the selected preview environment and the resource metadata.
   */
  protected resolveNamingPreview(resourceName: string, resourceType: string): string | null {
    return resolveNamingPreviewFromContext({
      resourceName,
      resourceType,
      environment: this.previewEnv(),
      config: this.config(),
      project: this.project(),
    });
  }

  ngOnInit(): void {
    void this.initializeComponent();
  }

  public ngOnDestroy(): void {
    this.pageContextService.clear();
  }

  private async initializeComponent(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('CONFIG_DETAIL.ERROR.NO_ID');
      return;
    }
    await this.loadConfig(id);

    // React to route param changes (e.g. cross-config navigation)
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      void this.handleRouteParamChange(params.get('id'));
    });
  }

  private async handleRouteParamChange(newId: string | null): Promise<void> {
    if (newId && newId !== this.config()?.id) {
      this.resetState();
      await this.loadConfig(newId);
    }
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
    this.generationSection.reset();
    this.gitSection.reset();
    this.tagsSection.reset();
    this.variableGroupsSection.reset();
    this.storageAccountDetails.set({});
    this.previewEnvId.set(null);
    this.diagnostics.set([]);
    this.loadError.set('');
  }

  private async loadConfig(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      // Phase 1 — critical data (blocks rendering)
      const [config, resourceGroups] = await Promise.all([
        this.infraConfigService.getById(id),
        this.infraConfigService.getResourceGroups(id),
      ]);
      this.config.set(config);
      this.resourceGroups.set(resourceGroups);
      this.sidebarContextService.setConfigContext(config.id, config.name, config.projectId);

      // Phase 2 — secondary data + auto-expand first RG (all independent, fire in parallel)
      const projectPromise = config.projectId
        ? this.projectService
            .getProject(config.projectId)
            .then((project) => {
              this.project.set(project);
              this.sidebarContextService.setConfigContext(
                config.id,
                config.name,
                config.projectId,
                project.layoutPreset === 'MultiRepo'
              );
              // Pre-select the first effective environment for the naming preview
              const effectiveEnvs = project?.environmentDefinitions ?? [];
              const firstEnv = [...effectiveEnvs].sort((a, b) => a.order - b.order)[0];
              if (firstEnv) {
                this.previewEnvId.set(firstEnv.id);
              }
            })
            .catch(() => {
              /* Non-blocking — project data used only for inheritance display */
            })
        : Promise.resolve();

      await Promise.all([
        projectPromise,
        this.openDefaultResourceGroup(resourceGroups),
      ]);

      // Non-blocking: fire-and-forget secondary data (diagnostics, variable groups, cross-config refs)
      this.loadDiagnostics().catch(() => {});
      this.variableGroupsSection.load().catch(() => {});
      this.loadCrossConfigReferences().catch(() => {});

      this.recentlyViewedService.trackView({
        id: config.id,
        name: config.name,
        type: 'config',
      });

      // Auto-trigger generation if redirected from /config/:id/generate
      if (this.route.snapshot.queryParamMap.get('generate') === 'true') {
        void this.generationSection.generateAll();
      }
    } catch {
      this.loadError.set('CONFIG_DETAIL.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openAddResourceGroupDialog(): void { // NOSONAR S3776 - tracked under test-debt #22
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
      this.cacheStorageSubResources(resources);
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
    } catch {
      this.rgResources.update((prev) => ({ ...prev, [rgId]: [] }));
    } finally {
      this.rgResourcesLoading.set(null);
    }
  }

  private cacheStorageSubResources(resources: AzureResourceResponse[]): void {
    const storageSubResourcesById: Record<string, StorageAccountSubResourcesResponse> = {};

    for (const resource of resources) {
      if (resource.resourceType !== 'StorageAccount' || !resource.storageSubResources) {
        continue;
      }

      storageSubResourcesById[resource.id] = resource.storageSubResources;
    }

    if (Object.keys(storageSubResourcesById).length === 0) {
      return;
    }

    this.storageAccountDetails.update((prev) => ({
      ...prev,
      ...storageSubResourcesById,
    }));
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
    return buildGroupedResourcesForResourceGroup({
      resources: this.rgResources()[rgId] ?? [],
      crossConfigReferences: this.crossConfigReferences(),
      incomingCrossConfigReferences: this.incomingCrossConfigReferences(),
      parentChildResourceTypes: PARENT_CHILD_RESOURCE_TYPES,
      childResourceTypes: CHILD_RESOURCE_TYPES,
    });
  }

  /**
   * Returns cross-config references that are NOT already used as parent groupings
   * in the given resource group (to avoid duplication in the standalone section).
   */
  protected getUnparentedCrossConfigRefs(rgId: string): CrossConfigReferenceResponse[] {
    return getUnparentedCrossConfigReferences(
      this.groupResourcesForRg(rgId),
      this.crossConfigReferences(),
    );
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
    if (this.storageAccountDetails()[storageId]) return; // already cached
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

    const usedResourceTypes = new Set(
      currentConfig.resourceNamingTemplates
        .map((item) => item.resourceType)
        .filter((resourceType) => resourceType !== existing?.resourceType),
    );

    const availableResourceTypes = this.resourceTypeOptions
      .map((option) => option.value)
      .filter((resourceType) => !usedResourceTypes.has(resourceType));

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

  // ─── Abbreviation Overrides ───

  protected readonly abbreviationDisplayItems = computed(() => {
    const rgs = this.resourceGroups();
    const resources = this.rgResources();
    const overrides = this.config()?.resourceAbbreviationOverrides ?? [];

    // Collect all resource types used in this config
    const usedTypes = new Set<string>();
    for (const rg of rgs) {
      const rgRes = resources[rg.id];
      if (rgRes) {
        for (const r of rgRes) {
          usedTypes.add(r.resourceType);
        }
      }
    }
    // Always include ResourceGroup itself
    usedTypes.add('ResourceGroup');

    const overrideMap = new Map(overrides.map((o) => [o.resourceType, o]));

    return sortResourceTypesAlphabetically(usedTypes).map((resourceType) => {
      const override = overrideMap.get(resourceType);
      const defaultAbbr = RESOURCE_TYPE_ABBREVIATIONS[resourceType] ?? resourceType.toLowerCase();
      return {
        resourceType,
        defaultAbbreviation: defaultAbbr,
        customAbbreviation: override?.abbreviation ?? null,
        effectiveAbbreviation: override?.abbreviation ?? defaultAbbr,
        isCustomized: !!override,
      };
    });
  });

  protected isAbbreviationBusy(resourceType: string): boolean {
    const actionKey = this.namingActionKey();
    return actionKey === `abbr:${resourceType}` || actionKey === `abbr-remove:${resourceType}`;
  }

  protected openEditAbbreviationDialog(item: { resourceType: string; defaultAbbreviation: string; effectiveAbbreviation: string }): void {
    if (!this.canWrite()) return;

    const dialogRef = this.dialog.open(EditAbbreviationDialogComponent, {
      data: {
        resourceType: item.resourceType,
        defaultAbbreviation: item.defaultAbbreviation,
        currentAbbreviation: item.effectiveAbbreviation,
      } satisfies EditAbbreviationDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (result: EditAbbreviationDialogResult | null) => {
      if (!result) return;
      await this.saveAbbreviationOverride(item.resourceType, result.abbreviation);
    });
  }

  protected openResetAbbreviationDialog(item: { resourceType: string; defaultAbbreviation: string }): void {
    if (!this.canWrite()) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'ABBREVIATIONS.RESET_CONFIRM_TITLE',
        messageKey: 'ABBREVIATIONS.RESET_CONFIRM_MESSAGE',
        messageParams: { resourceType: item.resourceType, defaultAbbr: item.defaultAbbreviation },
        confirmKey: 'ABBREVIATIONS.RESET_CONFIRM_YES',
        cancelKey: 'ABBREVIATIONS.RESET_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (!confirmed) return;
      await this.removeAbbreviationOverride(item.resourceType);
    });
  }

  private async saveAbbreviationOverride(resourceType: string, abbreviation: string): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.namingActionKey.set(`abbr:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.infraConfigService.setResourceAbbreviationOverride(configId, resourceType, { abbreviation });
      await this.refreshConfig(configId);
    } catch {
      this.namingErrorKey.set('ABBREVIATIONS.SAVE_ERROR');
    } finally {
      this.namingActionKey.set(null);
    }
  }

  private async removeAbbreviationOverride(resourceType: string): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.namingActionKey.set(`abbr-remove:${resourceType}`);
    this.namingErrorKey.set('');

    try {
      await this.infraConfigService.removeResourceAbbreviationOverride(configId, resourceType);
      await this.refreshConfig(configId);
    } catch {
      this.namingErrorKey.set('ABBREVIATIONS.RESET_ERROR');
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

  protected openPushToGitDialog(): void {
    const configId = this.config()?.id;
    const projectId = this.config()?.projectId;
    if (!configId || !projectId) return;

    const data: PushToGitDialogData = { configId, projectId };
    this.dialog.open(PushToGitDialogComponent, { width: '480px', data });
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
    const currentConfig = this.config();
    const configId = currentConfig?.id;
    if (!configId) return true;

    const currentDiagnostics = this.diagnostics();
    const allResources = await this.getAllResourceGroupResources();
    const missingEnvResources = this.collectMissingEnvResources(allResources);
    const pendingCustomDomains = await this.customDomainDiagnosticsService.collectPendingIssues(
      Object.values(allResources).flatMap((resources) => resources ?? []),
    );

    if (currentDiagnostics.length === 0
      && missingEnvResources.length === 0
      && pendingCustomDomains.length === 0) {
      return true;
    }

    const dialogData = this.buildGenerationDiagnosticsDialogData(
      configId,
      currentConfig.name,
      currentDiagnostics,
      missingEnvResources,
      pendingCustomDomains,
    );
    const dialogRef = this.dialog.open(GenerationDiagnosticsDialogComponent, {
      data: dialogData,
      width: '640px',
      maxHeight: '80vh',
    });
    const result = await firstValueFrom(dialogRef.afterClosed());
    return result === true;
  }

  private async getAllResourceGroupResources(): Promise<ResourceGroupResourcesById> {
    const allResources: ResourceGroupResourcesById = { ...this.rgResources() };
    const rgIdsToLoad = this.resourceGroups()
      .filter((resourceGroup) => allResources[resourceGroup.id] === undefined)
      .map((resourceGroup) => resourceGroup.id);

    if (rgIdsToLoad.length === 0) {
      return allResources;
    }

    const loadedResources = await Promise.all(rgIdsToLoad.map((rgId) => this.loadResourceGroupResources(rgId)));
    for (const { rgId, resources } of loadedResources) {
      allResources[rgId] = resources;
    }

    this.rgResources.set(allResources);
    return allResources;
  }

  private async loadResourceGroupResources(rgId: string): Promise<{ rgId: string; resources: AzureResourceResponse[] }> {
    try {
      const resources = await this.resourceGroupService.getResources(rgId);
      return { rgId, resources };
    } catch {
      return { rgId, resources: [] };
    }
  }

  private collectMissingEnvResources(allResources: ResourceGroupResourcesById): MissingEnvResource[] {
    const missingEnvResources: MissingEnvResource[] = [];

    for (const resources of Object.values(allResources)) {
      if (!resources) {
        continue;
      }

      for (const resource of resources) {
        const missingEnvironments = this.getMissingEnvironments(resource);
        if (missingEnvironments.length === 0) {
          continue;
        }

        missingEnvResources.push({
          resourceId: resource.id,
          resourceName: resource.name,
          resourceType: resource.resourceType,
          missingEnvironments,
        });
      }
    }

    return missingEnvResources;
  }

  private buildGenerationDiagnosticsDialogData(
    configId: string,
    configName: string,
    diagnostics: ResourceDiagnosticResponse[],
    missingEnvResources: MissingEnvResource[],
    pendingCustomDomains: PendingCustomDomainIssue[],
  ): GenerationDiagnosticsDialogData {
    return {
      configDiagnostics: diagnostics.length > 0
        ? [{
          configId,
          configName,
          diagnostics,
        }]
        : [],
      missingEnvConfigs: missingEnvResources.length > 0
        ? [{
          configId,
          configName,
          resources: missingEnvResources,
        }]
        : undefined,
      pendingCustomDomainConfigs: pendingCustomDomains.length > 0
        ? [{
          configId,
          configName,
          domains: pendingCustomDomains,
        }]
        : undefined,
    };
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

  protected async loadCrossConfigReferences(): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.crossConfigLoading()) return;

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
}
