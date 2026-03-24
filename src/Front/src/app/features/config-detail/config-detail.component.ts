import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import {
  InfrastructureConfigResponse,
  ResourceNamingTemplateResponse,
  EnvironmentDefinitionResponse,
} from '../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse, AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddEnvironmentDialogComponent, AddEnvironmentDialogData } from './add-environment-dialog/add-environment-dialog.component';
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
import { ProjectService } from '../../shared/services/project.service';
import { BicepGeneratorService } from '../../shared/services/bicep-generator.service';
import { CascadeDeleteDialogComponent, CascadeDeleteDialogData } from '../../shared/components/cascade-delete-dialog/cascade-delete-dialog.component';
import { DependentResourceResponse } from '../../shared/interfaces/dependent-resource.interface';
import { GenerateBicepResponse } from '../../shared/interfaces/bicep-generator.interface';
import { saveAs } from 'file-saver';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { RESOURCE_TYPE_ABBREVIATIONS, RESOURCE_TYPE_ICONS, RESOURCE_TYPE_OPTIONS, PARENT_CHILD_RESOURCE_TYPES, CHILD_RESOURCE_TYPES } from './enums/resource-type.enum';
import { FormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { BicepHighlightPipe } from './pipes/bicep-highlight.pipe';

interface ResourceDisplayItem {
  resource: AzureResourceResponse;
  children?: AzureResourceResponse[];
  isParent: boolean;
}


@Component({
  selector: 'app-config-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTooltipModule,
    BicepHighlightPipe,
  ],
  templateUrl: './config-detail.component.html',
  styleUrl: './config-detail.component.scss',
})
export class ConfigDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
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
  private readonly projectService = inject(ProjectService);
  private readonly bicepService = inject(BicepGeneratorService);
  private readonly authService = inject(AuthenticationService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);

  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly resourceGroups = signal<ResourceGroupResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly envActionId = signal<string | null>(null);
  protected readonly envErrorKey = signal('');
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

  // ─── Bicep Generation ───
  protected readonly bicepLoading = signal(false);
  protected readonly bicepResult = signal<GenerateBicepResponse | null>(null);
  protected readonly bicepErrorKey = signal('');
  protected readonly bicepPanelOpen = signal(false);
  protected readonly bicepPanelCollapsed = signal(false);
  protected readonly bicepDownloading = signal(false);

  // ─── Bicep File Viewer ───
  protected readonly bicepViewerFile = signal<string | null>(null);
  protected readonly bicepViewerContent = signal<string | null>(null);
  protected readonly bicepViewerLoading = signal(false);
  protected readonly bicepExpandedFolders = signal<Set<string>>(new Set<string>(['modules']));

  protected readonly bicepModuleEntries = computed(() => {
    const result = this.bicepResult();
    if (!result?.moduleUris) return [];
    return Object.entries(result.moduleUris).map(([name, uri]) => ({ name, uri }));
  });

  protected readonly bicepParamEntries = computed(() => {
    const result = this.bicepResult();
    if (!result?.parameterFileUris) return [];
    return Object.entries(result.parameterFileUris).map(([name, uri]) => ({ name, uri }));
  });

  // ─── Inheritance ───
  protected readonly inheritanceLoading = signal(false);

  protected readonly useProjectEnvironments = computed(() => this.config()?.useProjectEnvironments ?? false);
  protected readonly useProjectNamingConventions = computed(() => this.config()?.useProjectNamingConventions ?? false);

  protected readonly projectSortedEnvironments = computed(() => {
    const envs = this.project()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
  });

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
    if (this.useProjectEnvironments()) {
      return this.project()?.environmentDefinitions ?? [];
    }
    return this.config()?.environmentDefinitions ?? [];
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
      this.recentlyViewedService.trackView({
        id: config.id,
        name: config.name,
        type: 'config',
      });

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
      const effectiveEnvs = config.useProjectEnvironments
        ? (this.project()?.environmentDefinitions ?? [])
        : (config.environmentDefinitions ?? []);
      const firstEnv = [...effectiveEnvs].sort((a, b) => a.order - b.order)[0];
      if (firstEnv) {
        this.previewEnvId.set(firstEnv.id);
      }
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
      // Auto-expand all parent resources by default
      const parentIds = resources
        .filter((r) => PARENT_CHILD_RESOURCE_TYPES[r.resourceType])
        .map((r) => r.id);
      if (parentIds.length > 0) {
        this.expandedParentResources.update((prev) => {
          const next = new Set(prev);
          parentIds.forEach((id) => next.add(id));
          return next;
        });
      }
    } catch {
      this.rgResources.update((prev) => ({ ...prev, [rgId]: [] }));
    } finally {
      this.rgResourcesLoading.set(null);
    }
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

    // Index parents
    for (const res of resources) {
      if (PARENT_CHILD_RESOURCE_TYPES[res.resourceType]) {
        parentMap.set(res.id, res);
        if (!childrenByParent.has(res.id)) {
          childrenByParent.set(res.id, []);
        }
      }
    }

    // Assign children to their parents; fallback to standalone
    for (const res of resources) {
      if (parentMap.has(res.id)) continue; // skip parents themselves
      if (CHILD_RESOURCE_TYPES.has(res.resourceType) && res.parentResourceId && parentMap.has(res.parentResourceId)) {
        childrenByParent.get(res.parentResourceId)!.push(res);
      } else {
        standalone.push(res);
      }
    }

    const result: ResourceDisplayItem[] = [];

    // Emit parents with their children
    for (const [parentId, parent] of parentMap) {
      result.push({
        resource: parent,
        children: childrenByParent.get(parentId) ?? [],
        isParent: true,
      });
    }

    // Emit standalone resources
    for (const res of standalone) {
      result.push({ resource: res, isParent: false });
    }

    return result;
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

  protected openAddResourceDialog(rgId: string): void {
    const rg = this.resourceGroups().find((r) => r.id === rgId);
    const envs = this.useProjectEnvironments()
      ? this.projectSortedEnvironments()
      : this.sortedEnvironments();
    const dialogRef = this.dialog.open(AddResourceDialogComponent, {
      data: {
        resourceGroupId: rgId,
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
        await this.loadRgResources(rgId);
      }
    });
  }

  protected openAddChildResourceDialog(parentResource: AzureResourceResponse, rgId: string): void {
    const rg = this.resourceGroups().find((r) => r.id === rgId);
    const envs = this.useProjectEnvironments()
      ? this.projectSortedEnvironments()
      : this.sortedEnvironments();
    const dialogRef = this.dialog.open(AddResourceDialogComponent, {
      data: {
        resourceGroupId: rgId,
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
        await this.loadRgResources(rgId);
      }
    });
  }

  protected openAddEnvironmentDialog(existing?: EnvironmentDefinitionResponse): void {
    const currentConfig = this.config();
    if (!currentConfig) return;

    const dialogRef = this.dialog.open(AddEnvironmentDialogComponent, {
      data: {
        configId: currentConfig.id,
        existing,
        allEnvironments: currentConfig.environmentDefinitions,
      } satisfies AddEnvironmentDialogData,
      width: '520px',
    });

    dialogRef.afterClosed().subscribe(async (result: InfrastructureConfigResponse | null) => {
      if (result) {
        const refreshed = await this.infraConfigService.getById(currentConfig.id);
        this.config.set(refreshed);
      }
    });
  }

  protected openRemoveEnvironmentDialog(env: EnvironmentDefinitionResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_TITLE',
        messageKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_MESSAGE',
        messageParams: { name: env.name },
        confirmKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (!confirmed) return;
      await this.removeEnvironment(env);
    });
  }

  private async removeEnvironment(env: EnvironmentDefinitionResponse): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.envActionId.set(env.id);
    this.envErrorKey.set('');

    try {
      await this.infraConfigService.removeEnvironment(configId, env.id);
      const refreshed = await this.infraConfigService.getById(configId);
      this.config.set(refreshed);
    } catch {
      this.envErrorKey.set('CONFIG_DETAIL.ENVIRONMENTS.REMOVE_ERROR');
    } finally {
      this.envActionId.set(null);
    }
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

  protected async toggleInheritanceEnvironments(useProject: boolean): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.inheritanceLoading.set(true);
    try {
      await this.infraConfigService.setInheritance(configId, {
        useProjectEnvironments: useProject,
        useProjectNamingConventions: this.useProjectNamingConventions(),
      });
      await this.refreshConfig(configId);
    } finally {
      this.inheritanceLoading.set(false);
    }
  }

  protected async toggleInheritanceNaming(useProject: boolean): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.inheritanceLoading.set(true);
    try {
      await this.infraConfigService.setInheritance(configId, {
        useProjectEnvironments: this.useProjectEnvironments(),
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
    this.bicepViewerFile.set(null);
    this.bicepViewerContent.set(null);
  }

  protected async openBicepFile(filePath: string): Promise<void> {
    const configId = this.config()?.id;
    if (!configId || this.bicepViewerLoading()) return;

    // Toggle off if already viewing this file
    if (this.bicepViewerFile() === filePath) {
      this.bicepViewerFile.set(null);
      this.bicepViewerContent.set(null);
      return;
    }

    this.bicepViewerFile.set(filePath);
    this.bicepViewerContent.set(null);
    this.bicepViewerLoading.set(true);
    try {
      const content = await this.bicepService.getFileContent(configId, filePath);
      this.bicepViewerContent.set(content);
    } catch {
      this.bicepViewerContent.set(null);
    } finally {
      this.bicepViewerLoading.set(false);
    }
  }

  protected closeBicepViewer(): void {
    this.bicepViewerFile.set(null);
    this.bicepViewerContent.set(null);
  }

  protected toggleBicepFolder(folder: string): void {
    this.bicepExpandedFolders.update(set => {
      const next = new Set(set);
      if (next.has(folder)) {
        next.delete(folder);
      } else {
        next.add(folder);
      }
      return next;
    });
  }

  protected isBicepFolderExpanded(folder: string): boolean {
    return this.bicepExpandedFolders().has(folder);
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
}
