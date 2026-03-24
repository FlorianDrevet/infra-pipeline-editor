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
import { FunctionAppResponse, FunctionAppEnvironmentConfigEntry } from '../../shared/interfaces/function-app.interface';
import { AppConfigurationResponse, AppConfigurationEnvironmentConfigEntry } from '../../shared/interfaces/app-configuration.interface';
import { ContainerAppEnvironmentResponse, ContainerAppEnvironmentEnvironmentConfigEntry } from '../../shared/interfaces/container-app-environment.interface';
import { ContainerAppResponse, ContainerAppEnvironmentConfigEntry } from '../../shared/interfaces/container-app.interface';
import { LogAnalyticsWorkspaceResponse, LogAnalyticsWorkspaceEnvironmentConfigEntry } from '../../shared/interfaces/log-analytics-workspace.interface';
import { ApplicationInsightsResponse, ApplicationInsightsEnvironmentConfigEntry } from '../../shared/interfaces/application-insights.interface';
import { CosmosDbResponse, CosmosDbEnvironmentConfigEntry } from '../../shared/interfaces/cosmos-db.interface';
import { UserAssignedIdentityResponse } from '../../shared/interfaces/user-assigned-identity.interface';
import { AppServicePlanService } from '../../shared/services/app-service-plan.service';
import { WebAppService } from '../../shared/services/web-app.service';
import { FunctionAppService } from '../../shared/services/function-app.service';
import { AppConfigurationService } from '../../shared/services/app-configuration.service';
import { ContainerAppEnvironmentService } from '../../shared/services/container-app-environment.service';
import { ContainerAppService } from '../../shared/services/container-app.service';
import { LogAnalyticsWorkspaceService } from '../../shared/services/log-analytics-workspace.service';
import { ApplicationInsightsService } from '../../shared/services/application-insights.service';
import { CosmosDbService } from '../../shared/services/cosmos-db.service';
import { UserAssignedIdentityService } from '../../shared/services/user-assigned-identity.service';
import { InfrastructureConfigResponse, EnvironmentDefinitionResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { RoleAssignmentResponse, AzureRoleDefinitionResponse } from '../../shared/interfaces/role-assignment.interface';
import { AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { RESOURCE_TYPE_ICONS } from '../config-detail/enums/resource-type.enum';
import { LOCATION_OPTIONS } from '../../shared/enums/location.enum';
import { OS_TYPE_OPTIONS } from '../config-detail/enums/os-type.enum';
import { RUNTIME_STACK_OPTIONS } from '../config-detail/enums/runtime-stack.enum';
import { FUNCTION_APP_RUNTIME_STACK_OPTIONS } from '../config-detail/enums/function-app-runtime-stack.enum';
import { APP_SERVICE_PLAN_SKU_OPTIONS } from '../config-detail/enums/app-service-plan-sku.enum';
import { AddRoleAssignmentDialogComponent, AddRoleAssignmentDialogData } from './add-role-assignment-dialog/add-role-assignment-dialog.component';

/** Union type for any loaded resource */
type ResourceData = KeyVaultResponse | RedisCacheResponse | StorageAccountResponse | AppServicePlanResponse | WebAppResponse | FunctionAppResponse | UserAssignedIdentityResponse | AppConfigurationResponse | ContainerAppEnvironmentResponse | ContainerAppResponse | LogAnalyticsWorkspaceResponse | ApplicationInsightsResponse | CosmosDbResponse;

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

const APP_CONFIGURATION_SKU_OPTIONS = [
  { label: 'Free', value: 'Free' },
  { label: 'Standard', value: 'Standard' },
];

const APP_CONFIGURATION_PUBLIC_NETWORK_OPTIONS = [
  { label: 'Enabled', value: 'Enabled' },
  { label: 'Disabled', value: 'Disabled' },
];

const CAE_SKU_OPTIONS = [
  { label: 'Consumption', value: 'Consumption' },
  { label: 'Premium', value: 'Premium' },
];

const CAE_WORKLOAD_PROFILE_OPTIONS = [
  { label: 'Consumption', value: 'Consumption' },
  { label: 'D4', value: 'D4' },
  { label: 'D8', value: 'D8' },
  { label: 'D16', value: 'D16' },
  { label: 'D32', value: 'D32' },
  { label: 'E4', value: 'E4' },
  { label: 'E8', value: 'E8' },
  { label: 'E16', value: 'E16' },
  { label: 'E32', value: 'E32' },
];

const CA_CPU_OPTIONS = [
  { label: '0.25', value: '0.25' },
  { label: '0.5', value: '0.5' },
  { label: '1.0', value: '1.0' },
  { label: '2.0', value: '2.0' },
  { label: '4.0', value: '4.0' },
];

const CA_MEMORY_OPTIONS = [
  { label: '0.5 Gi', value: '0.5Gi' },
  { label: '1.0 Gi', value: '1.0Gi' },
  { label: '2.0 Gi', value: '2.0Gi' },
  { label: '4.0 Gi', value: '4.0Gi' },
  { label: '8.0 Gi', value: '8.0Gi' },
];

const CA_TRANSPORT_OPTIONS = [
  { label: 'Auto', value: 'auto' },
  { label: 'HTTP', value: 'http' },
  { label: 'HTTP/2', value: 'http2' },
  { label: 'TCP', value: 'tcp' },
];

const LAW_SKU_OPTIONS = [
  { label: 'Free', value: 'Free' },
  { label: 'PerGB2018', value: 'PerGB2018' },
  { label: 'PerNode', value: 'PerNode' },
  { label: 'Premium', value: 'Premium' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Standalone', value: 'Standalone' },
  { label: 'Capacity Reservation', value: 'CapacityReservation' },
];

const AI_RETENTION_OPTIONS = [
  { label: '30 days', value: 30 },
  { label: '60 days', value: 60 },
  { label: '90 days', value: 90 },
  { label: '120 days', value: 120 },
  { label: '180 days', value: 180 },
  { label: '270 days', value: 270 },
  { label: '365 days', value: 365 },
  { label: '550 days', value: 550 },
  { label: '730 days', value: 730 },
];

const AI_INGESTION_MODE_OPTIONS = [
  { label: 'Application Insights', value: 'ApplicationInsights' },
  { label: 'Log Analytics', value: 'LogAnalytics' },
  { label: 'App Insights + Diagnostic Settings', value: 'ApplicationInsightsWithDiagnosticSettings' },
];

const COSMOS_API_TYPE_OPTIONS = [
  { label: 'SQL (NoSQL)', value: 'SQL' },
  { label: 'MongoDB', value: 'MongoDB' },
  { label: 'Cassandra', value: 'Cassandra' },
  { label: 'Table', value: 'Table' },
  { label: 'Gremlin', value: 'Gremlin' },
];

const COSMOS_CONSISTENCY_LEVEL_OPTIONS = [
  { label: 'Eventual', value: 'Eventual' },
  { label: 'Consistent Prefix', value: 'ConsistentPrefix' },
  { label: 'Session', value: 'Session' },
  { label: 'Bounded Staleness', value: 'BoundedStaleness' },
  { label: 'Strong', value: 'Strong' },
];

const COSMOS_BACKUP_POLICY_OPTIONS = [
  { label: 'Periodic', value: 'Periodic' },
  { label: 'Continuous', value: 'Continuous' },
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
  private readonly functionAppService = inject(FunctionAppService);
  private readonly appConfigurationService = inject(AppConfigurationService);
  private readonly containerAppEnvironmentService = inject(ContainerAppEnvironmentService);
  private readonly containerAppService = inject(ContainerAppService);
  private readonly logAnalyticsWorkspaceService = inject(LogAnalyticsWorkspaceService);
  private readonly applicationInsightsService = inject(ApplicationInsightsService);
  private readonly cosmosDbService = inject(CosmosDbService);
  private readonly userAssignedIdentityService = inject(UserAssignedIdentityService);
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
  protected readonly functionAppRuntimeStackOptions = FUNCTION_APP_RUNTIME_STACK_OPTIONS;
  protected readonly aspSkuOptions = APP_SERVICE_PLAN_SKU_OPTIONS;
  protected readonly appConfigurationSkuOptions = APP_CONFIGURATION_SKU_OPTIONS;
  protected readonly appConfigurationPublicNetworkOptions = APP_CONFIGURATION_PUBLIC_NETWORK_OPTIONS;
  protected readonly caeSkuOptions = CAE_SKU_OPTIONS;
  protected readonly caeWorkloadProfileOptions = CAE_WORKLOAD_PROFILE_OPTIONS;
  protected readonly caCpuOptions = CA_CPU_OPTIONS;
  protected readonly caMemoryOptions = CA_MEMORY_OPTIONS;
  protected readonly caTransportOptions = CA_TRANSPORT_OPTIONS;
  protected readonly lawSkuOptions = LAW_SKU_OPTIONS;
  protected readonly aiRetentionOptions = AI_RETENTION_OPTIONS;
  protected readonly aiIngestionModeOptions = AI_INGESTION_MODE_OPTIONS;
  protected readonly cosmosApiTypeOptions = COSMOS_API_TYPE_OPTIONS;
  protected readonly cosmosConsistencyLevelOptions = COSMOS_CONSISTENCY_LEVEL_OPTIONS;
  protected readonly cosmosBackupPolicyOptions = COSMOS_BACKUP_POLICY_OPTIONS;

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
      case 'FunctionApp':
        return this.functionAppService.getById(this.resourceId);
      case 'UserAssignedIdentity':
        return this.userAssignedIdentityService.getById(this.resourceId);
      case 'AppConfiguration':
        return this.appConfigurationService.getById(this.resourceId);
      case 'ContainerAppEnvironment':
        return this.containerAppEnvironmentService.getById(this.resourceId);
      case 'ContainerApp':
        return this.containerAppService.getById(this.resourceId);
      case 'LogAnalyticsWorkspace':
        return this.logAnalyticsWorkspaceService.getById(this.resourceId);
      case 'ApplicationInsights':
        return this.applicationInsightsService.getById(this.resourceId);
      case 'CosmosDb':
        return this.cosmosDbService.getById(this.resourceId);
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
    } else if (this.resourceType === 'FunctionApp') {
      const fa = resource as FunctionAppResponse;
      base['appServicePlanId'] = [fa.appServicePlanId];
      base['runtimeStack'] = [fa.runtimeStack, [Validators.required]];
      base['runtimeVersion'] = [fa.runtimeVersion];
      base['httpsOnly'] = [fa.httpsOnly];
    } else if (this.resourceType === 'ContainerApp') {
      const ca = resource as ContainerAppResponse;
      base['containerAppEnvironmentId'] = [ca.containerAppEnvironmentId];
    } else if (this.resourceType === 'ApplicationInsights') {
      const ai = resource as ApplicationInsightsResponse;
      base['logAnalyticsWorkspaceId'] = [ai.logAnalyticsWorkspaceId];
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
      case 'FunctionApp': {
        const fa = resource as FunctionAppResponse;
        const settings = fa.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          httpsOnly: [settings?.httpsOnly ?? null],
          runtimeStack: [settings?.runtimeStack ?? null],
          runtimeVersion: [settings?.runtimeVersion ?? null],
          maxInstanceCount: [settings?.maxInstanceCount ?? null],
          functionsWorkerRuntime: [settings?.functionsWorkerRuntime ?? null],
        });
      }
      default:
        return this.fb.group({});
      case 'AppConfiguration': {
        const ac = resource as AppConfigurationResponse;
        const settings = ac.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          softDeleteRetentionInDays: [settings?.softDeleteRetentionInDays ?? null],
          purgeProtectionEnabled: [settings?.purgeProtectionEnabled ?? null],
          disableLocalAuth: [settings?.disableLocalAuth ?? null],
          publicNetworkAccess: [settings?.publicNetworkAccess ?? null],
        });
      }
      case 'ContainerAppEnvironment': {
        const cae = resource as ContainerAppEnvironmentResponse;
        const settings = cae.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          workloadProfileType: [settings?.workloadProfileType ?? null],
          internalLoadBalancerEnabled: [settings?.internalLoadBalancerEnabled ?? null],
          zoneRedundancyEnabled: [settings?.zoneRedundancyEnabled ?? null],
          logAnalyticsWorkspaceId: [settings?.logAnalyticsWorkspaceId ?? null],
        });
      }
      case 'ContainerApp': {
        const ca = resource as ContainerAppResponse;
        const settings = ca.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          containerImage: [settings?.containerImage ?? null],
          cpuCores: [settings?.cpuCores ?? null],
          memoryGi: [settings?.memoryGi ?? null],
          minReplicas: [settings?.minReplicas ?? null],
          maxReplicas: [settings?.maxReplicas ?? null],
          ingressEnabled: [settings?.ingressEnabled ?? null],
          ingressTargetPort: [settings?.ingressTargetPort ?? null],
          ingressExternal: [settings?.ingressExternal ?? null],
          transportMethod: [settings?.transportMethod ?? null],
        });
      }
      case 'LogAnalyticsWorkspace': {
        const law = resource as LogAnalyticsWorkspaceResponse;
        const settings = law.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          retentionInDays: [settings?.retentionInDays ?? null],
          dailyQuotaGb: [settings?.dailyQuotaGb ?? null],
        });
      }
      case 'ApplicationInsights': {
        const ai = resource as ApplicationInsightsResponse;
        const settings = ai.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          samplingPercentage: [settings?.samplingPercentage ?? null],
          retentionInDays: [settings?.retentionInDays ?? null],
          disableIpMasking: [settings?.disableIpMasking ?? null],
          disableLocalAuth: [settings?.disableLocalAuth ?? null],
          ingestionMode: [settings?.ingestionMode ?? null],
        });
      }
      case 'CosmosDb': {
        const cosmosDb = resource as CosmosDbResponse;
        const settings = cosmosDb.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          databaseApiType: [settings?.databaseApiType ?? null],
          consistencyLevel: [settings?.consistencyLevel ?? null],
          maxStalenessPrefix: [settings?.maxStalenessPrefix ?? null],
          maxIntervalInSeconds: [settings?.maxIntervalInSeconds ?? null],
          enableAutomaticFailover: [settings?.enableAutomaticFailover ?? null],
          enableMultipleWriteLocations: [settings?.enableMultipleWriteLocations ?? null],
          backupPolicyType: [settings?.backupPolicyType ?? null],
          enableFreeTier: [settings?.enableFreeTier ?? null],
        });
      }
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
        case 'FunctionApp':
          await this.functionAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            appServicePlanId: general.appServicePlanId,
            runtimeStack: general.runtimeStack,
            runtimeVersion: general.runtimeVersion,
            httpsOnly: general.httpsOnly,
            environmentSettings: this.buildFunctionAppEnvSettings(),
          });
          break;
        case 'UserAssignedIdentity':
          await this.userAssignedIdentityService.update(this.resourceId, {
            name: general.name,
            location: general.location,
          });
          break;
        case 'AppConfiguration':
          await this.appConfigurationService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildAppConfigurationEnvSettings(),
          });
          break;
        case 'ContainerAppEnvironment':
          await this.containerAppEnvironmentService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildContainerAppEnvironmentEnvSettings(),
          });
          break;
        case 'ContainerApp':
          await this.containerAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            containerAppEnvironmentId: general.containerAppEnvironmentId,
            environmentSettings: this.buildContainerAppEnvSettings(),
          });
          break;
        case 'LogAnalyticsWorkspace':
          await this.logAnalyticsWorkspaceService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildLogAnalyticsWorkspaceEnvSettings(),
          });
          break;
        case 'ApplicationInsights':
          await this.applicationInsightsService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            logAnalyticsWorkspaceId: general.logAnalyticsWorkspaceId,
            environmentSettings: this.buildApplicationInsightsEnvSettings(),
          });
          break;
        case 'CosmosDb':
          await this.cosmosDbService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildCosmosDbEnvSettings(),
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
        case 'FunctionApp':
          await this.functionAppService.delete(this.resourceId);
          break;
        case 'UserAssignedIdentity':
          await this.userAssignedIdentityService.delete(this.resourceId);
          break;
        case 'AppConfiguration':
          await this.appConfigurationService.delete(this.resourceId);
          break;
        case 'ContainerAppEnvironment':
          await this.containerAppEnvironmentService.delete(this.resourceId);
          break;
        case 'ContainerApp':
          await this.containerAppService.delete(this.resourceId);
          break;
        case 'LogAnalyticsWorkspace':
          await this.logAnalyticsWorkspaceService.delete(this.resourceId);
          break;
        case 'ApplicationInsights':
          await this.applicationInsightsService.delete(this.resourceId);
          break;
        case 'CosmosDb':
          await this.cosmosDbService.delete(this.resourceId);
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
        resourceGroupId: (this.resource() as { resourceGroupId?: string })?.resourceGroupId ?? '',
        configLocation: this.resource()?.location ?? 'EastUS2',
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

  private buildFunctionAppEnvSettings(): FunctionAppEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        httpsOnly: raw.httpsOnly ?? null,
        runtimeStack: raw.runtimeStack || null,
        runtimeVersion: raw.runtimeVersion || null,
        maxInstanceCount: raw.maxInstanceCount != null ? Number(raw.maxInstanceCount) : null,
        functionsWorkerRuntime: raw.functionsWorkerRuntime || null,
      };
    });
  }

  private buildAppConfigurationEnvSettings(): AppConfigurationEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        softDeleteRetentionInDays: raw.softDeleteRetentionInDays != null ? Number(raw.softDeleteRetentionInDays) : null,
        purgeProtectionEnabled: raw.purgeProtectionEnabled ?? null,
        disableLocalAuth: raw.disableLocalAuth ?? null,
        publicNetworkAccess: raw.publicNetworkAccess || null,
      };
    });
  }

  private buildContainerAppEnvironmentEnvSettings(): ContainerAppEnvironmentEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        workloadProfileType: raw.workloadProfileType || null,
        internalLoadBalancerEnabled: raw.internalLoadBalancerEnabled ?? null,
        zoneRedundancyEnabled: raw.zoneRedundancyEnabled ?? null,
        logAnalyticsWorkspaceId: raw.logAnalyticsWorkspaceId || null,
      };
    });
  }

  private buildContainerAppEnvSettings(): ContainerAppEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        containerImage: raw.containerImage || null,
        cpuCores: raw.cpuCores || null,
        memoryGi: raw.memoryGi || null,
        minReplicas: raw.minReplicas != null ? Number(raw.minReplicas) : null,
        maxReplicas: raw.maxReplicas != null ? Number(raw.maxReplicas) : null,
        ingressEnabled: raw.ingressEnabled ?? null,
        ingressTargetPort: raw.ingressTargetPort != null ? Number(raw.ingressTargetPort) : null,
        ingressExternal: raw.ingressExternal ?? null,
        transportMethod: raw.transportMethod || null,
      };
    });
  }

  private buildLogAnalyticsWorkspaceEnvSettings(): LogAnalyticsWorkspaceEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        retentionInDays: raw.retentionInDays != null ? Number(raw.retentionInDays) : null,
        dailyQuotaGb: raw.dailyQuotaGb != null ? Number(raw.dailyQuotaGb) : null,
      };
    });
  }

  private buildApplicationInsightsEnvSettings(): ApplicationInsightsEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        samplingPercentage: raw.samplingPercentage != null ? Number(raw.samplingPercentage) : null,
        retentionInDays: raw.retentionInDays != null ? Number(raw.retentionInDays) : null,
        disableIpMasking: raw.disableIpMasking ?? null,
        disableLocalAuth: raw.disableLocalAuth ?? null,
        ingestionMode: raw.ingestionMode || null,
      };
    });
  }

  private buildCosmosDbEnvSettings(): CosmosDbEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        databaseApiType: raw.databaseApiType || null,
        consistencyLevel: raw.consistencyLevel || null,
        maxStalenessPrefix: raw.maxStalenessPrefix != null ? Number(raw.maxStalenessPrefix) : null,
        maxIntervalInSeconds: raw.maxIntervalInSeconds != null ? Number(raw.maxIntervalInSeconds) : null,
        enableAutomaticFailover: raw.enableAutomaticFailover ?? null,
        enableMultipleWriteLocations: raw.enableMultipleWriteLocations ?? null,
        backupPolicyType: raw.backupPolicyType || null,
        enableFreeTier: raw.enableFreeTier ?? null,
      };
    });
  }
}
