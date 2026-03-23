import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { KeyVaultService } from '../../shared/services/key-vault.service';
import { RedisCacheService } from '../../shared/services/redis-cache.service';
import { StorageAccountService } from '../../shared/services/storage-account.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RoleAssignmentService } from '../../shared/services/role-assignment.service';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { KeyVaultResponse, KeyVaultEnvironmentConfigEntry } from '../../shared/interfaces/key-vault.interface';
import { RedisCacheResponse, RedisCacheEnvironmentConfigEntry } from '../../shared/interfaces/redis-cache.interface';
import { StorageAccountResponse, StorageAccountEnvironmentConfigEntry } from '../../shared/interfaces/storage-account.interface';
import { AppServicePlanResponse, AppServicePlanEnvironmentConfigEntry } from '../../shared/interfaces/app-service-plan.interface';
import { WebAppResponse, WebAppEnvironmentConfigEntry } from '../../shared/interfaces/web-app.interface';
import { AppServicePlanService } from '../../shared/services/app-service-plan.service';
import { WebAppService } from '../../shared/services/web-app.service';
import { InfrastructureConfigResponse, EnvironmentDefinitionResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { RoleAssignmentResponse, AzureRoleDefinitionResponse } from '../../shared/interfaces/role-assignment.interface';
import { AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { RESOURCE_TYPE_ICONS } from '../config-detail/enums/resource-type.enum';
import { LOCATION_OPTIONS } from '../../shared/enums/location.enum';
import { OS_TYPE_OPTIONS } from '../config-detail/enums/os-type.enum';
import { RUNTIME_STACK_OPTIONS } from '../config-detail/enums/runtime-stack.enum';
import { APP_SERVICE_PLAN_SKU_OPTIONS } from '../config-detail/enums/app-service-plan-sku.enum';
import { AddRoleAssignmentDialogComponent, AddRoleAssignmentDialogData } from './add-role-assignment-dialog/add-role-assignment-dialog.component';

/** Union type for any loaded resource */
type ResourceData = KeyVaultResponse | RedisCacheResponse | StorageAccountResponse | AppServicePlanResponse | WebAppResponse;

/** SKU options per resource type */
const KEY_VAULT_SKU_OPTIONS = [
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

const REDIS_SKU_OPTIONS = [
  { label: 'Basic', value: 'Basic' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

const REDIS_TLS_OPTIONS = [
  { label: 'TLS 1.0', value: 'Tls10' },
  { label: 'TLS 1.1', value: 'Tls11' },
  { label: 'TLS 1.2', value: 'Tls12' },
];

const REDIS_EVICTION_OPTIONS = [
  { label: 'NoEviction', value: 'NoEviction' },
  { label: 'AllKeysLru', value: 'AllKeysLru' },
  { label: 'VolatileLru', value: 'VolatileLru' },
  { label: 'AllKeysRandom', value: 'AllKeysRandom' },
  { label: 'VolatileRandom', value: 'VolatileRandom' },
  { label: 'VolatileTtl', value: 'VolatileTtl' },
  { label: 'AllKeysLfu', value: 'AllKeysLfu' },
  { label: 'VolatileLfu', value: 'VolatileLfu' },
];

const REDIS_VERSION_OPTIONS = [
  { label: 'Redis 4', value: 4 },
  { label: 'Redis 6', value: 6 },
];

const STORAGE_SKU_OPTIONS = [
  { label: 'Standard_LRS', value: 'Standard_LRS' },
  { label: 'Standard_GRS', value: 'Standard_GRS' },
  { label: 'Standard_RAGRS', value: 'Standard_RAGRS' },
  { label: 'Standard_ZRS', value: 'Standard_ZRS' },
  { label: 'Premium_LRS', value: 'Premium_LRS' },
  { label: 'Premium_ZRS', value: 'Premium_ZRS' },
];

const STORAGE_KIND_OPTIONS = [
  { label: 'StorageV2', value: 'StorageV2' },
  { label: 'BlobStorage', value: 'BlobStorage' },
  { label: 'BlockBlobStorage', value: 'BlockBlobStorage' },
];

const STORAGE_ACCESS_TIER_OPTIONS = [
  { label: 'Hot', value: 'Hot' },
  { label: 'Cool', value: 'Cool' },
];

const STORAGE_TLS_OPTIONS = [
  { label: 'TLS 1.0', value: 'Tls10' },
  { label: 'TLS 1.1', value: 'Tls11' },
  { label: 'TLS 1.2', value: 'Tls12' },
];

@Component({
  selector: 'app-resource-edit',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatOptionModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './resource-edit.component.html',
  styleUrl: './resource-edit.component.scss',
})
export class ResourceEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private readonly keyVaultService = inject(KeyVaultService);
  private readonly redisCacheService = inject(RedisCacheService);
  private readonly storageAccountService = inject(StorageAccountService);
  private readonly appServicePlanService = inject(AppServicePlanService);
  private readonly webAppService = inject(WebAppService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);
  private readonly roleAssignmentService = inject(RoleAssignmentService);
  private readonly resourceGroupService = inject(ResourceGroupService);

  // ─── Route params ───
  protected configId = '';
  protected resourceType = '';
  protected resourceId = '';

  // ─── State ───
  protected readonly resource = signal<ResourceData | null>(null);
  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly loadError = signal('');
  protected readonly saveError = signal('');
  protected readonly saveSuccess = signal(false);

  // ─── Role Assignments ───
  protected readonly roleAssignments = signal<RoleAssignmentResponse[]>([]);
  protected readonly roleAssignmentsLoading = signal(false);
  protected readonly roleAssignmentsError = signal('');
  protected readonly allResources = signal<AzureResourceResponse[]>([]);
  protected readonly availableRoleDefs = signal<AzureRoleDefinitionResponse[]>([]);

  // ─── Options ───
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;
  protected readonly locationOptions = LOCATION_OPTIONS;
  protected readonly keyVaultSkuOptions = KEY_VAULT_SKU_OPTIONS;
  protected readonly redisSkuOptions = REDIS_SKU_OPTIONS;
  protected readonly redisTlsOptions = REDIS_TLS_OPTIONS;
  protected readonly redisEvictionOptions = REDIS_EVICTION_OPTIONS;
  protected readonly redisVersionOptions = REDIS_VERSION_OPTIONS;
  protected readonly storageSkuOptions = STORAGE_SKU_OPTIONS;
  protected readonly storageKindOptions = STORAGE_KIND_OPTIONS;
  protected readonly storageAccessTierOptions = STORAGE_ACCESS_TIER_OPTIONS;
  protected readonly storageTlsOptions = STORAGE_TLS_OPTIONS;
  protected readonly osTypeOptions = OS_TYPE_OPTIONS;
  protected readonly runtimeStackOptions = RUNTIME_STACK_OPTIONS;
  protected readonly aspSkuOptions = APP_SERVICE_PLAN_SKU_OPTIONS;

  // ─── Forms ───
  protected generalForm!: FormGroup;
  protected envForms = signal<{ envName: string; form: FormGroup }[]>([]);

  // ─── Environments ───
  protected readonly environments = computed<EnvironmentDefinitionResponse[]>(() => {
    const cfg = this.config();
    const proj = this.project();
    if (!cfg) return [];
    if (cfg.useProjectEnvironments && proj) {
      return [...(proj.environmentDefinitions ?? [])].sort((a, b) => a.order - b.order);
    }
    return [...(cfg.environmentDefinitions ?? [])].sort((a, b) => a.order - b.order);
  });

  protected readonly selectedEnvIndex = signal(0);

  // ─── Permission ───
  protected readonly canWrite = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner' || me?.role === 'Contributor';
  });

  protected readonly isOwner = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner';
  });

  async ngOnInit(): Promise<void> {
    this.configId = this.route.snapshot.paramMap.get('configId') ?? '';
    this.resourceType = this.route.snapshot.paramMap.get('resourceType') ?? '';
    this.resourceId = this.route.snapshot.paramMap.get('resourceId') ?? '';

    if (!this.configId || !this.resourceType || !this.resourceId) {
      this.loadError.set('RESOURCE_EDIT.ERROR.MISSING_PARAMS');
      return;
    }

    await this.loadData();
  }

  private async loadData(): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      // Load config + project in parallel with resource
      const [config, resource] = await Promise.all([
        this.infraConfigService.getById(this.configId),
        this.loadResource(),
      ]);
      this.config.set(config);
      this.resource.set(resource);

      // Load parent project for permissions and environments
      if (config.projectId) {
        try {
          const project = await this.projectService.getProject(config.projectId);
          this.project.set(project);
        } catch {
          // Non-blocking — project data used for permissions and inherited envs
        }
      }

      this.buildGeneralForm(resource);
      this.buildEnvForms(resource);

      // Load role assignments and sibling resources in background (non-blocking)
      this.loadRoleAssignments();
      this.loadAllResources();
    } catch {
      this.loadError.set('RESOURCE_EDIT.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadResource(): Promise<ResourceData> {
    switch (this.resourceType) {
      case 'KeyVault':
        return this.keyVaultService.getById(this.resourceId);
      case 'RedisCache':
        return this.redisCacheService.getById(this.resourceId);
      case 'StorageAccount':
        return this.storageAccountService.getById(this.resourceId);
      case 'AppServicePlan':
        return this.appServicePlanService.getById(this.resourceId);
      case 'WebApp':
        return this.webAppService.getById(this.resourceId);
      default:
        throw new Error(`Unknown resource type: ${this.resourceType}`);
    }
  }

  private buildGeneralForm(resource: ResourceData): void {
    const base: Record<string, unknown[]> = {
      name: [resource.name, [Validators.required, Validators.maxLength(80)]],
      location: [resource.location, [Validators.required]],
    };

    if (this.resourceType === 'AppServicePlan') {
      const asp = resource as AppServicePlanResponse;
      base['osType'] = [asp.osType, [Validators.required]];
    } else if (this.resourceType === 'WebApp') {
      const wa = resource as WebAppResponse;
      base['appServicePlanId'] = [wa.appServicePlanId];
      base['runtimeStack'] = [wa.runtimeStack, [Validators.required]];
      base['runtimeVersion'] = [wa.runtimeVersion];
      base['alwaysOn'] = [wa.alwaysOn];
      base['httpsOnly'] = [wa.httpsOnly];
    }

    this.generalForm = this.fb.group(base);
  }

  private buildEnvForms(resource: ResourceData): void {
    const envs = this.environments();
    const forms: { envName: string; form: FormGroup }[] = [];

    for (const env of envs) {
      forms.push({
        envName: env.name,
        form: this.buildSingleEnvForm(resource, env.name),
      });
    }

    this.envForms.set(forms);
  }

  private buildSingleEnvForm(resource: ResourceData, envName: string): FormGroup {
    switch (this.resourceType) {
      case 'KeyVault': {
        const kv = resource as KeyVaultResponse;
        const settings = kv.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
        });
      }
      case 'RedisCache': {
        const rc = resource as RedisCacheResponse;
        const settings = rc.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          capacity: [settings?.capacity ?? null],
          redisVersion: [settings?.redisVersion ?? null],
          enableNonSslPort: [settings?.enableNonSslPort ?? null],
          minimumTlsVersion: [settings?.minimumTlsVersion ?? null],
          maxMemoryPolicy: [settings?.maxMemoryPolicy ?? null],
        });
      }
      case 'StorageAccount': {
        const sa = resource as StorageAccountResponse;
        const settings = sa.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          kind: [settings?.kind ?? null],
          accessTier: [settings?.accessTier ?? null],
          allowBlobPublicAccess: [settings?.allowBlobPublicAccess ?? null],
          enableHttpsTrafficOnly: [settings?.enableHttpsTrafficOnly ?? null],
          minimumTlsVersion: [settings?.minimumTlsVersion ?? null],
        });
      }
      case 'AppServicePlan': {
        const asp = resource as AppServicePlanResponse;
        const settings = asp.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          capacity: [settings?.capacity ?? null],
        });
      }
      case 'WebApp': {
        const wa = resource as WebAppResponse;
        const settings = wa.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          alwaysOn: [settings?.alwaysOn ?? null],
          httpsOnly: [settings?.httpsOnly ?? null],
          runtimeStack: [settings?.runtimeStack ?? null],
          runtimeVersion: [settings?.runtimeVersion ?? null],
        });
      }
      default:
        return this.fb.group({});
    }
  }

  protected async onSave(): Promise<void> {
    if (this.generalForm.invalid || this.isSaving()) return;

    this.isSaving.set(true);
    this.saveError.set('');
    this.saveSuccess.set(false);

    const general = this.generalForm.getRawValue();

    try {
      switch (this.resourceType) {
        case 'KeyVault':
          await this.keyVaultService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildKeyVaultEnvSettings(),
          });
          break;
        case 'RedisCache':
          await this.redisCacheService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildRedisCacheEnvSettings(),
          });
          break;
        case 'StorageAccount':
          await this.storageAccountService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildStorageAccountEnvSettings(),
          });
          break;
        case 'AppServicePlan':
          await this.appServicePlanService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            osType: general.osType,
            environmentSettings: this.buildAppServicePlanEnvSettings(),
          });
          break;
        case 'WebApp':
          await this.webAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            appServicePlanId: general.appServicePlanId,
            runtimeStack: general.runtimeStack,
            runtimeVersion: general.runtimeVersion,
            alwaysOn: general.alwaysOn,
            httpsOnly: general.httpsOnly,
            environmentSettings: this.buildWebAppEnvSettings(),
          });
          break;
      }

      // Reload to reflect saved state
      const updated = await this.loadResource();
      this.resource.set(updated);
      this.buildGeneralForm(updated);
      this.buildEnvForms(updated);
      this.saveSuccess.set(true);
      setTimeout(() => this.saveSuccess.set(false), 4000);
    } catch {
      this.saveError.set('RESOURCE_EDIT.ERROR.SAVE_FAILED');
    } finally {
      this.isSaving.set(false);
    }
  }

  protected openDeleteDialog(): void {
    const res = this.resource();
    if (!res) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.DELETE.CONFIRM_TITLE',
        messageKey: 'RESOURCE_EDIT.DELETE.CONFIRM_MESSAGE',
        messageParams: { name: res.name, type: this.resourceType },
        confirmKey: 'RESOURCE_EDIT.DELETE.CONFIRM_YES',
        cancelKey: 'RESOURCE_EDIT.DELETE.CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.deleteResource();
    });
  }

  private async deleteResource(): Promise<void> {
    this.isSaving.set(true);
    this.saveError.set('');

    try {
      switch (this.resourceType) {
        case 'KeyVault':
          await this.keyVaultService.delete(this.resourceId);
          break;
        case 'RedisCache':
          await this.redisCacheService.delete(this.resourceId);
          break;
        case 'StorageAccount':
          await this.storageAccountService.delete(this.resourceId);
          break;
        case 'AppServicePlan':
          await this.appServicePlanService.delete(this.resourceId);
          break;
        case 'WebApp':
          await this.webAppService.delete(this.resourceId);
          break;
      }
      this.router.navigate(['/config', this.configId]);
    } catch {
      this.saveError.set('RESOURCE_EDIT.ERROR.DELETE_FAILED');
      this.isSaving.set(false);
    }
  }

  // ─── Role Assignment methods ───

  private async loadRoleAssignments(): Promise<void> {
    this.roleAssignmentsLoading.set(true);
    this.roleAssignmentsError.set('');
    try {
      const assignments = await this.roleAssignmentService.getByResourceId(this.resourceId);
      this.roleAssignments.set(assignments);

      // Load role definitions for all unique target resource ids to resolve role names
      const targetIds = [...new Set(assignments.map(a => a.targetResourceId))];
      const allDefs: AzureRoleDefinitionResponse[] = [];
      for (const targetId of targetIds) {
        try {
          const defs = await this.roleAssignmentService.getAvailableRoleDefinitions(targetId);
          allDefs.push(...defs);
        } catch {
          // Non-blocking
        }
      }
      // Deduplicate by id
      const deduped = allDefs.filter((d, i, arr) => arr.findIndex(x => x.id === d.id) === i);
      this.availableRoleDefs.set(deduped);
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.LOAD_ERROR');
    } finally {
      this.roleAssignmentsLoading.set(false);
    }
  }

  private async loadAllResources(): Promise<void> {
    try {
      const rgs = await this.infraConfigService.getResourceGroups(this.configId);
      const resourcePromises = rgs.map(rg => this.resourceGroupService.getResources(rg.id));
      const results = await Promise.all(resourcePromises);
      const flat = results.flat().filter(r => r.id !== this.resourceId);
      this.allResources.set(flat);
    } catch {
      this.allResources.set([]);
    }
  }

  protected resolveTargetName(targetResourceId: string): string {
    const res = this.allResources().find(r => r.id === targetResourceId);
    return res?.name ?? targetResourceId;
  }

  protected resolveTargetType(targetResourceId: string): string {
    const res = this.allResources().find(r => r.id === targetResourceId);
    return res?.resourceType ?? '';
  }

  protected resolveRoleName(roleDefinitionId: string): string {
    const def = this.availableRoleDefs().find(d => d.id === roleDefinitionId);
    return def?.name ?? roleDefinitionId;
  }

  protected resolveRoleDocUrl(roleDefinitionId: string): string {
    const def = this.availableRoleDefs().find(d => d.id === roleDefinitionId);
    return def?.documentationUrl ?? '';
  }

  protected openAddRoleAssignmentDialog(): void {
    const dialogRef = this.dialog.open(AddRoleAssignmentDialogComponent, {
      data: {
        sourceResourceId: this.resourceId,
        currentResourceName: this.resource()?.name ?? '',
        siblingResources: this.allResources(),
      } satisfies AddRoleAssignmentDialogData,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe((result?: RoleAssignmentResponse) => {
      if (result) {
        this.roleAssignments.update(list => [...list, result]);
        // Reload role definitions to include new target's defs
        this.loadRoleAssignments();
      }
    });
  }

  protected openRemoveRoleAssignmentDialog(assignment: RoleAssignmentResponse): void {
    const roleName = this.resolveRoleName(assignment.roleDefinitionId);
    const targetName = this.resolveTargetName(assignment.targetResourceId);

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_TITLE',
        messageKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_MESSAGE',
        messageParams: { role: roleName, target: targetName },
        confirmKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_YES',
        cancelKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.removeRoleAssignment(assignment.id);
    });
  }

  private async removeRoleAssignment(roleAssignmentId: string): Promise<void> {
    this.roleAssignmentsError.set('');
    try {
      await this.roleAssignmentService.remove(this.resourceId, roleAssignmentId);
      this.roleAssignments.update(list => list.filter(ra => ra.id !== roleAssignmentId));
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_ERROR');
    }
  }

  // ─── Environment settings builders ───

  private buildKeyVaultEnvSettings(): KeyVaultEnvironmentConfigEntry[] {
    return this.envForms().map(ef => ({
      environmentName: ef.envName,
      sku: ef.form.getRawValue().sku || null,
    }));
  }

  private buildRedisCacheEnvSettings(): RedisCacheEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        capacity: raw.capacity != null ? Number(raw.capacity) : null,
        redisVersion: raw.redisVersion != null ? Number(raw.redisVersion) : null,
        enableNonSslPort: raw.enableNonSslPort ?? null,
        minimumTlsVersion: raw.minimumTlsVersion || null,
        maxMemoryPolicy: raw.maxMemoryPolicy || null,
      };
    });
  }

  private buildStorageAccountEnvSettings(): StorageAccountEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        kind: raw.kind || null,
        accessTier: raw.accessTier || null,
        allowBlobPublicAccess: raw.allowBlobPublicAccess ?? null,
        enableHttpsTrafficOnly: raw.enableHttpsTrafficOnly ?? null,
        minimumTlsVersion: raw.minimumTlsVersion || null,
      };
    });
  }

  private buildAppServicePlanEnvSettings(): AppServicePlanEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        capacity: raw.capacity != null ? Number(raw.capacity) : null,
      };
    });
  }

  private buildWebAppEnvSettings(): WebAppEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        alwaysOn: raw.alwaysOn ?? null,
        httpsOnly: raw.httpsOnly ?? null,
        runtimeStack: raw.runtimeStack || null,
        runtimeVersion: raw.runtimeVersion || null,
      };
    });
  }
}
