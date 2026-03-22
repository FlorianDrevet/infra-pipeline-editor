import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
import { RESOURCE_TYPE_ABBREVIATIONS, RESOURCE_TYPE_ICONS, RESOURCE_TYPE_OPTIONS } from './enums/resource-type.enum';
import { FormsModule } from '@angular/forms';



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
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './config-detail.component.html',
  styleUrl: './config-detail.component.scss',
})
export class ConfigDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly dialog = inject(MatDialog);

  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
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

  // canWrite defaults to true — access checks are now at project level
  protected readonly canWrite = signal(true);

  protected readonly canAddResourceNamingTemplate = computed(() => {
    const configuredTypes = new Set((this.config()?.resourceNamingTemplates ?? []).map((item) => item.resourceType));
    return this.resourceTypeOptions.some((option) => !configuredTypes.has(option.value));
  });

  protected readonly sortedEnvironments = computed(() => {
    const envs = this.config()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
  });

  protected readonly previewEnvId = signal<string | null>(null);

  protected readonly previewEnv = computed(() => {
    const id = this.previewEnvId();
    if (!id) return null;
    return this.config()?.environmentDefinitions.find((e) => e.id === id) ?? null;
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

    // Pick the resource-specific template override, or fall back to the default
    const resourceOverride = cfg.resourceNamingTemplates.find((t) => t.resourceType === resourceType);
    const template = resourceOverride?.template ?? cfg.defaultNamingTemplate;
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

      // Pre-select the first environment (by order) for the naming preview
      const firstEnv = [...(config.environmentDefinitions ?? [])].sort((a, b) => a.order - b.order)[0];
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
    } catch {
      this.rgResources.update((prev) => ({ ...prev, [rgId]: [] }));
    } finally {
      this.rgResourcesLoading.set(null);
    }
  }

  protected openAddResourceDialog(rgId: string): void {
    const rg = this.resourceGroups().find((r) => r.id === rgId);
    const dialogRef = this.dialog.open(AddResourceDialogComponent, {
      data: { resourceGroupId: rgId, location: rg?.location ?? '' } satisfies AddResourceDialogData,
      width: '560px',
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

  private async refreshConfig(configId: string): Promise<void> {
    const refreshed = await this.infraConfigService.getById(configId);
    this.config.set(refreshed);
  }
}
