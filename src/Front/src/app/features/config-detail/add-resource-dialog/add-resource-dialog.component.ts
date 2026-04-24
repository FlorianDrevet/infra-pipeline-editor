import { Component, DestroyRef, inject, signal, computed } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, debounceTime, distinctUntilChanged, EMPTY, filter, Observable, switchMap, tap } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslateModule } from '@ngx-translate/core';
import { LOCATION_OPTIONS } from '../enums/location.enum';
import { RESOURCE_TYPE_OPTIONS, ResourceTypeEnum, RESOURCE_TYPE_ICONS, RESOURCE_TYPE_CATEGORIES, PARENT_CHILD_RESOURCE_TYPES, ResourceTypeCategory } from '../enums/resource-type.enum';
import { OS_TYPE_OPTIONS } from '../enums/os-type.enum';
import { APP_SERVICE_PLAN_SKU_OPTIONS } from '../enums/app-service-plan-sku.enum';
import { RUNTIME_STACK_OPTIONS } from '../enums/runtime-stack.enum';
import { FUNCTION_APP_RUNTIME_STACK_OPTIONS } from '../enums/function-app-runtime-stack.enum';
import { KeyVaultService } from '../../../shared/services/key-vault.service';
import { RedisCacheService } from '../../../shared/services/redis-cache.service';
import { StorageAccountService } from '../../../shared/services/storage-account.service';
import { AppServicePlanService } from '../../../shared/services/app-service-plan.service';
import { WebAppService } from '../../../shared/services/web-app.service';
import { UserAssignedIdentityService } from '../../../shared/services/user-assigned-identity.service';
import { FunctionAppService } from '../../../shared/services/function-app.service';
import { AppConfigurationService } from '../../../shared/services/app-configuration.service';
import { ContainerAppEnvironmentService } from '../../../shared/services/container-app-environment.service';
import { ContainerAppService } from '../../../shared/services/container-app.service';
import { LogAnalyticsWorkspaceService } from '../../../shared/services/log-analytics-workspace.service';
import { ApplicationInsightsService } from '../../../shared/services/application-insights.service';
import { CosmosDbService } from '../../../shared/services/cosmos-db.service';
import { SqlServerService } from '../../../shared/services/sql-server.service';
import { SqlDatabaseService } from '../../../shared/services/sql-database.service';
import { ServiceBusNamespaceService } from '../../../shared/services/service-bus-namespace.service';
import { ContainerRegistryService } from '../../../shared/services/container-registry.service';
import { ResourceGroupService } from '../../../shared/services/resource-group.service';
import { KeyVaultEnvironmentConfigEntry } from '../../../shared/interfaces/key-vault.interface';
import { RedisCacheEnvironmentConfigEntry } from '../../../shared/interfaces/redis-cache.interface';
import { StorageAccountEnvironmentConfigEntry } from '../../../shared/interfaces/storage-account.interface';
import { AppServicePlanEnvironmentConfigEntry } from '../../../shared/interfaces/app-service-plan.interface';
import { WebAppEnvironmentConfigEntry } from '../../../shared/interfaces/web-app.interface';
import { FunctionAppEnvironmentConfigEntry } from '../../../shared/interfaces/function-app.interface';
import { AppConfigurationEnvironmentConfigEntry } from '../../../shared/interfaces/app-configuration.interface';
import { ContainerAppEnvironmentEnvironmentConfigEntry } from '../../../shared/interfaces/container-app-environment.interface';
import { ContainerAppEnvironmentConfigEntry } from '../../../shared/interfaces/container-app.interface';
import { LogAnalyticsWorkspaceEnvironmentConfigEntry } from '../../../shared/interfaces/log-analytics-workspace.interface';
import { ApplicationInsightsEnvironmentConfigEntry } from '../../../shared/interfaces/application-insights.interface';
import { CosmosDbEnvironmentConfigEntry } from '../../../shared/interfaces/cosmos-db.interface';
import { SqlServerEnvironmentConfigEntry } from '../../../shared/interfaces/sql-server.interface';
import { SqlDatabaseEnvironmentConfigEntry } from '../../../shared/interfaces/sql-database.interface';
import { ServiceBusNamespaceEnvironmentConfigEntry } from '../../../shared/interfaces/service-bus-namespace.interface';
import { AcrAuthMode, ContainerRegistryEnvironmentConfigEntry } from '../../../shared/interfaces/container-registry.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { ProjectService } from '../../../shared/services/project.service';
import { ProjectResourceResponse } from '../../../shared/interfaces/cross-config-reference.interface';
import { NameAvailabilityService } from '../../../shared/services/name-availability.service';
import { EnvironmentNameAvailabilityResponseItem } from '../../../shared/interfaces/name-availability.interface';
import { ToggleSectionCardComponent } from '../../../shared/components/toggle-section-card/toggle-section-card.component';
import { DeploymentConfigComponent } from '../../../shared/components/deployment-config/deployment-config.component';
import { DsButtonComponent, DsTextFieldComponent, DsSelectComponent } from '../../../shared/components/ds';

export interface AddResourceDialogData {
  resourceGroupId: string;
  configId: string;
  projectId: string;
  location: string;
  environments: { name: string }[];
  parentResource?: { id: string; name: string; resourceType: string };
}

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

const CAE_SKU_OPTIONS = [
  { label: 'Consumption', value: 'Consumption' },
  { label: 'Premium', value: 'Premium' },
];

const ACR_SKU_OPTIONS = [
  { label: 'Basic', value: 'Basic' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

const ACR_PUBLIC_NETWORK_OPTIONS = [
  { label: 'Enabled', value: 'Enabled' },
  { label: 'Disabled', value: 'Disabled' },
];

const WEBAPP_RUNTIME_VERSION_MAP: Record<string, string[]> = {
  DotNet: ['10', '9', '8'],
  Node: ['22-lts', '20-lts'],
  Python: ['3.13', '3.12', '3.11', '3.10'],
  Java: ['21', '17', '11'],
  Php: ['8.4', '8.3', '8.2'],
};

const FUNCTIONAPP_RUNTIME_VERSION_MAP: Record<string, string[]> = {
  DotNet: ['10-isolated', '9-isolated', '8-isolated', '8-in-process'],
  Node: ['22', '20'],
  Python: ['3.12', '3.11', '3.10'],
  Java: ['21', '17', '11'],
  PowerShell: ['7.4', '7.2'],
};

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

type DialogStep = 'type' | 'plan-selection' | 'create-plan' | 'common' | 'environments';

@Component({
  selector: 'app-add-resource-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatButtonToggleModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    MatTabsModule,
    ReactiveFormsModule,
    TranslateModule,
    ToggleSectionCardComponent,
    DeploymentConfigComponent,
    DsButtonComponent,
    DsTextFieldComponent,
    DsSelectComponent,
  ],
  templateUrl: './add-resource-dialog.component.html',
  styleUrl: './add-resource-dialog.component.scss',
})
export class AddResourceDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddResourceDialogComponent>);
  private readonly data: AddResourceDialogData = inject(MAT_DIALOG_DATA);
  private readonly keyVaultService = inject(KeyVaultService);
  private readonly redisCacheService = inject(RedisCacheService);
  private readonly storageAccountService = inject(StorageAccountService);
  private readonly appServicePlanService = inject(AppServicePlanService);
  private readonly webAppService = inject(WebAppService);
  private readonly userAssignedIdentityService = inject(UserAssignedIdentityService);
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
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly projectService = inject(ProjectService);
  private readonly nameAvailabilityService = inject(NameAvailabilityService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly step = signal<DialogStep>('type');
  protected readonly selectedType = signal<ResourceTypeEnum | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly envFormsValid = signal(true);

  // ── Name Availability (live DNS check) ──
  protected readonly nameAvailabilityChecking = signal(false);
  protected readonly nameAvailabilityResults = signal<EnvironmentNameAvailabilityResponseItem[]>([]);
  protected readonly nameAvailabilityOverridden = signal(false);

  private static readonly NAME_AVAILABILITY_TYPES = new Set<ResourceTypeEnum>([
    ResourceTypeEnum.ContainerRegistry,
    ResourceTypeEnum.StorageAccount,
    ResourceTypeEnum.KeyVault,
    ResourceTypeEnum.RedisCache,
    ResourceTypeEnum.AppConfiguration,
    ResourceTypeEnum.ServiceBusNamespace,
    ResourceTypeEnum.EventHubNamespace,
    ResourceTypeEnum.WebApp,
    ResourceTypeEnum.FunctionApp,
    ResourceTypeEnum.SqlServer,
  ]);

  protected readonly isNameAvailabilityCheckEnabled = computed(
    () => AddResourceDialogComponent.NAME_AVAILABILITY_TYPES.has(this.selectedType()!) && !this.isExistingResource(),
  );

  protected readonly nameAvailabilityOverallState = computed<'idle' | 'checking' | 'all-ok' | 'has-unavailable' | 'has-invalid' | 'unknown'>(() => {
    if (this.nameAvailabilityChecking()) return 'checking';
    const items = this.nameAvailabilityResults();
    if (items.length === 0) return 'idle';
    if (items.some(i => i.status === 'invalid')) return 'has-invalid';
    if (items.some(i => i.status === 'unavailable')) return 'has-unavailable';
    if (items.every(i => i.status === 'available')) return 'all-ok';
    return 'unknown';
  });

  protected readonly isSubmitBlockedByNameAvailability = computed(() => {
    if (!this.isNameAvailabilityCheckEnabled()) return false;
    const state = this.nameAvailabilityOverallState();
    if (state === 'has-unavailable' || state === 'has-invalid') {
      return !this.nameAvailabilityOverridden();
    }
    return state === 'checking';
  });

  // ── Deployment Mode (WebApp / FunctionApp) ──
  protected readonly deploymentMode = signal<'Code' | 'Container'>('Code');
  protected readonly isContainerMode = computed(() => this.deploymentMode() === 'Container');
  protected readonly selectedContainerRegistryId = signal<string | null>(null);
  protected readonly acrAuthMode = signal<AcrAuthMode | null>(null);
  protected readonly availableContainerRegistries = signal<AzureResourceResponse[]>([]);

  // ── Parent resource pre-selection ──
  private readonly parentResource = this.data.parentResource;
  private readonly allowedChildTypes: ResourceTypeEnum[] | null = this.parentResource
    ? (PARENT_CHILD_RESOURCE_TYPES[this.parentResource.resourceType] as ResourceTypeEnum[] | undefined) ?? null
    : null;
  protected readonly hasParentResource = !!this.parentResource;

  // ── Plan selection state (WebApp flow) ──
  protected readonly existingPlans = signal<AzureResourceResponse[]>([]);
  protected readonly crossConfigPlans = signal<ProjectResourceResponse[]>([]);
  protected readonly plansLoading = signal(false);
  protected readonly selectedPlanId = signal<string | null>(null);
  protected readonly selectedPlanName = signal<string | null>(null);
  protected readonly isCreatingPlan = signal(false);

  protected readonly createPlanForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: ['', [Validators.required]],
    osType: ['Linux', [Validators.required]],
  });

  protected readonly environments = this.data.environments;
  protected readonly hasEnvironments = this.data.environments.length > 0;

  protected readonly needsEnvironmentSettings = computed(() => {
    const type = this.selectedType();
    return type !== null && type !== ResourceTypeEnum.UserAssignedIdentity;
  });



  protected readonly resourceTypeOptions = RESOURCE_TYPE_OPTIONS;
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;
  protected readonly resourceTypeCategories = this.allowedChildTypes
    ? RESOURCE_TYPE_CATEGORIES
        .map(cat => ({
          ...cat,
          types: cat.types.filter(t => this.allowedChildTypes!.includes(t)),
        }))
        .filter(cat => cat.types.length > 0)
    : RESOURCE_TYPE_CATEGORIES;
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
  protected readonly aspSkuOptions = APP_SERVICE_PLAN_SKU_OPTIONS;
  protected readonly runtimeStackOptions = RUNTIME_STACK_OPTIONS;
  protected readonly functionAppRuntimeStackOptions = FUNCTION_APP_RUNTIME_STACK_OPTIONS;
  protected readonly runtimeVersionOptions = signal<string[]>([]);
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
  protected readonly acrSkuOptions = ACR_SKU_OPTIONS;
  protected readonly acrPublicNetworkOptions = ACR_PUBLIC_NETWORK_OPTIONS;

  protected readonly ResourceTypeEnum = ResourceTypeEnum;

  protected readonly parentResourceSuffix = computed(() => {
    const type = this.selectedType();
    if (type === ResourceTypeEnum.ContainerApp) return 'CAE';
    if (type === ResourceTypeEnum.ApplicationInsights) return 'LAW';
    if (type === ResourceTypeEnum.ContainerAppEnvironment) return 'LAW';
    if (type === ResourceTypeEnum.SqlDatabase) return 'SQL';
    return 'ASP';
  });

  protected readonly parentResourceIcon = computed(() => {
    const suffix = this.parentResourceSuffix();
    if (suffix === 'CAE') return 'cloud_queue';
    if (suffix === 'LAW') return 'analytics';
    if (suffix === 'SQL') return 'dns';
    return 'dns';
  });

  // ── Common form (name + location + type-specific fields) ──
  protected readonly commonForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: [this.data.location, [Validators.required]],
    osType: [''],
    appServicePlanId: [''],
    containerAppEnvironmentId: [''],
    logAnalyticsWorkspaceId: [''],
    sqlServerId: [''],
    deploymentMode: ['Code'],
    containerRegistryId: [null as string | null],
    acrAuthMode: [null as AcrAuthMode | null],
    dockerImageName: [null as string | null],
    runtimeStack: [''],
    runtimeVersion: [''],
    alwaysOn: [true],
    httpsOnly: [true],
    version: ['V12'],
    administratorLogin: [''],
    collation: ['SQL_Latin1_General_CP1_CI_AS'],
    kind: ['StorageV2'],
    accessTier: ['Hot'],
    allowBlobPublicAccess: [false],
    enableHttpsTrafficOnly: [true],
    minimumTlsVersion: ['Tls12'],
    redisVersion: [6 as number | null],
    enableNonSslPort: [false],
    disableAccessKeyAuthentication: [false],
    enableAadAuth: [false],
    isExisting: [false],
  });

  protected readonly redisAadWarning = computed(() => {
    const type = this.selectedType();
    if (type !== ResourceTypeEnum.RedisCache) return false;
    const disableKey = this.commonForm.get('disableAccessKeyAuthentication')?.value;
    const aadEnabled = this.commonForm.get('enableAadAuth')?.value;
    return disableKey === true && aadEnabled !== true;
  });

  // ── Create Plan form (inline ASP creation) ──

  // ── Per-environment FormArray ──
  protected readonly envFormArray = new FormArray<FormGroup>([]);

  private readonly commonFormStatus = toSignal(this.commonForm.statusChanges, { initialValue: 'INVALID' as const });
  private readonly isExistingValue = toSignal(
    this.commonForm.get('isExisting')!.valueChanges as Observable<boolean>,
    { initialValue: false }
  );
  protected readonly isExistingResource = computed(() => this.isExistingValue() === true);

  protected readonly isCommonValid = computed(() => {
    this.commonFormStatus();
    return this.commonForm.valid;
  });

  private readonly _envStatusSub = this.envFormArray.statusChanges
    .pipe(takeUntilDestroyed())
    .subscribe(() => this.envFormsValid.set(this.envFormArray.valid));

  private readonly prefilled = new Set<number>();

  constructor() {
    this.wireNameAvailabilityCheck();

    if (this.parentResource && this.allowedChildTypes) {
      this.selectedPlanId.set(this.parentResource.id);
      this.selectedPlanName.set(this.parentResource.name);
      if (this.allowedChildTypes.length === 1) {
        const childType = this.allowedChildTypes[0];
        this.selectedType.set(childType);
        this.buildEnvForms(childType);
        this.updateCommonFormValidators(childType);
        this.prefillParentFormField(childType);
        this.step.set('common');
        if (childType === ResourceTypeEnum.WebApp || childType === ResourceTypeEnum.FunctionApp || childType === ResourceTypeEnum.ContainerApp) {
          this.loadAvailableContainerRegistries();
        }
      }
    }
  }

  private wireNameAvailabilityCheck(): void {
    const ctrl = this.commonForm.get('name');
    if (!ctrl) return;

    ctrl.valueChanges
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        debounceTime(500),
        distinctUntilChanged(),
        filter((v): v is string => typeof v === 'string' && v.trim().length > 0),
        tap(() => {
          this.nameAvailabilityChecking.set(true);
          this.nameAvailabilityResults.set([]);
          this.nameAvailabilityOverridden.set(false);
        }),
        switchMap(name => {
          const type = this.selectedType();
          if (!type || !AddResourceDialogComponent.NAME_AVAILABILITY_TYPES.has(type)) {
            this.nameAvailabilityChecking.set(false);
            return EMPTY;
          }
          return this.nameAvailabilityService
            .check$(type, {
              projectId: this.data.projectId,
              configId: this.data.configId,
              name: name.trim(),
            })
            .pipe(
              catchError(() => {
                this.nameAvailabilityChecking.set(false);
                this.nameAvailabilityResults.set([]);
                return EMPTY;
              }),
            );
        }),
      )
      .subscribe(res => {
        this.nameAvailabilityChecking.set(false);
        this.nameAvailabilityResults.set(res.environments ?? []);
      });
  }

  protected overrideNameAvailability(): void {
    this.nameAvailabilityOverridden.set(true);
  }

  private prefillParentFormField(type: ResourceTypeEnum): void {
    if (!this.parentResource) return;
    if (type === ResourceTypeEnum.ContainerApp) {
      this.commonForm.patchValue({ containerAppEnvironmentId: this.parentResource.id });
    } else if (type === ResourceTypeEnum.ApplicationInsights || type === ResourceTypeEnum.ContainerAppEnvironment) {
      this.commonForm.patchValue({ logAnalyticsWorkspaceId: this.parentResource.id });
    } else if (type === ResourceTypeEnum.SqlDatabase) {
      this.commonForm.patchValue({ sqlServerId: this.parentResource.id });
    } else {
      this.commonForm.patchValue({ appServicePlanId: this.parentResource.id });
    }
  }

  // ── Type Selection ──
  protected onSelectType(type: ResourceTypeEnum): void {
    this.selectedType.set(type);
    this.errorKey.set('');
    this.buildEnvForms(type);
    this.updateCommonFormValidators(type);

    if (this.parentResource) {
      this.prefillParentFormField(type);
      this.step.set('common');
      if (type === ResourceTypeEnum.WebApp || type === ResourceTypeEnum.FunctionApp || type === ResourceTypeEnum.ContainerApp) {
        this.loadAvailableContainerRegistries();
      }
    } else if (type === ResourceTypeEnum.WebApp || type === ResourceTypeEnum.FunctionApp || type === ResourceTypeEnum.ContainerApp || type === ResourceTypeEnum.ApplicationInsights || type === ResourceTypeEnum.ContainerAppEnvironment || type === ResourceTypeEnum.SqlDatabase) {
      this.step.set('plan-selection');
      this.loadExistingPlans();
    } else {
      this.step.set('common');
    }
  }

  // ── Plan selection (WebApp flow) ──
  private async loadAvailableContainerRegistries(): Promise<void> {
    try {
      const resourceGroups = await this.infraConfigService.getResourceGroups(this.data.configId);
      const allResourceArrays = await Promise.all(resourceGroups.map(rg => this.resourceGroupService.getResources(rg.id)));
      const localResources = allResourceArrays.flat();
      const localAcrs = localResources.filter(r => r.resourceType === 'ContainerRegistry');

      // Also load cross-config ACRs from the same project
      try {
        const projectResources = await this.projectService.getProjectResources(this.data.projectId);
        const localIds = new Set(localAcrs.map(r => r.id));
        const crossConfigAcrs = projectResources
          .filter(pr => pr.resourceType === 'ContainerRegistry' && pr.configId !== this.data.configId && !localIds.has(pr.resourceId))
          .map(pr => ({ id: pr.resourceId, resourceType: pr.resourceType, name: `${pr.resourceName} (${pr.configName})`, location: '' } as AzureResourceResponse));
        this.availableContainerRegistries.set([...localAcrs, ...crossConfigAcrs]);
      } catch {
        this.availableContainerRegistries.set(localAcrs);
      }
    } catch {
      this.availableContainerRegistries.set([]);
    }
  }

  private async loadExistingPlans(): Promise<void> {
    this.plansLoading.set(true);
    try {
      const filterType = this.selectedType() === ResourceTypeEnum.ContainerApp ? 'ContainerAppEnvironment'
        : this.selectedType() === ResourceTypeEnum.ApplicationInsights ? 'LogAnalyticsWorkspace'
        : this.selectedType() === ResourceTypeEnum.ContainerAppEnvironment ? 'LogAnalyticsWorkspace'
        : this.selectedType() === ResourceTypeEnum.SqlDatabase ? 'SqlServer'
        : 'AppServicePlan';

      // Load from all resource groups in this config
      const resourceGroups = await this.infraConfigService.getResourceGroups(this.data.configId);
      const allResourcePromises = resourceGroups.map(rg => this.resourceGroupService.getResources(rg.id));
      const allResourceArrays = await Promise.all(allResourcePromises);
      const allResources = allResourceArrays.flat();
      const plans = allResources.filter(r => r.resourceType === filterType);
      this.existingPlans.set(plans);
      const localAcrs = allResources.filter(r => r.resourceType === 'ContainerRegistry');

      // Load cross-config resources from other configs in the same project
      const projectResources = await this.projectService.getProjectResources(this.data.projectId);
      const crossConfigResources = projectResources.filter(
        r => r.resourceType === filterType && r.configId !== this.data.configId
      );
      this.crossConfigPlans.set(crossConfigResources);

      // Also include cross-config ACRs
      const localAcrIds = new Set(localAcrs.map(r => r.id));
      const crossConfigAcrs = projectResources
        .filter(pr => pr.resourceType === 'ContainerRegistry' && pr.configId !== this.data.configId && !localAcrIds.has(pr.resourceId))
        .map(pr => ({ id: pr.resourceId, resourceType: pr.resourceType, name: `${pr.resourceName} (${pr.configName})`, location: '' } as AzureResourceResponse));
      this.availableContainerRegistries.set([...localAcrs, ...crossConfigAcrs]);
    } catch {
      this.existingPlans.set([]);
      this.crossConfigPlans.set([]);
    } finally {
      this.plansLoading.set(false);
    }
  }

  protected onSelectPlan(plan: AzureResourceResponse): void {
    this.selectedPlanId.set(plan.id);
    this.selectedPlanName.set(plan.name);
    if (this.selectedType() === ResourceTypeEnum.ContainerApp) {
      this.commonForm.patchValue({ containerAppEnvironmentId: plan.id });
    } else if (this.selectedType() === ResourceTypeEnum.ApplicationInsights || this.selectedType() === ResourceTypeEnum.ContainerAppEnvironment) {
      this.commonForm.patchValue({ logAnalyticsWorkspaceId: plan.id });
    } else if (this.selectedType() === ResourceTypeEnum.SqlDatabase) {
      this.commonForm.patchValue({ sqlServerId: plan.id });
    } else {
      this.commonForm.patchValue({ appServicePlanId: plan.id });
    }
    this.step.set('common');
  }

  protected async onSelectCrossConfigPlan(plan: ProjectResourceResponse): Promise<void> {
    // Auto-create cross-config reference if not already existing
    try {
      const refs = await this.infraConfigService.getCrossConfigReferences(this.data.configId);
      const alreadyReferenced = refs.some(r => r.targetResourceId === plan.resourceId);
      if (!alreadyReferenced) {
        await this.infraConfigService.addCrossConfigReference(this.data.configId, {
          targetResourceId: plan.resourceId,
        });
      }
    } catch {
      // Non-blocking — reference may already exist
    }

    // Treat it like a normal plan selection
    this.selectedPlanId.set(plan.resourceId);
    this.selectedPlanName.set(plan.resourceName);
    if (this.selectedType() === ResourceTypeEnum.ContainerApp) {
      this.commonForm.patchValue({ containerAppEnvironmentId: plan.resourceId });
    } else if (this.selectedType() === ResourceTypeEnum.ApplicationInsights || this.selectedType() === ResourceTypeEnum.ContainerAppEnvironment) {
      this.commonForm.patchValue({ logAnalyticsWorkspaceId: plan.resourceId });
    } else if (this.selectedType() === ResourceTypeEnum.SqlDatabase) {
      this.commonForm.patchValue({ sqlServerId: plan.resourceId });
    } else {
      this.commonForm.patchValue({ appServicePlanId: plan.resourceId });
    }
    this.step.set('common');
  }

  protected readonly createSqlServerForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: ['', [Validators.required]],
    version: ['V12', [Validators.required]],
    administratorLogin: ['', [Validators.required]],
  });

  protected readonly sqlServerVersionOptions = [
    { label: '12.0', value: 'V12' },
  ];

  protected readonly sqlDatabaseSkuOptions = [
    { label: 'Basic', value: 'Basic' },
    { label: 'Standard', value: 'Standard' },
    { label: 'Premium', value: 'Premium' },
    { label: 'General Purpose', value: 'GeneralPurpose' },
    { label: 'Business Critical', value: 'BusinessCritical' },
    { label: 'Hyperscale', value: 'Hyperscale' },
  ];

  protected readonly sqlMinTlsOptions = [
    { label: 'TLS 1.0', value: '1.0' },
    { label: 'TLS 1.1', value: '1.1' },
    { label: 'TLS 1.2', value: '1.2' },
  ];

  protected readonly serviceBusSkuOptions = [
    { label: 'Basic', value: 'Basic' },
    { label: 'Standard', value: 'Standard' },
    { label: 'Premium', value: 'Premium' },
  ];

  protected readonly serviceBusTlsOptions = [
    { label: 'TLS 1.0', value: '1.0' },
    { label: 'TLS 1.1', value: '1.1' },
    { label: 'TLS 1.2', value: '1.2' },
  ];

  protected onStartCreatePlan(): void {
    this.createPlanForm.patchValue({ location: this.data.location });
    const osCtrl = this.createPlanForm.get('osType')!;
    if (this.parentResourceSuffix() === 'ASP') {
      osCtrl.setValidators([Validators.required]);
    } else {
      osCtrl.clearValidators();
    }
    osCtrl.updateValueAndValidity();
    if (this.parentResourceSuffix() === 'SQL') {
      this.createSqlServerForm.patchValue({ location: this.data.location });
    }
    this.step.set('create-plan');
    this.errorKey.set('');
  }

  protected onBackToPlanSelection(): void {
    this.step.set('plan-selection');
    this.errorKey.set('');
  }

  protected onSkipPlanSelection(): void {
    this.selectedPlanId.set(null);
    this.selectedPlanName.set(null);
    this.commonForm.patchValue({ logAnalyticsWorkspaceId: '' });
    this.step.set('common');
  }

  protected async onCreatePlanAndContinue(): Promise<void> {
    if (this.createPlanForm.invalid) return;
    this.isCreatingPlan.set(true);
    this.errorKey.set('');

    const planData = this.createPlanForm.getRawValue();
    try {
      if (this.selectedType() === ResourceTypeEnum.ContainerApp) {
        const created = await this.containerAppEnvironmentService.create({
          resourceGroupId: this.data.resourceGroupId,
          name: planData.name!,
          location: planData.location!,
          environmentSettings: this.environments.map(env => ({
            environmentName: env.name,
            sku: 'Consumption',
          })),
        });
        this.selectedPlanId.set(created.id);
        this.selectedPlanName.set(created.name);
        this.commonForm.patchValue({ containerAppEnvironmentId: created.id });
      } else if (this.selectedType() === ResourceTypeEnum.ApplicationInsights || this.selectedType() === ResourceTypeEnum.ContainerAppEnvironment) {
        const created = await this.logAnalyticsWorkspaceService.create({
          resourceGroupId: this.data.resourceGroupId,
          name: planData.name!,
          location: planData.location!,
          environmentSettings: this.environments.map(env => ({
            environmentName: env.name,
            sku: 'PerGB2018',
          })),
        });
        this.selectedPlanId.set(created.id);
        this.selectedPlanName.set(created.name);
        this.commonForm.patchValue({ logAnalyticsWorkspaceId: created.id });
      } else if (this.selectedType() === ResourceTypeEnum.SqlDatabase) {
        const sqlData = this.createSqlServerForm.getRawValue();
        const created = await this.sqlServerService.create({
          resourceGroupId: this.data.resourceGroupId,
          name: sqlData.name!,
          location: sqlData.location!,
          version: sqlData.version!,
          administratorLogin: sqlData.administratorLogin!,
          environmentSettings: this.environments.map(env => ({
            environmentName: env.name,
            minimalTlsVersion: '1.2',
          })),
        });
        this.selectedPlanId.set(created.id);
        this.selectedPlanName.set(created.name);
        this.commonForm.patchValue({ sqlServerId: created.id });
      } else {
        const created = await this.appServicePlanService.create({
          resourceGroupId: this.data.resourceGroupId,
          name: planData.name!,
          location: planData.location!,
          osType: planData.osType!,
          environmentSettings: this.environments.map(env => ({
            environmentName: env.name,
            sku: 'B1',
            capacity: 1,
          })),
        });
        this.selectedPlanId.set(created.id);
        this.selectedPlanName.set(created.name);
        this.commonForm.patchValue({ appServicePlanId: created.id });
      }
      this.step.set('common');
    } catch {
      this.errorKey.set('CONFIG_DETAIL.RESOURCES.FORM.CREATE_PLAN_ERROR_' + this.parentResourceSuffix());
    } finally {
      this.isCreatingPlan.set(false);
    }
  }

  // ── Navigation ──
  protected onBackToType(): void {
    if (this.parentResource && this.allowedChildTypes?.length === 1) {
      this.dialogRef.close(false);
      return;
    }
    this.selectedType.set(null);
    this.selectedPlanId.set(this.parentResource?.id ?? null);
    this.selectedPlanName.set(this.parentResource?.name ?? null);
    this.deploymentMode.set('Code');
    this.selectedContainerRegistryId.set(null);
    this.acrAuthMode.set(null);
    this.commonForm.patchValue({
      deploymentMode: 'Code',
      containerRegistryId: null,
      acrAuthMode: null,
      dockerImageName: null,
    });
    this.step.set('type');
    this.errorKey.set('');
    this.clearExtraValidators();
  }

  protected onBackToCommon(): void {
    this.step.set('common');
    this.errorKey.set('');
  }

  private resolveAcrAuthMode(containerRegistryId: string | null | undefined, acrAuthMode: AcrAuthMode | null | undefined): AcrAuthMode | null {
    if (!containerRegistryId) {
      return null;
    }

    return acrAuthMode ?? 'ManagedIdentity';
  }

  protected onDeploymentModeChange(mode: 'Code' | 'Container'): void {
    this.deploymentMode.set(mode);
    this.commonForm.patchValue({ deploymentMode: mode });
    if (mode === 'Code') {
      this.selectedContainerRegistryId.set(null);
      this.acrAuthMode.set(null);
      this.commonForm.patchValue({ containerRegistryId: null, acrAuthMode: null, dockerImageName: null });
      this.commonForm.controls.runtimeStack.setValidators([Validators.required]);
      this.commonForm.controls.runtimeVersion.setValidators([Validators.required]);
    } else {
      const nextAcrAuthMode = this.resolveAcrAuthMode(this.selectedContainerRegistryId(), this.acrAuthMode());
      this.acrAuthMode.set(nextAcrAuthMode);
      this.commonForm.patchValue({ acrAuthMode: nextAcrAuthMode });
      this.commonForm.controls.runtimeStack.clearValidators();
      this.commonForm.controls.runtimeVersion.clearValidators();
    }
    this.commonForm.controls.runtimeStack.updateValueAndValidity();
    this.commonForm.controls.runtimeVersion.updateValueAndValidity();
  }

  protected onContainerRegistryChange(acrId: string | null): void {
    const nextAcrAuthMode = this.resolveAcrAuthMode(acrId, this.acrAuthMode());
    this.selectedContainerRegistryId.set(acrId);
    this.acrAuthMode.set(nextAcrAuthMode);
    this.commonForm.patchValue({
      containerRegistryId: acrId,
      acrAuthMode: nextAcrAuthMode,
    });
  }

  protected onAcrAuthModeChange(mode: AcrAuthMode): void {
    const nextAcrAuthMode = this.resolveAcrAuthMode(this.selectedContainerRegistryId(), mode);
    this.acrAuthMode.set(nextAcrAuthMode);
    this.commonForm.patchValue({ acrAuthMode: nextAcrAuthMode });
  }

  protected onRuntimeStackChange(stack: string): void {
    const type = this.selectedType();
    const map = type === ResourceTypeEnum.FunctionApp
      ? FUNCTIONAPP_RUNTIME_VERSION_MAP
      : WEBAPP_RUNTIME_VERSION_MAP;
    const versions = map[stack] ?? [];
    this.runtimeVersionOptions.set(versions);
    this.commonForm.patchValue({ runtimeVersion: versions[0] ?? '' });
  }

  protected onBackFromCommon(): void {
    if (this.parentResource) {
      this.onBackToType();
    } else if (this.selectedType() === ResourceTypeEnum.WebApp || this.selectedType() === ResourceTypeEnum.FunctionApp || this.selectedType() === ResourceTypeEnum.ContainerApp || this.selectedType() === ResourceTypeEnum.ApplicationInsights || this.selectedType() === ResourceTypeEnum.SqlDatabase) {
      this.step.set('plan-selection');
    } else {
      this.onBackToType();
    }
    this.errorKey.set('');
  }

  protected onNextToEnvironments(): void {
    if (this.commonForm.invalid || this.isSubmitBlockedByNameAvailability()) return;
    if (!this.needsEnvironmentSettings() || this.isExistingResource()) {
      this.onSubmit();
      return;
    }
    if (!this.hasEnvironments) return;
    this.step.set('environments');
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }

  // ── Environment Forms ──
  private buildEnvForms(type: ResourceTypeEnum): void {
    this.envFormArray.clear();
    this.prefilled.clear();
    for (const _ of this.environments) {
      this.envFormArray.push(this.createEnvFormGroup(type));
    }
    this.envFormsValid.set(this.envFormArray.valid);
  }

  private createEnvFormGroup(type: ResourceTypeEnum): FormGroup {
    switch (type) {
      case ResourceTypeEnum.KeyVault:
        return this.fb.group({
          sku: ['Standard', [Validators.required]],
        });
      case ResourceTypeEnum.RedisCache:
        return this.fb.group({
          skuName: ['Standard', [Validators.required]],
          capacity: [1, [Validators.required, Validators.min(0)]],
          maxMemoryPolicy: ['NoEviction', [Validators.required]],
        });
      case ResourceTypeEnum.StorageAccount:
        return this.fb.group({
          sku: ['Standard_LRS', [Validators.required]],
        });
      case ResourceTypeEnum.AppServicePlan:
        return this.fb.group({
          sku: ['B1', [Validators.required]],
          capacity: [1, [Validators.required, Validators.min(1)]],
        });
      case ResourceTypeEnum.WebApp:
        return this.fb.group({
          alwaysOn: [true],
          httpsOnly: [true],
        });
      case ResourceTypeEnum.FunctionApp:
        return this.fb.group({
          httpsOnly: [true],
          maxInstanceCount: [null as number | null],
        });
      case ResourceTypeEnum.UserAssignedIdentity:
        return this.fb.group({});
      case ResourceTypeEnum.AppConfiguration:
        return this.fb.group({
          sku: ['Standard', [Validators.required]],
          softDeleteRetentionInDays: [7],
          purgeProtectionEnabled: [false],
          disableLocalAuth: [false],
          publicNetworkAccess: ['Enabled', [Validators.required]],
        });
      case ResourceTypeEnum.ContainerAppEnvironment:
        return this.fb.group({
          sku: ['Consumption', [Validators.required]],
          workloadProfileType: ['Consumption'],
          internalLoadBalancerEnabled: [false],
          zoneRedundancyEnabled: [false],
        });
      case ResourceTypeEnum.ContainerApp:
        return this.fb.group({
          cpuCores: ['0.25'],
          memoryGi: ['0.5Gi'],
          minReplicas: [0],
          maxReplicas: [10],
          ingressEnabled: [true],
          ingressTargetPort: [80],
          ingressExternal: [true],
          transportMethod: ['auto'],
          readinessProbeEnabled: [false],
          readinessProbePath: [null as string | null],
          readinessProbePort: [null as number | null],
          livenessProbeEnabled: [false],
          livenessProbePath: [null as string | null],
          livenessProbePort: [null as number | null],
          startupProbeEnabled: [false],
          startupProbePath: [null as string | null],
          startupProbePort: [null as number | null],
        });
      case ResourceTypeEnum.LogAnalyticsWorkspace:
        return this.fb.group({
          sku: ['PerGB2018'],
          retentionInDays: [30],
          dailyQuotaGb: [null as number | null],
        });
      case ResourceTypeEnum.ApplicationInsights:
        return this.fb.group({
          samplingPercentage: [100],
          retentionInDays: [90],
          disableIpMasking: [false],
          disableLocalAuth: [false],
          ingestionMode: ['LogAnalytics'],
        });
      case ResourceTypeEnum.CosmosDb:
        return this.fb.group({
          databaseApiType: ['SQL'],
          consistencyLevel: ['Session'],
          maxStalenessPrefix: [null as number | null],
          maxIntervalInSeconds: [null as number | null],
          enableAutomaticFailover: [false],
          enableMultipleWriteLocations: [false],
          backupPolicyType: ['Continuous'],
          enableFreeTier: [false],
        });
      case ResourceTypeEnum.SqlServer:
        return this.fb.group({
          minimalTlsVersion: ['1.2'],
        });
      case ResourceTypeEnum.SqlDatabase:
        return this.fb.group({
          sku: ['Basic', [Validators.required]],
          maxSizeGb: [null as number | null],
          zoneRedundant: [false],
        });
      case ResourceTypeEnum.ServiceBusNamespace:
        return this.fb.group({
          sku: ['Standard', [Validators.required]],
          capacity: [null as number | null],
          zoneRedundant: [false],
          disableLocalAuth: [false],
          minimumTlsVersion: ['1.2'],
        });
      case ResourceTypeEnum.ContainerRegistry:
        return this.fb.group({
          sku: ['Standard', [Validators.required]],
          adminUserEnabled: [false],
          publicNetworkAccess: ['Enabled', [Validators.required]],
          zoneRedundancy: [false],
        });
      default:
        return this.fb.group({});
    }
  }

  protected getEnvFormGroup(index: number): FormGroup {
    return this.envFormArray.at(index);
  }

  protected onProbeToggle(envIndex: number, probeType: 'readiness' | 'liveness' | 'startup', enabled: boolean): void {
    const envGroup = this.envFormArray.at(envIndex);
    const defaults: Record<string, { path: string; port: number }> = {
      readiness: { path: '/healthz/ready', port: 8080 },
      liveness: { path: '/healthz/live', port: 8080 },
      startup: { path: '/healthz/startup', port: 8080 },
    };

    envGroup.patchValue({
      [`${probeType}ProbeEnabled`]: enabled,
      [`${probeType}ProbePath`]: enabled ? defaults[probeType].path : null,
      [`${probeType}ProbePort`]: enabled ? defaults[probeType].port : null,
    });
  }

  protected onTabChange(index: number): void {
    if (index > 0 && !this.prefilled.has(index) && this.envFormArray.length > 1) {
      const firstGroup = this.envFormArray.at(0);
      const targetGroup = this.envFormArray.at(index);
      targetGroup.patchValue(firstGroup.getRawValue());
      this.prefilled.add(index);
    }
  }

  protected copyFromFirst(): void {
    if (this.envFormArray.length < 2) return;
    const firstValue = this.envFormArray.at(0).getRawValue();
    for (let i = 1; i < this.envFormArray.length; i++) {
      this.envFormArray.at(i).patchValue(firstValue);
      this.prefilled.add(i);
    }
  }

  // ── Submit ──
  protected async onSubmit(): Promise<void> {
    const type = this.selectedType();
    if (!type || this.commonForm.invalid || !this.envFormsValid() || this.isSubmitBlockedByNameAvailability()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const common = this.commonForm.getRawValue();

    try {
      switch (type) {
        case ResourceTypeEnum.KeyVault: {
          await this.keyVaultService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            environmentSettings: this.buildKeyVaultEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.RedisCache: {
          await this.redisCacheService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            redisVersion: common.redisVersion ? Number(common.redisVersion) : null,
            enableNonSslPort: common.enableNonSslPort ?? false,
            minimumTlsVersion: common.minimumTlsVersion || null,
            disableAccessKeyAuthentication: common.disableAccessKeyAuthentication ?? false,
            enableAadAuth: common.enableAadAuth ?? false,
            environmentSettings: this.buildRedisCacheEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.StorageAccount: {
          await this.storageAccountService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            kind: common.kind!,
            accessTier: common.accessTier!,
            allowBlobPublicAccess: common.allowBlobPublicAccess ?? false,
            enableHttpsTrafficOnly: common.enableHttpsTrafficOnly ?? true,
            minimumTlsVersion: common.minimumTlsVersion!,
            environmentSettings: this.buildStorageAccountEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.AppServicePlan: {
          await this.appServicePlanService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            osType: common.osType!,
            environmentSettings: this.buildAppServicePlanEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.WebApp: {
          const acrAuthMode = common.deploymentMode === 'Container'
            ? this.resolveAcrAuthMode(common.containerRegistryId || null, common.acrAuthMode as AcrAuthMode | null | undefined)
            : null;

          await this.webAppService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            appServicePlanId: common.appServicePlanId!,
            deploymentMode: common.deploymentMode || 'Code',
            containerRegistryId: common.deploymentMode === 'Container' ? (common.containerRegistryId || null) : null,
            acrAuthMode,
            dockerImageName: common.deploymentMode === 'Container' ? (common.dockerImageName || null) : null,
            runtimeStack: common.runtimeStack!,
            runtimeVersion: common.runtimeVersion!,
            alwaysOn: common.alwaysOn!,
            httpsOnly: common.httpsOnly!,
            environmentSettings: this.buildWebAppEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.FunctionApp: {
          const acrAuthMode = common.deploymentMode === 'Container'
            ? this.resolveAcrAuthMode(common.containerRegistryId || null, common.acrAuthMode as AcrAuthMode | null | undefined)
            : null;

          await this.functionAppService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            appServicePlanId: common.appServicePlanId!,
            deploymentMode: common.deploymentMode || 'Code',
            containerRegistryId: common.deploymentMode === 'Container' ? (common.containerRegistryId || null) : null,
            acrAuthMode,
            dockerImageName: common.deploymentMode === 'Container' ? (common.dockerImageName || null) : null,
            runtimeStack: common.runtimeStack!,
            runtimeVersion: common.runtimeVersion!,
            httpsOnly: common.httpsOnly!,
            environmentSettings: this.buildFunctionAppEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.UserAssignedIdentity: {
          await this.userAssignedIdentityService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            isExisting: common.isExisting ?? false,
          });
          break;
        }
        case ResourceTypeEnum.AppConfiguration: {
          await this.appConfigurationService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            environmentSettings: this.buildAppConfigurationEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.ContainerAppEnvironment: {
          await this.containerAppEnvironmentService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            logAnalyticsWorkspaceId: common.logAnalyticsWorkspaceId || null,
            environmentSettings: this.buildContainerAppEnvironmentEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.ContainerApp: {
          const acrAuthMode = this.resolveAcrAuthMode(common.containerRegistryId || null, common.acrAuthMode as AcrAuthMode | null | undefined);

          await this.containerAppService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            containerAppEnvironmentId: common.containerAppEnvironmentId!,
            containerRegistryId: common.containerRegistryId || null,
            acrAuthMode,
            dockerImageName: common.dockerImageName || null,
            environmentSettings: this.buildContainerAppEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.LogAnalyticsWorkspace: {
          await this.logAnalyticsWorkspaceService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            environmentSettings: this.buildLogAnalyticsWorkspaceEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.ApplicationInsights: {
          await this.applicationInsightsService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            logAnalyticsWorkspaceId: common.logAnalyticsWorkspaceId!,
            environmentSettings: this.buildApplicationInsightsEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.CosmosDb: {
          await this.cosmosDbService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            environmentSettings: this.buildCosmosDbEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.SqlServer: {
          await this.sqlServerService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            version: common.version!,
            administratorLogin: common.administratorLogin!,
            environmentSettings: this.buildSqlServerEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.SqlDatabase: {
          await this.sqlDatabaseService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            sqlServerId: common.sqlServerId!,
            collation: common.collation!,
            environmentSettings: this.buildSqlDatabaseEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.ServiceBusNamespace: {
          await this.serviceBusNamespaceService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            environmentSettings: this.buildServiceBusNamespaceEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
        case ResourceTypeEnum.ContainerRegistry: {
          await this.containerRegistryService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            environmentSettings: this.buildContainerRegistryEnvironmentSettings(),
            isExisting: common.isExisting ?? false,
            });
          break;
        }
      }
      this.dialogRef.close(true);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.RESOURCES.ADD_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private buildKeyVaultEnvironmentSettings(): KeyVaultEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
      };
    });
  }

  private buildRedisCacheEnvironmentSettings(): RedisCacheEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.skuName || null,
        capacity: raw.capacity ? Number(raw.capacity) : null,
        maxMemoryPolicy: raw.maxMemoryPolicy || null,
      };
    });
  }

  private buildStorageAccountEnvironmentSettings(): StorageAccountEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
      };
    });
  }

  private buildAppServicePlanEnvironmentSettings(): AppServicePlanEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        capacity: raw.capacity ? Number(raw.capacity) : null,
      };
    });
  }

  private buildWebAppEnvironmentSettings(): WebAppEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        alwaysOn: raw.alwaysOn ?? null,
        httpsOnly: raw.httpsOnly ?? null,
        dockerImageTag: raw.dockerImageTag || null,
      };
    });
  }

  private buildFunctionAppEnvironmentSettings(): FunctionAppEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        httpsOnly: raw.httpsOnly ?? null,
        maxInstanceCount: raw.maxInstanceCount != null ? Number(raw.maxInstanceCount) : null,
        dockerImageTag: raw.dockerImageTag || null,
      };
    });
  }

  private buildAppConfigurationEnvironmentSettings(): AppConfigurationEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        softDeleteRetentionInDays: raw.softDeleteRetentionInDays != null ? Number(raw.softDeleteRetentionInDays) : null,
        purgeProtectionEnabled: raw.purgeProtectionEnabled ?? null,
        disableLocalAuth: raw.disableLocalAuth ?? null,
        publicNetworkAccess: raw.publicNetworkAccess || null,
      };
    });
  }

  private buildContainerAppEnvironmentEnvironmentSettings(): ContainerAppEnvironmentEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        workloadProfileType: raw.workloadProfileType || null,
        internalLoadBalancerEnabled: raw.internalLoadBalancerEnabled ?? null,
        zoneRedundancyEnabled: raw.zoneRedundancyEnabled ?? null,
      };
    });
  }

  private buildContainerAppEnvironmentSettings(): ContainerAppEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        cpuCores: raw.cpuCores || null,
        memoryGi: raw.memoryGi || null,
        minReplicas: raw.minReplicas != null ? Number(raw.minReplicas) : null,
        maxReplicas: raw.maxReplicas != null ? Number(raw.maxReplicas) : null,
        ingressEnabled: raw.ingressEnabled ?? null,
        ingressTargetPort: raw.ingressTargetPort != null ? Number(raw.ingressTargetPort) : null,
        ingressExternal: raw.ingressExternal ?? null,
        transportMethod: raw.transportMethod || null,
        readinessProbePath: raw.readinessProbePath || null,
        readinessProbePort: raw.readinessProbePort != null ? Number(raw.readinessProbePort) : null,
        livenessProbePath: raw.livenessProbePath || null,
        livenessProbePort: raw.livenessProbePort != null ? Number(raw.livenessProbePort) : null,
        startupProbePath: raw.startupProbePath || null,
        startupProbePort: raw.startupProbePort != null ? Number(raw.startupProbePort) : null,
      };
    });
  }

  private buildLogAnalyticsWorkspaceEnvironmentSettings(): LogAnalyticsWorkspaceEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        retentionInDays: raw.retentionInDays != null ? Number(raw.retentionInDays) : null,
        dailyQuotaGb: raw.dailyQuotaGb != null ? Number(raw.dailyQuotaGb) : null,
      };
    });
  }

  private buildApplicationInsightsEnvironmentSettings(): ApplicationInsightsEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        samplingPercentage: raw.samplingPercentage != null ? Number(raw.samplingPercentage) : null,
        retentionInDays: raw.retentionInDays != null ? Number(raw.retentionInDays) : null,
        disableIpMasking: raw.disableIpMasking ?? null,
        disableLocalAuth: raw.disableLocalAuth ?? null,
        ingestionMode: raw.ingestionMode || null,
      };
    });
  }

  private buildCosmosDbEnvironmentSettings(): CosmosDbEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
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

  private buildSqlServerEnvironmentSettings(): SqlServerEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        minimalTlsVersion: raw.minimalTlsVersion || null,
      };
    });
  }

  private buildSqlDatabaseEnvironmentSettings(): SqlDatabaseEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        maxSizeGb: raw.maxSizeGb != null ? Number(raw.maxSizeGb) : null,
        zoneRedundant: raw.zoneRedundant ?? null,
      };
    });
  }

  private buildServiceBusNamespaceEnvironmentSettings(): ServiceBusNamespaceEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        capacity: raw.capacity != null ? Number(raw.capacity) : null,
        zoneRedundant: raw.zoneRedundant ?? null,
        disableLocalAuth: raw.disableLocalAuth ?? null,
        minimumTlsVersion: raw.minimumTlsVersion || null,
      };
    });
  }

  private buildContainerRegistryEnvironmentSettings(): ContainerRegistryEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      return {
        environmentName: env.name,
        sku: raw.sku || null,
        adminUserEnabled: raw.adminUserEnabled ?? null,
        publicNetworkAccess: raw.publicNetworkAccess || null,
        zoneRedundancy: raw.zoneRedundancy ?? null,
      };
    });
  }

  private updateCommonFormValidators(type: ResourceTypeEnum): void {
    this.clearExtraValidators();
    if (type === ResourceTypeEnum.AppServicePlan) {
      this.commonForm.controls.osType.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.WebApp || type === ResourceTypeEnum.FunctionApp) {
      this.commonForm.controls.runtimeStack.setValidators([Validators.required]);
      this.commonForm.controls.runtimeVersion.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.ContainerApp) {
      this.commonForm.controls.containerAppEnvironmentId.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.ApplicationInsights) {
      this.commonForm.controls.logAnalyticsWorkspaceId.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.RedisCache) {
      this.commonForm.controls.redisVersion.setValidators([Validators.required]);
      this.commonForm.controls.minimumTlsVersion.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.SqlServer) {
      this.commonForm.controls.version.setValidators([Validators.required]);
      this.commonForm.controls.administratorLogin.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.SqlDatabase) {
      this.commonForm.controls.sqlServerId.setValidators([Validators.required]);
      this.commonForm.controls.collation.setValidators([Validators.required]);
    } else if (type === ResourceTypeEnum.StorageAccount) {
      this.commonForm.controls.kind.setValidators([Validators.required]);
      this.commonForm.controls.accessTier.setValidators([Validators.required]);
      this.commonForm.controls.minimumTlsVersion.setValidators([Validators.required]);
    }
    this.commonForm.controls.osType.updateValueAndValidity();
    this.commonForm.controls.runtimeStack.updateValueAndValidity();
    this.commonForm.controls.runtimeVersion.updateValueAndValidity();
    this.commonForm.controls.containerAppEnvironmentId.updateValueAndValidity();
    this.commonForm.controls.logAnalyticsWorkspaceId.updateValueAndValidity();
    this.commonForm.controls.sqlServerId.updateValueAndValidity();
    this.commonForm.controls.version.updateValueAndValidity();
    this.commonForm.controls.administratorLogin.updateValueAndValidity();
    this.commonForm.controls.collation.updateValueAndValidity();
    this.commonForm.controls.kind.updateValueAndValidity();
    this.commonForm.controls.accessTier.updateValueAndValidity();
    this.commonForm.controls.minimumTlsVersion.updateValueAndValidity();
  }

  private clearExtraValidators(): void {
    this.commonForm.controls.osType.clearValidators();
    this.commonForm.controls.runtimeStack.clearValidators();
    this.commonForm.controls.runtimeVersion.clearValidators();
    this.commonForm.controls.containerAppEnvironmentId.clearValidators();
    this.commonForm.controls.logAnalyticsWorkspaceId.clearValidators();
    this.commonForm.controls.sqlServerId.clearValidators();
    this.commonForm.controls.version.clearValidators();
    this.commonForm.controls.administratorLogin.clearValidators();
    this.commonForm.controls.collation.clearValidators();
    this.commonForm.controls.kind.clearValidators();
    this.commonForm.controls.accessTier.clearValidators();
    this.commonForm.controls.minimumTlsVersion.clearValidators();
    this.commonForm.controls.osType.updateValueAndValidity();
    this.commonForm.controls.runtimeStack.updateValueAndValidity();
    this.commonForm.controls.runtimeVersion.updateValueAndValidity();
    this.commonForm.controls.containerAppEnvironmentId.updateValueAndValidity();
    this.commonForm.controls.logAnalyticsWorkspaceId.updateValueAndValidity();
    this.commonForm.controls.sqlServerId.updateValueAndValidity();
    this.commonForm.controls.version.updateValueAndValidity();
    this.commonForm.controls.administratorLogin.updateValueAndValidity();
    this.commonForm.controls.collation.updateValueAndValidity();
    this.commonForm.controls.kind.updateValueAndValidity();
    this.commonForm.controls.accessTier.updateValueAndValidity();
    this.commonForm.controls.minimumTlsVersion.updateValueAndValidity();
  }
}
