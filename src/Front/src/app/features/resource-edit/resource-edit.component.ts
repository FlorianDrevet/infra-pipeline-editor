import { Component, DestroyRef, OnInit, OnDestroy, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { EMPTY, Subscription, catchError, debounceTime, distinctUntilChanged, filter, switchMap, tap } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
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
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatExpansionModule } from '@angular/material/expansion';
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
import { StorageAccountResponse, StorageAccountEnvironmentConfigEntry, BlobContainerResponse, StorageQueueResponse, StorageTableResponse, CorsRuleEntry, CorsRuleResponse, BlobLifecycleRuleEntry } from '../../shared/interfaces/storage-account.interface';
import { AppServicePlanResponse, AppServicePlanEnvironmentConfigEntry } from '../../shared/interfaces/app-service-plan.interface';
import { WebAppResponse, WebAppEnvironmentConfigEntry } from '../../shared/interfaces/web-app.interface';
import { FunctionAppResponse, FunctionAppEnvironmentConfigEntry } from '../../shared/interfaces/function-app.interface';
import { AppConfigurationResponse, AppConfigurationEnvironmentConfigEntry } from '../../shared/interfaces/app-configuration.interface';
import { ContainerAppEnvironmentResponse, ContainerAppEnvironmentEnvironmentConfigEntry } from '../../shared/interfaces/container-app-environment.interface';
import { ContainerAppResponse, ContainerAppEnvironmentConfigEntry } from '../../shared/interfaces/container-app.interface';
import { LogAnalyticsWorkspaceResponse, LogAnalyticsWorkspaceEnvironmentConfigEntry } from '../../shared/interfaces/log-analytics-workspace.interface';
import { ApplicationInsightsResponse, ApplicationInsightsEnvironmentConfigEntry } from '../../shared/interfaces/application-insights.interface';
import { CosmosDbResponse, CosmosDbEnvironmentConfigEntry } from '../../shared/interfaces/cosmos-db.interface';
import { ServiceBusNamespaceResponse, ServiceBusNamespaceEnvironmentConfigEntry } from '../../shared/interfaces/service-bus-namespace.interface';
import { AcrAuthMode, ContainerRegistryResponse, ContainerRegistryEnvironmentConfigEntry } from '../../shared/interfaces/container-registry.interface';
import { SqlServerResponse, SqlServerEnvironmentConfigEntry } from '../../shared/interfaces/sql-server.interface';
import { SqlDatabaseResponse, SqlDatabaseEnvironmentConfigEntry } from '../../shared/interfaces/sql-database.interface';
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
import { ServiceBusNamespaceService } from '../../shared/services/service-bus-namespace.service';
import { ContainerRegistryService } from '../../shared/services/container-registry.service';
import { SqlServerService } from '../../shared/services/sql-server.service';
import { SqlDatabaseService } from '../../shared/services/sql-database.service';
import { UserAssignedIdentityService } from '../../shared/services/user-assigned-identity.service';
import { NameAvailabilityService } from '../../shared/services/name-availability.service';
import { EnvironmentNameAvailabilityResponseItem } from '../../shared/interfaces/name-availability.interface';
import { InfrastructureConfigResponse, EnvironmentDefinitionResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectResponse, ProjectPipelineVariableGroupResponse } from '../../shared/interfaces/project.interface';
import { RoleAssignmentResponse, AzureRoleDefinitionResponse, IdentityRoleAssignmentResponse, RoleAssignmentImpactResponse, ACR_PULL_ROLE_DEFINITION_ID } from '../../shared/interfaces/role-assignment.interface';
import { AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { RESOURCE_TYPE_ICONS } from '../config-detail/enums/resource-type.enum';
import { LOCATION_OPTIONS } from '../../shared/enums/location.enum';
import { OS_TYPE_OPTIONS } from '../config-detail/enums/os-type.enum';
import { RUNTIME_STACK_OPTIONS } from '../config-detail/enums/runtime-stack.enum';
import { FUNCTION_APP_RUNTIME_STACK_OPTIONS } from '../config-detail/enums/function-app-runtime-stack.enum';
import { APP_SERVICE_PLAN_SKU_OPTIONS } from '../config-detail/enums/app-service-plan-sku.enum';
import { AddRoleAssignmentDialogComponent, AddRoleAssignmentDialogData, AddRoleAssignmentDialogResult } from './add-role-assignment-dialog/add-role-assignment-dialog.component';
import { AddAppSettingDialogComponent, AddAppSettingDialogData } from './add-app-setting-dialog/add-app-setting-dialog.component';
import { EditStaticAppSettingDialogComponent, EditStaticAppSettingDialogData } from './edit-static-app-setting-dialog/edit-static-app-setting-dialog.component';
import { AppSettingService } from '../../shared/services/app-setting.service';
import { SecureParameterMappingService } from '../../shared/services/secure-parameter-mapping.service';
import { SecureParameterMappingResponse } from '../../shared/interfaces/secure-parameter-mapping.interface';
import { AppSettingResponse } from '../../shared/interfaces/app-setting.interface';
import { AppConfigurationKeyService } from './services/app-configuration-key.service';
import { AppConfigurationKeyResponse } from './models/app-configuration-key.interface';
import { AddAppConfigKeyDialogComponent, AddAppConfigKeyDialogData } from './add-app-config-key-dialog/add-app-config-key-dialog.component';
import { RoleAssignmentImpactDialogComponent, RoleAssignmentImpactDialogData } from './role-assignment-impact-dialog/role-assignment-impact-dialog.component';
import { CreateUaiDialogComponent, CreateUaiDialogData } from './create-uai-dialog/create-uai-dialog.component';
import { CustomDomainService } from '../../shared/services/custom-domain.service';
import { CustomDomainResponse, AddCustomDomainRequest } from '../../shared/interfaces/custom-domain.interface';
import { AddCustomDomainDialogComponent, AddCustomDomainDialogData } from './add-custom-domain-dialog/add-custom-domain-dialog.component';
import { CompactSelectComponent } from '../../shared/components/compact-select/compact-select.component';
import { DeploymentConfigComponent } from '../../shared/components/deployment-config/deployment-config.component';
import { ToggleSectionCardComponent } from '../../shared/components/toggle-section-card/toggle-section-card.component';

/** Key Vault missing role entry for the KV access warning banner */
interface KvMissingRoleEntry {
  keyVaultResourceId: string;
  keyVaultName: string;
  missingRoleName: string | null;
  missingRoleDefinitionId: string | null;
  affectedSettingsCount: number;
  checking: boolean;
  assigning: boolean;
  selectedIdentityType: 'UserAssigned' | 'SystemAssigned';
  selectedUaiId: string | null;
}

/** Union type for any loaded resource */
type ResourceData = KeyVaultResponse | RedisCacheResponse | StorageAccountResponse | AppServicePlanResponse | WebAppResponse | FunctionAppResponse | UserAssignedIdentityResponse | AppConfigurationResponse | ContainerAppEnvironmentResponse | ContainerAppResponse | LogAnalyticsWorkspaceResponse | ApplicationInsightsResponse | CosmosDbResponse | ServiceBusNamespaceResponse | ContainerRegistryResponse | SqlServerResponse | SqlDatabaseResponse;

type CorsServiceKey = 'blob' | 'table';
type CorsListField = 'allowedOrigins' | 'allowedHeaders' | 'exposedHeaders';
type CorsMethodField = 'allowedMethods';
type CorsFieldKey = CorsListField | CorsMethodField | 'maxAgeInSeconds';

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

const STORAGE_CORS_METHOD_OPTIONS = ['DELETE', 'GET', 'HEAD', 'MERGE', 'OPTIONS', 'PATCH', 'POST', 'PUT'];
const STORAGE_CORS_ALLOWED_HEADER_SUGGESTIONS = ['authorization', 'content-type', 'x-ms-*', 'x-ms-meta*', 'x-ms-client-request-id'];
const STORAGE_CORS_EXPOSED_HEADER_SUGGESTIONS = ['content-type', 'etag', 'x-ms-*', 'x-ms-meta*', 'x-ms-request-id'];
const STORAGE_CORS_MAX_AGE_PRESETS = [300, 3600, 86400];

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

const ACR_SKU_OPTIONS = [
  { label: 'Basic', value: 'Basic' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

const ACR_PUBLIC_NETWORK_OPTIONS = [
  { label: 'Enabled', value: 'Enabled' },
  { label: 'Disabled', value: 'Disabled' },
];

const SQL_SERVER_VERSION_OPTIONS = [
  { label: '12.0', value: 'V12' },
];

const SQL_MIN_TLS_OPTIONS = [
  { label: 'TLS 1.0', value: '1.0' },
  { label: 'TLS 1.1', value: '1.1' },
  { label: 'TLS 1.2', value: '1.2' },
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

@Component({
  selector: 'app-resource-edit',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatOptionModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatTooltipModule,
    MatMenuModule,
    MatButtonToggleModule,
    MatExpansionModule,
    CompactSelectComponent,
    DeploymentConfigComponent,
    ToggleSectionCardComponent,
  ],
  templateUrl: './resource-edit.component.html',
  styleUrl: './resource-edit.component.scss',
})
export class ResourceEditComponent implements OnInit, OnDestroy {
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
  private readonly serviceBusNamespaceService = inject(ServiceBusNamespaceService);
  private readonly containerRegistryService = inject(ContainerRegistryService);
  private readonly sqlServerService = inject(SqlServerService);
  private readonly sqlDatabaseService = inject(SqlDatabaseService);
  private readonly userAssignedIdentityService = inject(UserAssignedIdentityService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);
  private readonly roleAssignmentService = inject(RoleAssignmentService);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly appSettingService = inject(AppSettingService);
  private readonly secureParamMappingService = inject(SecureParameterMappingService);
  private readonly configKeyService = inject(AppConfigurationKeyService);
  private readonly nameAvailabilityService = inject(NameAvailabilityService);
  private readonly customDomainService = inject(CustomDomainService);
  private readonly destroyRef = inject(DestroyRef);

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
  protected readonly formsDirty = signal(false);
  private formSubscriptions: Subscription[] = [];
  protected readonly saveSuccess = signal(false);

  // ─── Storage Services ───
  protected readonly storageSubTabIndex = signal(0);
  protected readonly storageActionLoading = signal(false);
  protected readonly storageActionError = signal('');
  protected readonly showBlobAddForm = signal(false);
  protected readonly showQueueAddForm = signal(false);
  protected readonly showTableAddForm = signal(false);
  protected readonly storageCorsRulesDraft = signal<CorsRuleEntry[]>([]);
  protected readonly storageTableCorsRulesDraft = signal<CorsRuleEntry[]>([]);
  protected readonly lifecycleRulesDraft = signal<BlobLifecycleRuleEntry[]>([]);
  protected readonly corsFieldErrors = signal<Record<string, string>>({});

  protected readonly isStorageAccount = computed(() => this.resourceType === 'StorageAccount');
  protected readonly isUserAssignedIdentity = computed(() => this.resourceType === 'UserAssignedIdentity');
  protected readonly isExistingResource = computed(() => (this.resource() as { isExisting?: boolean } | null)?.isExisting === true);

  // ─── App Pipeline ───

  protected supportsAppPipeline(): boolean {
    return this.resourceType === 'WebApp'
        || this.resourceType === 'FunctionApp'
        || this.resourceType === 'ContainerApp';
  }

  protected readonly redisAadWarning = computed(() => {
    if (this.resourceType !== 'RedisCache') return false;
    const form = this.generalForm;
    if (!form) return false;
    const disableKey = form.get('disableAccessKeyAuthentication')?.value;
    const aadEnabled = form.get('enableAadAuth')?.value;
    return disableKey === true && aadEnabled !== true;
  });

  protected readonly storageBlobContainers = computed<BlobContainerResponse[]>(() => {
    const res = this.resource();
    if (!res || this.resourceType !== 'StorageAccount') return [];
    return (res as StorageAccountResponse).blobContainers ?? [];
  });

  protected readonly storageQueues = computed<StorageQueueResponse[]>(() => {
    const res = this.resource();
    if (!res || this.resourceType !== 'StorageAccount') return [];
    return (res as StorageAccountResponse).queues ?? [];
  });

  protected readonly storageTables = computed<StorageTableResponse[]>(() => {
    const res = this.resource();
    if (!res || this.resourceType !== 'StorageAccount') return [];
    return (res as StorageAccountResponse).tables ?? [];
  });

  protected readonly storageCorsRules = computed<CorsRuleEntry[]>(() => this.storageCorsRulesDraft());
  protected readonly storageTableCorsRules = computed<CorsRuleEntry[]>(() => this.storageTableCorsRulesDraft());
  protected readonly lifecycleRules = computed<BlobLifecycleRuleEntry[]>(() => this.lifecycleRulesDraft());
  protected readonly corsMethodOptions = STORAGE_CORS_METHOD_OPTIONS;
  protected readonly corsAllowedHeaderSuggestions = STORAGE_CORS_ALLOWED_HEADER_SUGGESTIONS;
  protected readonly corsExposedHeaderSuggestions = STORAGE_CORS_EXPOSED_HEADER_SUGGESTIONS;
  protected readonly corsMaxAgePresets = STORAGE_CORS_MAX_AGE_PRESETS;

  // ─── Role Assignments ───
  protected readonly acrPullRoleId = ACR_PULL_ROLE_DEFINITION_ID;
  protected readonly roleAssignments = signal<RoleAssignmentResponse[]>([]);
  protected readonly roleAssignmentsLoading = signal(false);
  protected readonly roleAssignmentsError = signal('');
  protected readonly allResources = signal<AzureResourceResponse[]>([]);
  protected readonly availableRoleDefs = signal<AzureRoleDefinitionResponse[]>([]);

  protected readonly availableUserAssignedIdentities = computed(() =>
    this.allResources().filter(r => r.resourceType === 'UserAssignedIdentity')
  );

  protected readonly uaiOptionsForSelect = computed(() =>
    this.availableUserAssignedIdentities().map(uai => ({ value: uai.id, label: uai.name }))
  );

  /** The currently assigned UAI id and name, from the backend wrapper */
  protected readonly assignedIdentityId = signal<string | null>(null);
  protected readonly assignedIdentityName = signal<string | null>(null);

  protected readonly assignedUai = computed(() => {
    const id = this.assignedIdentityId();
    const name = this.assignedIdentityName();
    if (!id) return null;
    return { identityId: id, identityName: name ?? id };
  });

  /** UAIs available for switching (excludes the currently assigned one) */
  protected readonly switchableIdentities = computed(() => {
    const assigned = this.assignedUai();
    const all = this.availableUserAssignedIdentities();
    if (!assigned) return all;
    return all.filter(r => r.id !== assigned.identityId);
  });

  protected readonly availableContainerRegistries = computed(() =>
    this.allResources().filter(r => r.resourceType === 'ContainerRegistry')
  );

  protected readonly availableLogAnalyticsWorkspaces = computed(() =>
    this.allResources().filter(r => r.resourceType === 'LogAnalyticsWorkspace')
  );

  // ─── Deployment Mode (WebApp / FunctionApp) ───
  protected readonly deploymentMode = signal<'Code' | 'Container'>('Code');

  protected readonly isContainerMode = computed(() => this.deploymentMode() === 'Container');

  // ─── ACR Pull Access Check ───
  protected readonly acrAccessChecking = signal(false);
  protected readonly acrHasAccess = signal<boolean | null>(null);
  protected readonly acrMissingRoleName = signal<string | null>(null);
  protected readonly acrMissingRoleDefinitionId = signal<string | null>(null);
  protected readonly acrRoleAssigning = signal(false);

  protected readonly selectedContainerRegistryId = signal<string | null>(null);
  protected readonly acrAuthMode = signal<AcrAuthMode | null>(null);

  // UAI state for ACR flow
  protected readonly acrAssignedUaiId = signal<string | null>(null);
  protected readonly acrAssignedUaiName = signal<string | null>(null);
  protected readonly acrHasUai = signal(false);
  protected readonly acrSelectedUaiId = signal<string | null>(null);

  // ─── Name Availability (live Azure check) ───
  protected readonly nameAvailabilityChecking = signal(false);
  protected readonly nameAvailabilityResults = signal<EnvironmentNameAvailabilityResponseItem[]>([]);
  protected readonly nameAvailabilitySupported = signal<boolean | null>(null);

  /** Resource types where backend can run the Azure ARM name availability check. */
  private static readonly NAME_AVAILABILITY_TYPES = new Set([
    'ContainerRegistry',
    'StorageAccount',
    'KeyVault',
    'RedisCache',
    'AppConfiguration',
    'ServiceBusNamespace',
    'EventHubNamespace',
    'WebApp',
    'FunctionApp',
    'SqlServer',
  ]);

  /** True only for resource types where backend may run the Azure ARM check. */
  protected readonly isNameAvailabilityCheckEnabled = computed(
    () => ResourceEditComponent.NAME_AVAILABILITY_TYPES.has(this.resourceType) && !this.isExistingResource(),
  );

  /** Aggregated badge state derived from per-env results. */
  protected readonly nameAvailabilityOverallState = computed<'idle' | 'checking' | 'all-ok' | 'has-unavailable' | 'has-invalid' | 'all-current' | 'unknown'>(() => {
    if (this.nameAvailabilityChecking()) return 'checking';
    const items = this.nameAvailabilityResults();
    if (items.length === 0) return 'idle';
    if (items.some(i => i.status === 'invalid')) return 'has-invalid';
    if (items.some(i => i.status === 'unavailable')) return 'has-unavailable';
    if (items.every(i => i.status === 'available' || i.status === 'current')) {
      return items.every(i => i.status === 'current') ? 'all-current' : 'all-ok';
    }
    return 'unknown';
  });

  /** User explicitly confirmed that the unavailable name is theirs. Reset on every new check. */
  protected readonly nameAvailabilityOverridden = signal(false);

  /** True when name availability blocks saving (unavailable/invalid/checking and not overridden). */
  protected readonly isSaveBlockedByNameAvailability = computed(() => {
    if (!this.isNameAvailabilityCheckEnabled()) return false;
    const state = this.nameAvailabilityOverallState();
    if (state === 'has-unavailable' || state === 'has-invalid') {
      return !this.nameAvailabilityOverridden();
    }
    return state === 'checking';
  });

  /** UI state machine for ACR/UAI flow: idle | checking | ok | uai-missing-role | no-uai */
  protected readonly acrUaiState = computed(() => {
    if (this.acrAccessChecking()) return 'checking' as const;
    if (this.acrHasAccess() === true) return 'ok' as const;
    if (this.acrHasAccess() === null) return 'idle' as const;
    // hasAccess === false
    if (this.acrAssignedUaiId()) return 'uai-missing-role' as const;
    return 'no-uai' as const;
  });

  /** True when ACR integration is active: Container mode for WebApp/FunctionApp, or always for ContainerApp. */
  protected readonly isAcrEnabled = computed(() =>
    !!this.selectedContainerRegistryId() &&
    (this.isContainerMode() || this.resourceType === 'ContainerApp')
  );

  protected readonly isSaveBlockedByAcr = computed(() => {
    if (!this.isAcrEnabled() || this.acrAuthMode() !== 'ManagedIdentity') return false;
    return this.acrAccessChecking() || this.acrHasAccess() === false;
  });

  // ─── Tab Warning Badges ───
  protected readonly generalTabHasWarning = computed(() => this.isSaveBlockedByAcr() || this.isSaveBlockedByNameAvailability());

  protected readonly appSettingsTabHasWarning = computed(() => this.kvMissingRoleEntries().length > 0);

  protected readonly configKeysTabHasWarning = computed(() => this.configKeyKvMissingRoleEntries().length > 0);

  protected readonly environmentsTabHasWarning = computed(() => {
    const forms = this.envForms();
    if (forms.length === 0) return false;
    return forms.some(ef => {
      const controls = ef.form.controls;
      return Object.keys(controls).length > 0 && Object.values(controls).every(c => c.value === null || c.value === '');
    });
  });

  /** Role assignments grouped by identity: system-assigned flat + per-UAI collapsible groups */
  protected readonly groupedRoleAssignments = computed(() => {
    const all = this.roleAssignments();
    const systemAssigned: RoleAssignmentResponse[] = [];
    const uaiMap = new Map<string, { identityId: string; identityName: string; assignments: RoleAssignmentResponse[] }>();

    // Ensure the explicitly assigned UAI always appears as a group, even with 0 RAs
    const assigned = this.assignedUai();
    if (assigned) {
      uaiMap.set(assigned.identityId, {
        identityId: assigned.identityId,
        identityName: assigned.identityName,
        assignments: [],
      });
    }

    for (const ra of all) {
      if (ra.managedIdentityType === 'UserAssigned' && ra.userAssignedIdentityId) {
        const existing = uaiMap.get(ra.userAssignedIdentityId);
        if (existing) {
          existing.assignments.push(ra);
        } else {
          uaiMap.set(ra.userAssignedIdentityId, {
            identityId: ra.userAssignedIdentityId,
            identityName: this.resolveIdentityName(ra.userAssignedIdentityId),
            assignments: [ra],
          });
        }
      } else {
        systemAssigned.push(ra);
      }
    }
    return { systemAssigned, uaiGroups: [...uaiMap.values()] };
  });

  protected readonly expandedUaiIds = signal<Set<string>>(new Set());

  protected toggleUaiExpand(identityId: string): void {
    this.expandedUaiIds.update(set => {
      const next = new Set(set);
      if (next.has(identityId)) {
        next.delete(identityId);
      } else {
        next.add(identityId);
      }
      return next;
    });
  }

  protected isUaiExpanded(identityId: string): boolean {
    return this.expandedUaiIds().has(identityId);
  }

  // ─── Identity Granted Role Assignments (UAI only) ───
  protected readonly identityRoleAssignments = signal<IdentityRoleAssignmentResponse[]>([]);
  protected readonly identityRoleAssignmentsLoading = signal(false);
  protected readonly identityRoleAssignmentsError = signal('');

  /** Resources that use this UAI, grouped by source resource (excludes the UAI itself) */
  protected readonly usedByResources = computed(() => {
    const assignments = this.identityRoleAssignments();
    const grouped = new Map<string, { sourceResourceId: string; sourceResourceName: string; sourceResourceType: string; assignments: IdentityRoleAssignmentResponse[] }>();
    for (const ra of assignments) {
      // Skip assignments where the UAI is both source and identity (self-owned grants)
      if (ra.sourceResourceId === this.resourceId) continue;
      const existing = grouped.get(ra.sourceResourceId);
      if (existing) {
        existing.assignments.push(ra);
      } else {
        grouped.set(ra.sourceResourceId, {
          sourceResourceId: ra.sourceResourceId,
          sourceResourceName: ra.sourceResourceName,
          sourceResourceType: ra.sourceResourceType,
          assignments: [ra],
        });
      }
    }
    return [...grouped.values()];
  });

  // ─── App Settings ───
  protected readonly appSettings = signal<AppSettingResponse[]>([]);
  protected readonly appSettingsLoading = signal(false);
  protected readonly appSettingsError = signal('');
  protected readonly supportsAppSettings = computed(() =>
    ['WebApp', 'FunctionApp', 'ContainerApp'].includes(this.resourceType)
  );

  // ─── Custom Domains ───
  protected readonly customDomains = signal<CustomDomainResponse[]>([]);
  protected readonly customDomainsLoading = signal(false);
  protected readonly customDomainsError = signal('');
  protected readonly supportsCustomDomains = computed(() =>
    ['WebApp', 'FunctionApp', 'ContainerApp'].includes(this.resourceType)
  );
  protected customDomainsForEnv(envName: string): CustomDomainResponse[] {
    return this.customDomains().filter(d => d.environmentName === envName);
  }

  /** App settings grouped by category for sectioned display */
  protected readonly appSettingsGrouped = computed(() => {
    const all = this.appSettings();

    // 1. Variable Groups (including VG+KV secret combos) grouped by VG
    const byVg = new Map<string, { vgName: string; vgId: string; settings: AppSettingResponse[] }>();
    // 2. Output references
    const outputs: AppSettingResponse[] = [];
    // 3. Static values (including pure KV references without VG)
    const statics: AppSettingResponse[] = [];

    for (const s of all) {
      if (s.isViaVariableGroup && s.variableGroupId) {
        const vgId = s.variableGroupId;
        if (!byVg.has(vgId)) {
          byVg.set(vgId, { vgName: s.variableGroupName ?? vgId, vgId, settings: [] });
        }
        byVg.get(vgId)!.settings.push(s);
      } else if (s.isOutputReference) {
        outputs.push(s);
      } else {
        statics.push(s);
      }
    }

    return {
      byVg: [...byVg.values()],
      outputs,
      statics,
    };
  });

  protected readonly appSettingsSectionCount = computed(() => {
    const g = this.appSettingsGrouped();
    let n = 0;
    if (g.byVg.length > 0) n++;
    if (g.outputs.length > 0) n++;
    if (g.statics.length > 0) n++;
    return n;
  });

  // ─── Key Vault Access Check (App Settings) ───
  protected readonly kvMissingRoleEntries = signal<KvMissingRoleEntry[]>([]);
  protected readonly kvMissingRoleChecking = signal(false);

  // ─── Configuration Keys (AppConfiguration) ───
  protected readonly configKeys = signal<AppConfigurationKeyResponse[]>([]);
  protected readonly configKeysLoading = signal(false);
  protected readonly configKeysError = signal('');
  protected readonly supportsConfigKeys = computed(() => this.resourceType === 'AppConfiguration');
  protected readonly configKeyKvMissingRoleEntries = signal<KvMissingRoleEntry[]>([]);
  protected readonly configKeyKvMissingRoleChecking = signal(false);

  protected readonly configKeysGrouped = computed(() => {
    const all = this.configKeys();
    const byVg = new Map<string, { vgName: string; vgId: string; keys: AppConfigurationKeyResponse[] }>();
    const outputs: AppConfigurationKeyResponse[] = [];
    const statics: AppConfigurationKeyResponse[] = [];

    for (const k of all) {
      if (k.isViaVariableGroup && k.variableGroupId) {
        const vgId = k.variableGroupId;
        if (!byVg.has(vgId)) {
          byVg.set(vgId, { vgName: k.variableGroupName ?? vgId, vgId, keys: [] });
        }
        byVg.get(vgId)!.keys.push(k);
      } else if (k.isOutputReference) {
        outputs.push(k);
      } else {
        statics.push(k);
      }
    }

    return { byVg: [...byVg.values()], outputs, statics };
  });

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
  protected readonly acrSkuOptions = ACR_SKU_OPTIONS;
  protected readonly acrPublicNetworkOptions = ACR_PUBLIC_NETWORK_OPTIONS;
  protected readonly sqlServerVersionOptions = SQL_SERVER_VERSION_OPTIONS;
  protected readonly sqlMinTlsOptions = SQL_MIN_TLS_OPTIONS;
  protected readonly runtimeVersionOptions = signal<string[]>([]);

  // ─── Secure Parameter Mappings (SqlServer password config) ───
  protected readonly secureParamMappings = signal<SecureParameterMappingResponse[]>([]);
  protected readonly passwordMode = signal<'random' | 'variableGroup'>('random');
  protected readonly passwordVgOptions = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly passwordVgLoading = signal(false);
  protected readonly passwordSelectedVgId = signal<string | null>(null);
  protected readonly passwordNewGroupName = signal('');
  protected readonly passwordIsCreatingNewGroup = signal(false);
  protected readonly passwordNewGroupScope = signal<'project' | 'configuration'>('project');
  protected readonly passwordPipelineVariableName = signal('');
  protected readonly passwordSaving = signal(false);
  protected readonly passwordSaveSuccess = signal(false);

  /** Snapshot of the last saved (or loaded) state, used to detect unsaved changes. */
  private readonly passwordSavedState = signal<{ mode: 'random' | 'variableGroup'; variableGroupId: string | null; pipelineVariableName: string | null }>({
    mode: 'random', variableGroupId: null, pipelineVariableName: null,
  });

  protected readonly hasPasswordConfigChanged = computed(() => {
    const saved = this.passwordSavedState();
    if (this.passwordMode() !== saved.mode) return true;
    if (this.passwordMode() === 'variableGroup') {
      if (this.passwordIsCreatingNewGroup()) return true;
      if (this.passwordSelectedVgId() !== saved.variableGroupId) return true;
      if (this.passwordPipelineVariableName() !== (saved.pipelineVariableName ?? '')) return true;
    }
    return false;
  });

  protected readonly canSavePasswordConfig = computed(() => {
    if (this.passwordSaving()) return false;
    if (this.passwordMode() === 'random') return true;
    const hasVg = this.passwordIsCreatingNewGroup()
      ? this.passwordNewGroupName().trim().length > 0
      : !!this.passwordSelectedVgId();
    return hasVg && this.passwordPipelineVariableName().trim().length > 0;
  });

  // ─── Forms ───
  protected generalForm!: FormGroup;
  protected envForms = signal<{ envName: string; form: FormGroup }[]>([]);

  // ─── Environments ───
  protected readonly environments = computed<EnvironmentDefinitionResponse[]>(() => {
    const proj = this.project();
    if (!proj) return [];
    return [...(proj.environmentDefinitions ?? [])].sort((a, b) => a.order - b.order);
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

  // ─── Main tab index (for programmatic selection) ───
  protected readonly mainTabIndex = signal(0);

  // ─── Save bar visibility (only on General/Environments tabs when dirty) ───
  protected readonly showSaveBar = computed(() => {
    const tabIndex = this.mainTabIndex();
    const isOnSaveableTab = this.isUserAssignedIdentity()
      ? tabIndex === 0
      : tabIndex === 0 || tabIndex === 1 || (this.isStorageAccount() && tabIndex === 3) || (this.supportsAppPipeline() && tabIndex === 4);
    return this.formsDirty() && isOnSaveableTab && this.canWrite();
  });

  async ngOnInit(): Promise<void> {
    this.configId = this.route.snapshot.paramMap.get('configId') ?? '';
    this.resourceType = this.route.snapshot.paramMap.get('resourceType') ?? '';
    this.resourceId = this.route.snapshot.paramMap.get('resourceId') ?? '';

    if (!this.configId || !this.resourceType || !this.resourceId) {
      this.loadError.set('RESOURCE_EDIT.ERROR.MISSING_PARAMS');
      return;
    }

    // Handle storage sub-tab query param
    const tab = this.route.snapshot.queryParamMap.get('tab');
    if (tab && this.resourceType === 'StorageAccount') {
      this.mainTabIndex.set(3); // Storage Services tab
      switch (tab) {
        case 'blob_containers': this.storageSubTabIndex.set(0); break;
        case 'queues': this.storageSubTabIndex.set(1); break;
        case 'tables': this.storageSubTabIndex.set(2); break;
      }
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
      this.watchFormChanges();

      // Check ACR pull access: container mode for WebApp/FunctionApp, always for ContainerApp
      if (this.isAcrEnabled()) {
        this.checkAcrPullAccess();
      }

      // Load role assignments and sibling resources in background (non-blocking)
      this.loadRoleAssignments();
      this.loadAllResources();
      if (this.supportsAppSettings()) {
        this.loadAppSettings();
      }
      if (this.supportsCustomDomains()) {
        this.loadCustomDomains();
      }
      if (this.supportsConfigKeys()) {
        this.loadConfigKeys();
      }
      if (this.isUserAssignedIdentity()) {
        this.loadIdentityRoleAssignments();
      }
      if (this.resourceType === 'SqlServer') {
        this.loadSecureParamMappings();
      }
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
      case 'ServiceBusNamespace':
        return this.serviceBusNamespaceService.getById(this.resourceId);
      case 'ContainerRegistry':
        return this.containerRegistryService.getById(this.resourceId);
      case 'SqlServer':
        return this.sqlServerService.getById(this.resourceId);
      case 'SqlDatabase':
        return this.sqlDatabaseService.getById(this.resourceId);
      default:
        throw new Error(`Unknown resource type: ${this.resourceType}`);
    }
  }

  private buildGeneralForm(resource: ResourceData): void {
    const base: Record<string, unknown[]> = {
      name: [resource.name, [Validators.required, Validators.maxLength(80)]],
      location: [resource.location, [Validators.required]],
    };

    this.acrAccessChecking.set(false);
    this.resetAcrPullAccessState();
    this.selectedContainerRegistryId.set(null);
    this.acrAuthMode.set(null);

    if (this.resourceType === 'KeyVault') {
      const kv = resource as KeyVaultResponse;
      base['enableRbacAuthorization'] = [kv.enableRbacAuthorization];
      base['enabledForDeployment'] = [kv.enabledForDeployment];
      base['enabledForDiskEncryption'] = [kv.enabledForDiskEncryption];
      base['enabledForTemplateDeployment'] = [kv.enabledForTemplateDeployment];
      base['enablePurgeProtection'] = [kv.enablePurgeProtection];
      base['enableSoftDelete'] = [kv.enableSoftDelete];
    } else if (this.resourceType === 'AppServicePlan') {
      const asp = resource as AppServicePlanResponse;
      base['osType'] = [asp.osType, [Validators.required]];
    } else if (this.resourceType === 'WebApp') {
      const wa = resource as WebAppResponse;
      const resolvedAcrAuthMode = this.resolveAcrAuthMode(wa.containerRegistryId ?? null, wa.acrAuthMode ?? null);
      base['appServicePlanId'] = [wa.appServicePlanId];
      base['deploymentMode'] = [wa.deploymentMode || 'Code'];
      base['containerRegistryId'] = [wa.containerRegistryId ?? null];
      base['acrAuthMode'] = [resolvedAcrAuthMode];
      base['dockerImageName'] = [wa.dockerImageName ?? null];
      base['runtimeStack'] = [wa.runtimeStack, [Validators.required]];
      base['runtimeVersion'] = [wa.runtimeVersion];
      base['alwaysOn'] = [wa.alwaysOn];
      base['httpsOnly'] = [wa.httpsOnly];
      base['dockerfilePath'] = [wa.dockerfilePath ?? ''];
      base['sourceCodePath'] = [wa.sourceCodePath ?? ''];
      base['buildCommand'] = [wa.buildCommand ?? ''];
      base['applicationName'] = [wa.applicationName ?? ''];
      this.deploymentMode.set((wa.deploymentMode as 'Code' | 'Container') || 'Code');
      this.selectedContainerRegistryId.set(wa.containerRegistryId ?? null);
      this.acrAuthMode.set(resolvedAcrAuthMode);
    } else if (this.resourceType === 'FunctionApp') {
      const fa = resource as FunctionAppResponse;
      const resolvedAcrAuthMode = this.resolveAcrAuthMode(fa.containerRegistryId ?? null, fa.acrAuthMode ?? null);
      base['appServicePlanId'] = [fa.appServicePlanId];
      base['deploymentMode'] = [fa.deploymentMode || 'Code'];
      base['containerRegistryId'] = [fa.containerRegistryId ?? null];
      base['acrAuthMode'] = [resolvedAcrAuthMode];
      base['dockerImageName'] = [fa.dockerImageName ?? null];
      base['runtimeStack'] = [fa.runtimeStack, [Validators.required]];
      base['runtimeVersion'] = [fa.runtimeVersion];
      base['httpsOnly'] = [fa.httpsOnly];
      base['dockerfilePath'] = [fa.dockerfilePath ?? ''];
      base['sourceCodePath'] = [fa.sourceCodePath ?? ''];
      base['buildCommand'] = [fa.buildCommand ?? ''];
      base['applicationName'] = [fa.applicationName ?? ''];
      this.deploymentMode.set((fa.deploymentMode as 'Code' | 'Container') || 'Code');
      this.selectedContainerRegistryId.set(fa.containerRegistryId ?? null);
      this.acrAuthMode.set(resolvedAcrAuthMode);
    } else if (this.resourceType === 'StorageAccount') {
      const sa = resource as StorageAccountResponse;
      base['kind'] = [sa.kind, [Validators.required]];
      base['accessTier'] = [sa.accessTier, [Validators.required]];
      base['allowBlobPublicAccess'] = [sa.allowBlobPublicAccess];
      base['enableHttpsTrafficOnly'] = [sa.enableHttpsTrafficOnly];
      base['minimumTlsVersion'] = [sa.minimumTlsVersion, [Validators.required]];
      this.storageCorsRulesDraft.set((sa.corsRules ?? []).map(rule => ({
        allowedOrigins: [...rule.allowedOrigins],
        allowedMethods: [...rule.allowedMethods],
        allowedHeaders: [...rule.allowedHeaders],
        exposedHeaders: [...rule.exposedHeaders],
        maxAgeInSeconds: rule.maxAgeInSeconds,
      })));
      this.storageTableCorsRulesDraft.set((sa.tableCorsRules ?? []).map(rule => ({
        allowedOrigins: [...rule.allowedOrigins],
        allowedMethods: [...rule.allowedMethods],
        allowedHeaders: [...rule.allowedHeaders],
        exposedHeaders: [...rule.exposedHeaders],
        maxAgeInSeconds: rule.maxAgeInSeconds,
      })));
      this.lifecycleRulesDraft.set((sa.lifecycleRules ?? []).map(rule => ({
        ruleName: rule.ruleName,
        containerNames: [...rule.containerNames],
        timeToLiveInDays: rule.timeToLiveInDays,
      })));
    } else if (this.resourceType === 'ContainerApp') {
      const ca = resource as ContainerAppResponse;
      const resolvedAcrAuthMode = this.resolveAcrAuthMode(ca.containerRegistryId ?? null, ca.acrAuthMode ?? null);
      base['containerAppEnvironmentId'] = [ca.containerAppEnvironmentId];
      base['containerRegistryId'] = [ca.containerRegistryId ?? null];
      base['acrAuthMode'] = [resolvedAcrAuthMode];
      base['dockerImageName'] = [ca.dockerImageName ?? null];
      base['dockerfilePath'] = [ca.dockerfilePath ?? ''];
      base['applicationName'] = [ca.applicationName ?? ''];
      this.selectedContainerRegistryId.set(ca.containerRegistryId ?? null);
      this.acrAuthMode.set(resolvedAcrAuthMode);
    } else if (this.resourceType === 'ContainerAppEnvironment') {
      const cae = resource as ContainerAppEnvironmentResponse;
      base['logAnalyticsWorkspaceId'] = [cae.logAnalyticsWorkspaceId ?? null];
    } else if (this.resourceType === 'ApplicationInsights') {
      const ai = resource as ApplicationInsightsResponse;
      base['logAnalyticsWorkspaceId'] = [ai.logAnalyticsWorkspaceId];
    } else if (this.resourceType === 'RedisCache') {
      const rc = resource as RedisCacheResponse;
      base['redisVersion'] = [rc.redisVersion, [Validators.required]];
      base['enableNonSslPort'] = [rc.enableNonSslPort];
      base['minimumTlsVersion'] = [rc.minimumTlsVersion, [Validators.required]];
      base['disableAccessKeyAuthentication'] = [rc.disableAccessKeyAuthentication];
      base['enableAadAuth'] = [rc.enableAadAuth];
    } else if (this.resourceType === 'SqlServer') {
      const sql = resource as SqlServerResponse;
      base['version'] = [sql.version, [Validators.required]];
      base['administratorLogin'] = [sql.administratorLogin, [Validators.required]];
    } else if (this.resourceType === 'SqlDatabase') {
      const db = resource as SqlDatabaseResponse;
      base['sqlServerId'] = [db.sqlServerId, [Validators.required]];
      base['collation'] = [db.collation];
    }

    this.generalForm = this.fb.group(base);

    // Initialize runtime version options for WebApp/FunctionApp
    if (this.resourceType === 'WebApp') {
      const stack = this.generalForm.get('runtimeStack')?.value;
      this.runtimeVersionOptions.set(WEBAPP_RUNTIME_VERSION_MAP[stack] ?? []);
    } else if (this.resourceType === 'FunctionApp') {
      const stack = this.generalForm.get('runtimeStack')?.value;
      this.runtimeVersionOptions.set(FUNCTIONAPP_RUNTIME_VERSION_MAP[stack] ?? []);
    }
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

  protected onProbeToggle(envIndex: number, probeType: 'readiness' | 'liveness' | 'startup', enabled: boolean): void {
    const envForm = this.envForms()[envIndex]?.form;
    if (!envForm) return;
    const defaults: Record<string, { path: string; port: number }> = {
      readiness: { path: '/healthz/ready', port: 8080 },
      liveness: { path: '/healthz/live', port: 8080 },
      startup: { path: '/healthz/startup', port: 8080 },
    };

    envForm.patchValue({
      [`${probeType}ProbeEnabled`]: enabled,
      [`${probeType}ProbePath`]: enabled ? defaults[probeType].path : null,
      [`${probeType}ProbePort`]: enabled ? defaults[probeType].port : null,
    });
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
          maxMemoryPolicy: [settings?.maxMemoryPolicy ?? null],
        });
      }
      case 'StorageAccount': {
        const sa = resource as StorageAccountResponse;
        const settings = sa.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
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
          dockerImageTag: [settings?.dockerImageTag ?? null],
        });
      }
      case 'FunctionApp': {
        const fa = resource as FunctionAppResponse;
        const settings = fa.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          httpsOnly: [settings?.httpsOnly ?? null],
          maxInstanceCount: [settings?.maxInstanceCount ?? null],
          dockerImageTag: [settings?.dockerImageTag ?? null],
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
        });
      }
      case 'ContainerApp': {
        const ca = resource as ContainerAppResponse;
        const settings = ca.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          cpuCores: [settings?.cpuCores ?? null],
          memoryGi: [settings?.memoryGi ?? null],
          minReplicas: [settings?.minReplicas ?? null],
          maxReplicas: [settings?.maxReplicas ?? null],
          ingressEnabled: [settings?.ingressEnabled ?? null],
          ingressTargetPort: [settings?.ingressTargetPort ?? null],
          ingressExternal: [settings?.ingressExternal ?? null],
          transportMethod: [settings?.transportMethod ?? null],
          readinessProbeEnabled: [!!(settings?.readinessProbePath)],
          readinessProbePath: [settings?.readinessProbePath ?? null],
          readinessProbePort: [settings?.readinessProbePort ?? null],
          livenessProbeEnabled: [!!(settings?.livenessProbePath)],
          livenessProbePath: [settings?.livenessProbePath ?? null],
          livenessProbePort: [settings?.livenessProbePort ?? null],
          startupProbeEnabled: [!!(settings?.startupProbePath)],
          startupProbePath: [settings?.startupProbePath ?? null],
          startupProbePort: [settings?.startupProbePort ?? null],
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
      case 'ServiceBusNamespace': {
        const sb = resource as ServiceBusNamespaceResponse;
        const settings = sb.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          capacity: [settings?.capacity ?? null],
          zoneRedundant: [settings?.zoneRedundant ?? null],
          disableLocalAuth: [settings?.disableLocalAuth ?? null],
          minimumTlsVersion: [settings?.minimumTlsVersion ?? null],
        });
      }
      case 'ContainerRegistry': {
        const cr = resource as ContainerRegistryResponse;
        const settings = cr.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          adminUserEnabled: [settings?.adminUserEnabled ?? null],
          publicNetworkAccess: [settings?.publicNetworkAccess ?? null],
          zoneRedundancy: [settings?.zoneRedundancy ?? null],
        });
      }
      case 'SqlServer': {
        const sql = resource as SqlServerResponse;
        const settings = sql.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          minimalTlsVersion: [settings?.minimalTlsVersion ?? null],
        });
      }
      case 'SqlDatabase': {
        const db = resource as SqlDatabaseResponse;
        const settings = db.environmentSettings?.find(s => s.environmentName === envName);
        return this.fb.group({
          sku: [settings?.sku ?? null],
          maxSizeGb: [settings?.maxSizeGb ?? null],
          zoneRedundant: [settings?.zoneRedundant ?? null],
        });
      }
    }
  }

  /** User confirms the unavailable name is theirs and overrides the save block. */
  protected overrideNameAvailability(): void {
    this.nameAvailabilityOverridden.set(true);
  }

  private resolveAcrAuthMode(containerRegistryId: string | null | undefined, acrAuthMode: AcrAuthMode | null | undefined): AcrAuthMode | null {
    if (!containerRegistryId) {
      return null;
    }

    return acrAuthMode ?? 'ManagedIdentity';
  }

  private setAcrAuthMode(acrAuthMode: AcrAuthMode | null, emitEvent = false): void {
    this.acrAuthMode.set(acrAuthMode);

    const acrAuthModeControl = this.generalForm.get('acrAuthMode');
    if (acrAuthModeControl) {
      acrAuthModeControl.setValue(acrAuthMode, { emitEvent });
    }
  }

  private resetAcrPullAccessState(): void {
    this.acrHasAccess.set(null);
    this.acrMissingRoleName.set(null);
    this.acrMissingRoleDefinitionId.set(null);
    this.acrAssignedUaiId.set(null);
    this.acrAssignedUaiName.set(null);
    this.acrHasUai.set(false);
    this.acrSelectedUaiId.set(null);
  }

  protected async onSave(): Promise<void> {
    if (this.generalForm.invalid || this.isSaving() || this.isSaveBlockedByAcr() || this.isSaveBlockedByNameAvailability()) return;

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
            enableRbacAuthorization: general.enableRbacAuthorization,
            enabledForDeployment: general.enabledForDeployment,
            enabledForDiskEncryption: general.enabledForDiskEncryption,
            enabledForTemplateDeployment: general.enabledForTemplateDeployment,
            enablePurgeProtection: general.enablePurgeProtection,
            enableSoftDelete: general.enableSoftDelete,
            environmentSettings: this.buildKeyVaultEnvSettings(),
          });
          break;
        case 'RedisCache':
          await this.redisCacheService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            redisVersion: general.redisVersion != null ? Number(general.redisVersion) : null,
            enableNonSslPort: general.enableNonSslPort ?? false,
            minimumTlsVersion: general.minimumTlsVersion || null,
            disableAccessKeyAuthentication: general.disableAccessKeyAuthentication ?? false,
            enableAadAuth: general.enableAadAuth ?? false,
            environmentSettings: this.buildRedisCacheEnvSettings(),
          });
          break;
        case 'StorageAccount':
          if (!this.validateAllStorageCorsRules()) {
            this.saveError.set('RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.FIX_VALIDATION_ERRORS');
            this.isSaving.set(false);
            return;
          }

          await this.storageAccountService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            kind: general.kind,
            accessTier: general.accessTier,
            allowBlobPublicAccess: general.allowBlobPublicAccess ?? false,
            enableHttpsTrafficOnly: general.enableHttpsTrafficOnly ?? true,
            minimumTlsVersion: general.minimumTlsVersion,
            corsRules: this.buildStorageAccountCorsRules(),
            tableCorsRules: this.buildStorageAccountTableCorsRules(),
            lifecycleRules: this.buildLifecycleRules(),
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
        case 'WebApp': {
          const acrAuthMode = general.deploymentMode === 'Container'
            ? this.resolveAcrAuthMode(general.containerRegistryId || null, general.acrAuthMode as AcrAuthMode | null | undefined)
            : null;

          await this.webAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            appServicePlanId: general.appServicePlanId,
            deploymentMode: general.deploymentMode || 'Code',
            containerRegistryId: general.deploymentMode === 'Container' ? (general.containerRegistryId || null) : null,
            acrAuthMode,
            dockerImageName: general.deploymentMode === 'Container' ? (general.dockerImageName || null) : null,
            dockerfilePath: general.dockerfilePath || null,
            sourceCodePath: general.sourceCodePath || null,
            buildCommand: general.buildCommand || null,
            applicationName: general.applicationName || null,
            runtimeStack: general.runtimeStack,
            runtimeVersion: general.runtimeVersion,
            alwaysOn: general.alwaysOn,
            httpsOnly: general.httpsOnly,
            environmentSettings: this.buildWebAppEnvSettings(),
          });
          break;
        }
        case 'FunctionApp': {
          const acrAuthMode = general.deploymentMode === 'Container'
            ? this.resolveAcrAuthMode(general.containerRegistryId || null, general.acrAuthMode as AcrAuthMode | null | undefined)
            : null;

          await this.functionAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            appServicePlanId: general.appServicePlanId,
            deploymentMode: general.deploymentMode || 'Code',
            containerRegistryId: general.deploymentMode === 'Container' ? (general.containerRegistryId || null) : null,
            acrAuthMode,
            dockerImageName: general.deploymentMode === 'Container' ? (general.dockerImageName || null) : null,
            dockerfilePath: general.dockerfilePath || null,
            sourceCodePath: general.sourceCodePath || null,
            buildCommand: general.buildCommand || null,
            applicationName: general.applicationName || null,
            runtimeStack: general.runtimeStack,
            runtimeVersion: general.runtimeVersion,
            httpsOnly: general.httpsOnly,
            environmentSettings: this.buildFunctionAppEnvSettings(),
          });
          break;
        }
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
            logAnalyticsWorkspaceId: general.logAnalyticsWorkspaceId || null,
            environmentSettings: this.buildContainerAppEnvironmentEnvSettings(),
          });
          break;
        case 'ContainerApp': {
          const acrAuthMode = this.resolveAcrAuthMode(general.containerRegistryId || null, general.acrAuthMode as AcrAuthMode | null | undefined);

          await this.containerAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            containerAppEnvironmentId: general.containerAppEnvironmentId,
            containerRegistryId: general.containerRegistryId || null,
            acrAuthMode,
            dockerImageName: general.dockerImageName || null,
            dockerfilePath: general.dockerfilePath || null,
            applicationName: general.applicationName || null,
            environmentSettings: this.buildContainerAppEnvSettings(),
          });
          break;
        }
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
        case 'ServiceBusNamespace':
          await this.serviceBusNamespaceService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildServiceBusNamespaceEnvSettings(),
          });
          break;
        case 'ContainerRegistry':
          await this.containerRegistryService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: this.buildContainerRegistryEnvSettings(),
          });
          break;
        case 'SqlServer':
          await this.sqlServerService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            version: general.version,
            administratorLogin: general.administratorLogin,
            environmentSettings: this.buildSqlServerEnvSettings(),
          });
          break;
        case 'SqlDatabase':
          await this.sqlDatabaseService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            sqlServerId: general.sqlServerId,
            collation: general.collation ?? 'SQL_Latin1_General_CP1_CI_AS',
            environmentSettings: this.buildSqlDatabaseEnvSettings(),
          });
          break;
      }

      // Reload to reflect saved state
      const updated = await this.loadResource();
      this.resource.set(updated);
      this.buildGeneralForm(updated);
      this.buildEnvForms(updated);
      this.watchFormChanges();
      this.saveSuccess.set(true);
      setTimeout(() => this.saveSuccess.set(false), 4000);
    } catch {
      this.saveError.set('RESOURCE_EDIT.ERROR.SAVE_FAILED');
    } finally {
      this.isSaving.set(false);
    }
  }

  private watchFormChanges(): void {
    this.formSubscriptions.forEach(s => s.unsubscribe());
    this.formSubscriptions = [];
    this.formsDirty.set(false);

    this.formSubscriptions.push(
      this.generalForm.valueChanges.subscribe(() => this.formsDirty.set(true))
    );
    for (const ef of this.envForms()) {
      this.formSubscriptions.push(
        ef.form.valueChanges.subscribe(() => this.formsDirty.set(true))
      );
    }

    this.wireNameAvailabilityCheck();
  }

  private wireNameAvailabilityCheck(): void {
    if (!this.isNameAvailabilityCheckEnabled()) return;

    const ctrl = this.generalForm.get('name');
    if (!ctrl) return;

    // Reset previous state on rebuild
    this.nameAvailabilityChecking.set(false);
    this.nameAvailabilityResults.set([]);
    this.nameAvailabilitySupported.set(null);

    const sub = ctrl.valueChanges
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
          const projectId = this.config()?.projectId;
          if (!projectId) return EMPTY;
          return this.nameAvailabilityService
            .check$(this.resourceType, {
              projectId,
              configId: this.configId,
              name: name.trim(),
              currentPersistedName: this.resource()?.name ?? null,
            })
            .pipe(
              catchError(() => {
                this.nameAvailabilityChecking.set(false);
                this.nameAvailabilityResults.set([]);
                this.nameAvailabilitySupported.set(null);
                return EMPTY;
              }),
            );
        }),
      )
      .subscribe(res => {
        this.nameAvailabilityChecking.set(false);
        this.nameAvailabilityResults.set(res.environments ?? []);
        this.nameAvailabilitySupported.set(res.supported);
      });

    this.formSubscriptions.push(sub);
  }

  protected discardChanges(): void {
    const res = this.resource();
    if (!res) return;
    this.buildGeneralForm(res);
    this.buildEnvForms(res);
    this.watchFormChanges();
    // buildGeneralForm sets selectedContainerRegistryId for all applicable types (WebApp, FunctionApp, ContainerApp)
    if (this.isAcrEnabled()) {
      this.checkAcrPullAccess();
    } else {
      this.acrHasAccess.set(null);
    }
  }

  protected async onDeploymentModeChange(mode: 'Code' | 'Container'): Promise<void> {
    this.deploymentMode.set(mode);
    this.generalForm.patchValue({ deploymentMode: mode });
    const currentRegistryId = this.selectedContainerRegistryId() ?? this.generalForm.get('containerRegistryId')?.value ?? null;
    const nextAcrAuthMode = mode === 'Container'
      ? this.resolveAcrAuthMode(currentRegistryId, this.acrAuthMode() ?? (this.generalForm.get('acrAuthMode')?.value as AcrAuthMode | null | undefined))
      : null;

    this.acrAccessChecking.set(false);
    this.resetAcrPullAccessState();
    this.setAcrAuthMode(nextAcrAuthMode, true);
    this.formsDirty.set(true);

    if (mode === 'Container' && currentRegistryId && nextAcrAuthMode === 'ManagedIdentity') {
      await this.checkAcrPullAccess(currentRegistryId, nextAcrAuthMode);
    }
  }

  protected onRuntimeStackChange(stack: string): void {
    const map = this.resourceType === 'FunctionApp'
      ? FUNCTIONAPP_RUNTIME_VERSION_MAP
      : WEBAPP_RUNTIME_VERSION_MAP;
    const versions = map[stack] ?? [];
    this.runtimeVersionOptions.set(versions);
    const currentVersion = this.generalForm.get('runtimeVersion')?.value;
    if (currentVersion && !versions.includes(currentVersion)) {
      this.generalForm.patchValue({ runtimeVersion: versions[0] ?? '' });
    }
  }

  protected async onContainerRegistryChange(acrId: string | null): Promise<void> {
    const nextAcrAuthMode = this.resolveAcrAuthMode(acrId, this.acrAuthMode() ?? (this.generalForm.get('acrAuthMode')?.value as AcrAuthMode | null | undefined));

    this.selectedContainerRegistryId.set(acrId);
    this.generalForm.patchValue({ containerRegistryId: acrId });
    this.acrAccessChecking.set(false);
    this.resetAcrPullAccessState();
    this.setAcrAuthMode(nextAcrAuthMode, true);
    if (!acrId || nextAcrAuthMode !== 'ManagedIdentity') return;
    await this.checkAcrPullAccess(acrId, nextAcrAuthMode);
  }

  protected async onAcrAuthModeChange(mode: AcrAuthMode): Promise<void> {
    const currentRegistryId = this.selectedContainerRegistryId() ?? this.generalForm.get('containerRegistryId')?.value ?? null;
    const nextAcrAuthMode = this.resolveAcrAuthMode(currentRegistryId, mode);

    this.acrAccessChecking.set(false);
    this.resetAcrPullAccessState();
    this.setAcrAuthMode(nextAcrAuthMode, true);
    if (!currentRegistryId || nextAcrAuthMode !== 'ManagedIdentity') return;
    await this.checkAcrPullAccess(currentRegistryId, nextAcrAuthMode);
  }

  protected async checkAcrPullAccess(acrId?: string, acrAuthMode?: AcrAuthMode | null): Promise<void> {
    const registryId = acrId ?? this.generalForm.get('containerRegistryId')?.value;
    const requestedAcrAuthMode = this.resolveAcrAuthMode(registryId, acrAuthMode ?? this.acrAuthMode());
    if (!registryId || requestedAcrAuthMode !== 'ManagedIdentity') {
      this.resetAcrPullAccessState();
      return;
    }

    this.acrAuthMode.set(requestedAcrAuthMode);
    this.acrAccessChecking.set(true);
    this.resetAcrPullAccessState();

    try {
      const result = await this.containerRegistryService.checkAcrPullAccess(this.resourceId, registryId, requestedAcrAuthMode);
      if (this.selectedContainerRegistryId() !== registryId || this.acrAuthMode() !== requestedAcrAuthMode) {
        return;
      }

      this.acrHasAccess.set(result.hasAccess);
      this.acrMissingRoleName.set(result.missingRoleName ?? null);
      this.acrMissingRoleDefinitionId.set(result.missingRoleDefinitionId ?? null);
      this.acrAssignedUaiId.set(result.assignedUserAssignedIdentityId ?? null);
      this.acrAssignedUaiName.set(result.assignedUserAssignedIdentityName ?? null);
      this.acrHasUai.set(result.hasUserAssignedIdentity);
      this.acrAuthMode.set(this.resolveAcrAuthMode(registryId, result.acrAuthMode ?? requestedAcrAuthMode));
      // Pre-select the assigned UAI if one exists
      if (result.assignedUserAssignedIdentityId) {
        this.acrSelectedUaiId.set(result.assignedUserAssignedIdentityId);
      }
    } catch {
      this.acrHasAccess.set(null);
    } finally {
      this.acrAccessChecking.set(false);
    }
  }

  protected async addAcrPullRoleAssignment(): Promise<void> {
    const acrId = this.generalForm.get('containerRegistryId')?.value;
    const roleDefId = this.acrMissingRoleDefinitionId();
    const uaiId = this.acrSelectedUaiId() ?? this.acrAssignedUaiId();
    if (!acrId || !roleDefId || !uaiId) return;

    this.acrRoleAssigning.set(true);
    try {
      await this.roleAssignmentService.add(this.resourceId, {
        targetResourceId: acrId,
        managedIdentityType: 'UserAssigned',
        roleDefinitionId: roleDefId,
        userAssignedIdentityId: uaiId,
      });
      await this.checkAcrPullAccess(acrId);
      await this.loadRoleAssignments();
    } catch {
      // Error handled silently; the UI will still show the missing role
    } finally {
      this.acrRoleAssigning.set(false);
    }
  }

  protected createNewUai(): void {
    const res = this.resource();
    if (!res) return;
    const resourceGroupId = (res as { resourceGroupId?: string })?.resourceGroupId ?? '';
    const location = res.location ?? 'EastUS2';

    const dialogRef = this.dialog.open(CreateUaiDialogComponent, {
      data: { resourceGroupId, location } as CreateUaiDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (result: UserAssignedIdentityResponse | undefined) => {
      if (!result) return;
      await this.loadAllResources();
      this.acrSelectedUaiId.set(result.id);
    });
  }

  ngOnDestroy(): void {
    this.formSubscriptions.forEach(s => s.unsubscribe());
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
        case 'ServiceBusNamespace':
          await this.serviceBusNamespaceService.delete(this.resourceId);
          break;
        case 'ContainerRegistry':
          await this.containerRegistryService.delete(this.resourceId);
          break;
        case 'SqlServer':
          await this.sqlServerService.delete(this.resourceId);
          break;
        case 'SqlDatabase':
          await this.sqlDatabaseService.delete(this.resourceId);
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
      const result = await this.roleAssignmentService.getByResourceId(this.resourceId);
      this.roleAssignments.set(result.roleAssignments);
      this.assignedIdentityId.set(result.assignedUserAssignedIdentityId);
      this.assignedIdentityName.set(result.assignedUserAssignedIdentityName);

      const assignedUserAssignedIdentityId = result.assignedUserAssignedIdentityId;
      if (assignedUserAssignedIdentityId) {
        this.expandedUaiIds.update(current => {
          if (current.has(assignedUserAssignedIdentityId)) {
            return current;
          }

          const next = new Set(current);
          next.add(assignedUserAssignedIdentityId);
          return next;
        });
      }

      // Load role definitions for all unique target resource ids to resolve role names
      const targetIds = [...new Set(result.roleAssignments.map(a => a.targetResourceId))];
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
      const localResources = results.flat().filter(r => r.id !== this.resourceId);

      // Also load cross-config resources from the same project
      const projectId = this.config()?.projectId;
      if (projectId) {
        try {
          const projectResources = await this.projectService.getProjectResources(projectId);
          const localIds = new Set(localResources.map(r => r.id));
          const crossConfig = projectResources
            .filter(pr => pr.configId !== this.configId && !localIds.has(pr.resourceId) && pr.resourceId !== this.resourceId)
            .map(pr => ({
              id: pr.resourceId,
              resourceType: pr.resourceType,
              name: `${pr.resourceName} (${pr.configName})`,
              location: '',
            } as AzureResourceResponse));
          this.allResources.set([...localResources, ...crossConfig]);
        } catch {
          this.allResources.set(localResources);
        }
      } else {
        this.allResources.set(localResources);
      }
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

  protected resolveIdentityName(identityId: string): string {
    const res = this.allResources().find(r => r.id === identityId);
    if (res) return res.name;
    const isUuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(identityId);
    return isUuid ? `⚠ ${identityId.substring(0, 8)}…` : identityId;
  }

  protected async switchToSystemAssigned(assignment: RoleAssignmentResponse): Promise<void> {
    this.roleAssignmentsError.set('');
    try {
      const updated = await this.roleAssignmentService.updateIdentity(
        this.resourceId,
        assignment.id,
        { managedIdentityType: 'SystemAssigned' }
      );
      this.roleAssignments.update(list =>
        list.map(ra => ra.id === updated.id ? updated : ra)
      );
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UPDATE_IDENTITY_ERROR');
    }
  }

  protected async switchToUserAssignedIdentity(assignment: RoleAssignmentResponse, identityId: string): Promise<void> {
    this.roleAssignmentsError.set('');
    try {
      const updated = await this.roleAssignmentService.updateIdentity(
        this.resourceId,
        assignment.id,
        { managedIdentityType: 'UserAssigned', userAssignedIdentityId: identityId }
      );
      this.roleAssignments.update(list =>
        list.map(ra => ra.id === updated.id ? updated : ra)
      );
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UPDATE_IDENTITY_ERROR');
    }
  }

  protected openAddRoleAssignmentDialog(): void {
    const baseData = {
      sourceResourceId: this.resourceId,
      currentResourceName: this.resource()?.name ?? '',
      siblingResources: this.allResources(),
      resourceGroupId: (this.resource() as { resourceGroupId?: string })?.resourceGroupId ?? '',
      configLocation: this.resource()?.location ?? 'EastUS2',
    };

    const assignedUai = this.assignedUai();
    const data: AddRoleAssignmentDialogData = this.isUserAssignedIdentity()
      ? {
          ...baseData,
          isFromUserAssignedIdentity: true,
          userAssignedIdentityId: this.resourceId,
          userAssignedIdentityName: this.resource()?.name ?? '',
        }
      : {
          ...baseData,
          ...(assignedUai ? {
            assignedUserAssignedIdentityId: assignedUai.identityId,
            assignedUserAssignedIdentityName: assignedUai.identityName,
          } : {}),
        };

    const dialogRef = this.dialog.open(AddRoleAssignmentDialogComponent, {
      data,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe(async (dialogResult?: AddRoleAssignmentDialogResult) => {
      if (dialogResult) {
        const result = dialogResult.roleAssignment;
        // Immediately add any newly created identities so names resolve without delay
        if (dialogResult.createdIdentities.length > 0) {
          this.allResources.update(list => {
            const existingIds = new Set(list.map(r => r.id));
            const newOnes = dialogResult.createdIdentities.filter(ci => !existingIds.has(ci.id));
            return newOnes.length > 0 ? [...list, ...newOnes] : list;
          });
        }
        if (this.isUserAssignedIdentity()) {
          this.loadIdentityRoleAssignments();
        } else {
          this.roleAssignments.update(list => [...list, result]);
          this.loadRoleAssignments();
          if (this.supportsAppSettings()) {
            this.loadAppSettings();
          }
          if (this.supportsConfigKeys()) {
            this.loadConfigKeys();
          }
        }
        if (result.userAssignedIdentityId && !this.allResources().some(r => r.id === result.userAssignedIdentityId)) {
          await this.loadAllResources();
        }
      }
    });
  }

  protected async assignUaiToResource(identityId: string): Promise<void> {
    this.roleAssignmentsError.set('');
    try {
      await this.roleAssignmentService.assignIdentity(this.resourceId, identityId);
      await this.loadRoleAssignments();
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UPDATE_IDENTITY_ERROR');
    }
  }

  protected openUnassignUaiDialog(uai: { identityId: string; identityName: string }): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_TITLE',
        messageKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_MESSAGE',
        messageParams: { identity: uai.identityName },
        confirmKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_CONFIRM',
        cancelKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_CANCEL',
      } satisfies ConfirmDialogData,
      width: '480px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.unassignUaiFromResource(uai);
    });
  }

  private async unassignUaiFromResource(uai: { identityId: string; identityName: string }): Promise<void> {
    this.roleAssignmentsError.set('');
    try {
      await this.roleAssignmentService.unassignIdentity(this.resourceId);
      await this.loadRoleAssignments();
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_ERROR');
    }
  }

  private async checkUaiUsageAndProposeDelete(identityId: string, identityName: string): Promise<void> {
    try {
      const grants = await this.userAssignedIdentityService.getGrantedRoleAssignments(identityId);
      const usedByOthers = grants.some(ra => ra.sourceResourceId !== identityId);
      if (usedByOthers) return;

      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          titleKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.DELETE_UNUSED_UAI_TITLE',
          messageKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.DELETE_UNUSED_UAI_MESSAGE',
          messageParams: { identity: identityName },
          confirmKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.DELETE_UNUSED_UAI_CONFIRM',
          cancelKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.DELETE_UNUSED_UAI_CANCEL',
        } satisfies ConfirmDialogData,
        width: '480px',
      });

      dialogRef.afterClosed().subscribe(async (deleteConfirmed?: boolean) => {
        if (!deleteConfirmed) return;
        try {
          await this.userAssignedIdentityService.delete(identityId);
          this.allResources.update(list => list.filter(r => r.id !== identityId));
        } catch {
          this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.DELETE_UAI_ERROR');
        }
      });
    } catch {
      // Silently fail the usage check — unassign already succeeded
    }
  }

  protected async openRemoveRoleAssignmentDialog(assignment: RoleAssignmentResponse): Promise<void> {
    const roleName = this.resolveRoleName(assignment.roleDefinitionId);
    const targetName = this.resolveTargetName(assignment.targetResourceId);

    let impactResult: RoleAssignmentImpactResponse;
    try {
      impactResult = await this.roleAssignmentService.analyzeImpact(this.resourceId, assignment.id);
    } catch {
      // Impact analysis failed — fall back to a simple confirmation dialog
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
      return;
    }

    const filteredImpacts = impactResult.impacts.filter(i => i.impactType !== 'LastRoleToTarget');
    if (filteredImpacts.length === 0) {
      await this.removeRoleAssignment(assignment.id);
      return;
    }

    const dialogRef = this.dialog.open(RoleAssignmentImpactDialogComponent, {
      data: {
        roleName,
        targetResourceName: targetName,
        impactResult,
      } satisfies RoleAssignmentImpactDialogData,
      width: '520px',
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
      if (this.isAcrEnabled()) {
        await this.checkAcrPullAccess();
      }
      if (this.supportsAppSettings()) {
        this.loadAppSettings();
      }
      if (this.supportsConfigKeys()) {
        this.loadConfigKeys();
      }
    } catch {
      this.roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_ERROR');
    }
  }

  // ─── Identity Granted Role Assignments (UAI) ───

  private async loadIdentityRoleAssignments(): Promise<void> {
    this.identityRoleAssignmentsLoading.set(true);
    this.identityRoleAssignmentsError.set('');
    try {
      const assignments = await this.userAssignedIdentityService.getGrantedRoleAssignments(this.resourceId);
      this.identityRoleAssignments.set(assignments);
    } catch {
      this.identityRoleAssignmentsError.set('RESOURCE_EDIT.GRANTED_RIGHTS.LOAD_ERROR');
    } finally {
      this.identityRoleAssignmentsLoading.set(false);
    }
  }

  protected openUnlinkResourceFromIdentityDialog(group: { sourceResourceId: string; sourceResourceName: string; sourceResourceType: string; assignments: IdentityRoleAssignmentResponse[] }): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.USED_BY.UNLINK_RESOURCE_TITLE',
        messageKey: 'RESOURCE_EDIT.USED_BY.UNLINK_RESOURCE_MESSAGE',
        messageParams: { resource: group.sourceResourceName },
        confirmKey: 'RESOURCE_EDIT.USED_BY.UNLINK_YES',
        cancelKey: 'RESOURCE_EDIT.USED_BY.UNLINK_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.unlinkAllAssignmentsForResource(group);
    });
  }

  private async unlinkAllAssignmentsForResource(group: { sourceResourceId: string; assignments: IdentityRoleAssignmentResponse[] }): Promise<void> {
    this.identityRoleAssignmentsError.set('');
    try {
      await this.userAssignedIdentityService.unlinkResource(this.resourceId, group.sourceResourceId);
      await this.loadIdentityRoleAssignments();
    } catch {
      this.identityRoleAssignmentsError.set('RESOURCE_EDIT.USED_BY.UNLINK_ERROR');
    }
  }

  // ─── App Settings ───

  private async loadAppSettings(): Promise<void> {
    this.appSettingsLoading.set(true);
    this.appSettingsError.set('');
    try {
      const settings = await this.appSettingService.getByResourceId(this.resourceId);
      this.appSettings.set(settings);
      this.checkKvAccessForAppSettings(settings);
    } catch {
      this.appSettingsError.set('RESOURCE_EDIT.APP_SETTINGS.LOAD_ERROR');
    } finally {
      this.appSettingsLoading.set(false);
    }
  }

  private async checkKvAccessForAppSettings(settings: AppSettingResponse[]): Promise<void> {
    const missingByKv = new Map<string, AppSettingResponse[]>();
    for (const s of settings) {
      if (s.isKeyVaultReference && s.keyVaultResourceId && s.hasKeyVaultAccess === false) {
        const existing = missingByKv.get(s.keyVaultResourceId);
        if (existing) {
          existing.push(s);
        } else {
          missingByKv.set(s.keyVaultResourceId, [s]);
        }
      }
    }

    if (missingByKv.size === 0) {
      this.kvMissingRoleEntries.set([]);
      return;
    }

    const uaiOptions = this.uaiOptionsForSelect();
    const assigned = this.assignedUai();
    const singleUaiId = assigned ? assigned.identityId : (uaiOptions.length === 1 ? uaiOptions[0].value : null);
    const entries: KvMissingRoleEntry[] = [...missingByKv.entries()].map(([kvId, affected]) => ({
      keyVaultResourceId: kvId,
      keyVaultName: this.resolveSourceName(kvId),
      missingRoleName: null,
      missingRoleDefinitionId: null,
      affectedSettingsCount: affected.length,
      checking: true,
      assigning: false,
      selectedIdentityType: 'UserAssigned' as const,
      selectedUaiId: singleUaiId,
    }));
    this.kvMissingRoleEntries.set(entries);
    this.kvMissingRoleChecking.set(true);

    for (const entry of entries) {
      try {
        const result = await this.appSettingService.checkKeyVaultAccess(this.resourceId, entry.keyVaultResourceId);
        this.kvMissingRoleEntries.update(list =>
          list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId
            ? { ...e, missingRoleName: result.missingRoleName ?? null, missingRoleDefinitionId: result.missingRoleDefinitionId ?? null, checking: false }
            : e
          )
        );
      } catch {
        this.kvMissingRoleEntries.update(list =>
          list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId ? { ...e, checking: false } : e)
        );
      }
    }
    this.kvMissingRoleChecking.set(false);
  }

  protected async assignKvRole(entry: KvMissingRoleEntry): Promise<void> {
    this.kvMissingRoleEntries.update(list =>
      list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId ? { ...e, assigning: true } : e)
    );
    try {
      await this.roleAssignmentService.add(this.resourceId, {
        targetResourceId: entry.keyVaultResourceId,
        managedIdentityType: entry.selectedIdentityType,
        roleDefinitionId: entry.missingRoleDefinitionId!,
        userAssignedIdentityId: entry.selectedIdentityType === 'UserAssigned' ? (this.assignedUai()?.identityId ?? entry.selectedUaiId!) : undefined,
      });
      await this.loadAppSettings();
      await this.loadRoleAssignments();
    } catch {
      this.kvMissingRoleEntries.update(list =>
        list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId ? { ...e, assigning: false } : e)
      );
    }
  }

  protected setKvEntryIdentityType(kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void {
    this.kvMissingRoleEntries.update(list =>
      list.map(e => e.keyVaultResourceId === kvId ? { ...e, selectedIdentityType: type, selectedUaiId: type === 'SystemAssigned' ? null : e.selectedUaiId } : e)
    );
  }

  protected setKvEntryUaiId(kvId: string, uaiId: string | null): void {
    this.kvMissingRoleEntries.update(list =>
      list.map(e => e.keyVaultResourceId === kvId ? { ...e, selectedUaiId: uaiId } : e)
    );
  }

  protected createNewUaiForKvEntry(kvId: string): void {
    const res = this.resource();
    if (!res) return;
    const resourceGroupId = (res as { resourceGroupId?: string })?.resourceGroupId ?? '';
    const location = res.location ?? 'EastUS2';

    const dialogRef = this.dialog.open(CreateUaiDialogComponent, {
      data: { resourceGroupId, location } as CreateUaiDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (result: UserAssignedIdentityResponse | undefined) => {
      if (!result) return;
      await this.loadAllResources();
      this.setKvEntryUaiId(kvId, result.id);
    });
  }

  protected resolveSourceName(sourceResourceId: string): string {
    const res = this.allResources().find(r => r.id === sourceResourceId);
    return res?.name ?? sourceResourceId;
  }

  protected resolveSourceType(sourceResourceId: string): string {
    const res = this.allResources().find(r => r.id === sourceResourceId);
    return res?.resourceType ?? '';
  }

  protected openAddAppSettingDialog(): void {
    const dialogRef = this.dialog.open(AddAppSettingDialogComponent, {
      data: {
        resourceId: this.resourceId,
        currentResourceName: this.resource()?.name ?? '',
        siblingResources: this.allResources(),
        environments: this.environments().map(e => ({ name: e.name })),
        projectId: this.config()?.projectId ?? '',
      } satisfies AddAppSettingDialogData,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe((result?: AppSettingResponse) => {
      if (result) {
        this.appSettings.update(list => [...list, result]);
      }
    });
  }

  protected openRemoveAppSettingDialog(setting: AppSettingResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_TITLE',
        messageKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_MESSAGE',
        messageParams: { name: setting.name },
        confirmKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_YES',
        cancelKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.removeAppSetting(setting.id);
    });
  }

  protected openEditStaticAppSettingDialog(setting: AppSettingResponse): void {
    const dialogRef = this.dialog.open(EditStaticAppSettingDialogComponent, {
      data: {
        resourceId: this.resourceId,
        appSettingId: setting.id,
        currentName: setting.name,
        currentEnvironmentValues: setting.environmentValues ?? {},
        environments: this.environments().map(e => ({ name: e.name })),
      } satisfies EditStaticAppSettingDialogData,
      width: '480px',
      maxHeight: '80vh',
    });

    dialogRef.afterClosed().subscribe((result?: AppSettingResponse) => {
      if (result) {
        this.appSettings.update(list => list.map(s => s.id === result.id ? result : s));
      }
    });
  }

  private async removeAppSetting(appSettingId: string): Promise<void> {
    this.appSettingsError.set('');
    try {
      await this.appSettingService.remove(this.resourceId, appSettingId);
      this.appSettings.update(list => list.filter(s => s.id !== appSettingId));
    } catch {
      this.appSettingsError.set('RESOURCE_EDIT.APP_SETTINGS.REMOVE_ERROR');
    }
  }

  // ─── Configuration Keys ───

  private async loadConfigKeys(): Promise<void> {
    this.configKeysLoading.set(true);
    this.configKeysError.set('');
    try {
      const keys = await this.configKeyService.list(this.resourceId);
      this.configKeys.set(keys);
      this.checkKvAccessForConfigKeys(keys);
    } catch {
      this.configKeysError.set('RESOURCE_EDIT.CONFIG_KEYS.LOAD_ERROR');
    } finally {
      this.configKeysLoading.set(false);
    }
  }

  private async checkKvAccessForConfigKeys(keys: AppConfigurationKeyResponse[]): Promise<void> {
    const missingByKv = new Map<string, AppConfigurationKeyResponse[]>();
    for (const k of keys) {
      if (k.isKeyVaultReference && k.keyVaultResourceId && k.hasKeyVaultAccess === false) {
        const existing = missingByKv.get(k.keyVaultResourceId);
        if (existing) {
          existing.push(k);
        } else {
          missingByKv.set(k.keyVaultResourceId, [k]);
        }
      }
    }

    if (missingByKv.size === 0) {
      this.configKeyKvMissingRoleEntries.set([]);
      return;
    }

    const uaiOptions = this.uaiOptionsForSelect();
    const assigned = this.assignedUai();
    const singleUaiId = assigned ? assigned.identityId : (uaiOptions.length === 1 ? uaiOptions[0].value : null);
    const entries: KvMissingRoleEntry[] = [...missingByKv.entries()].map(([kvId, affected]) => ({
      keyVaultResourceId: kvId,
      keyVaultName: this.resolveSourceName(kvId),
      missingRoleName: null,
      missingRoleDefinitionId: null,
      affectedSettingsCount: affected.length,
      checking: true,
      assigning: false,
      selectedIdentityType: 'UserAssigned' as const,
      selectedUaiId: singleUaiId,
    }));
    this.configKeyKvMissingRoleEntries.set(entries);
    this.configKeyKvMissingRoleChecking.set(true);

    for (const entry of entries) {
      try {
        const result = await this.appSettingService.checkKeyVaultAccess(this.resourceId, entry.keyVaultResourceId);
        this.configKeyKvMissingRoleEntries.update(list =>
          list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId
            ? { ...e, missingRoleName: result.missingRoleName ?? null, missingRoleDefinitionId: result.missingRoleDefinitionId ?? null, checking: false }
            : e
          )
        );
      } catch {
        this.configKeyKvMissingRoleEntries.update(list =>
          list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId ? { ...e, checking: false } : e)
        );
      }
    }
    this.configKeyKvMissingRoleChecking.set(false);
  }

  protected async assignConfigKeyKvRole(entry: KvMissingRoleEntry): Promise<void> {
    this.configKeyKvMissingRoleEntries.update(list =>
      list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId ? { ...e, assigning: true } : e)
    );
    try {
      await this.roleAssignmentService.add(this.resourceId, {
        targetResourceId: entry.keyVaultResourceId,
        managedIdentityType: entry.selectedIdentityType,
        roleDefinitionId: entry.missingRoleDefinitionId!,
        userAssignedIdentityId: entry.selectedIdentityType === 'UserAssigned' ? (this.assignedUai()?.identityId ?? entry.selectedUaiId!) : undefined,
      });
      await this.loadConfigKeys();
      await this.loadRoleAssignments();
    } catch {
      this.configKeyKvMissingRoleEntries.update(list =>
        list.map(e => e.keyVaultResourceId === entry.keyVaultResourceId ? { ...e, assigning: false } : e)
      );
    }
  }

  protected setConfigKeyKvEntryIdentityType(kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void {
    this.configKeyKvMissingRoleEntries.update(list =>
      list.map(e => e.keyVaultResourceId === kvId ? { ...e, selectedIdentityType: type, selectedUaiId: type === 'SystemAssigned' ? null : e.selectedUaiId } : e)
    );
  }

  protected setConfigKeyKvEntryUaiId(kvId: string, uaiId: string | null): void {
    this.configKeyKvMissingRoleEntries.update(list =>
      list.map(e => e.keyVaultResourceId === kvId ? { ...e, selectedUaiId: uaiId } : e)
    );
  }

  protected createNewUaiForConfigKeyKvEntry(kvId: string): void {
    const res = this.resource();
    if (!res) return;
    const resourceGroupId = (res as { resourceGroupId?: string })?.resourceGroupId ?? '';
    const location = res.location ?? 'EastUS2';

    const dialogRef = this.dialog.open(CreateUaiDialogComponent, {
      data: { resourceGroupId, location } as CreateUaiDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (result: UserAssignedIdentityResponse | undefined) => {
      if (!result) return;
      await this.loadAllResources();
      this.setConfigKeyKvEntryUaiId(kvId, result.id);
    });
  }

  protected openAddConfigKeyDialog(): void {
    const dialogRef = this.dialog.open(AddAppConfigKeyDialogComponent, {
      data: {
        appConfigurationId: this.resourceId,
        siblingResources: this.allResources(),
        environments: this.environments().map(e => ({ name: e.name })),
        projectId: this.config()?.projectId ?? '',
      } satisfies AddAppConfigKeyDialogData,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe((result?: AppConfigurationKeyResponse) => {
      if (result) {
        this.configKeys.update(list => [...list, result]);
      }
    });
  }

  protected openRemoveConfigKeyDialog(configKey: AppConfigurationKeyResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.CONFIG_KEYS.REMOVE_TITLE',
        messageKey: 'RESOURCE_EDIT.CONFIG_KEYS.REMOVE_MESSAGE',
        messageParams: { name: configKey.key },
        confirmKey: 'RESOURCE_EDIT.CONFIG_KEYS.REMOVE_YES',
        cancelKey: 'RESOURCE_EDIT.CONFIG_KEYS.REMOVE_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      await this.removeConfigKey(configKey.id);
    });
  }

  private async removeConfigKey(configKeyId: string): Promise<void> {
    this.configKeysError.set('');
    try {
      await this.configKeyService.remove(this.resourceId, configKeyId);
      this.configKeys.update(list => list.filter(k => k.id !== configKeyId));
    } catch {
      this.configKeysError.set('RESOURCE_EDIT.CONFIG_KEYS.REMOVE_ERROR');
    }
  }

  protected getConfigKeyType(key: AppConfigurationKeyResponse): string {
    if (key.isOutputReference) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_OUTPUT';
    if (key.isKeyVaultReference) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_KV';
    if (key.isViaVariableGroup) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_VG';
    return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_STATIC';
  }

  // ─── Storage Services ───

  protected blobAddForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(63), this.uniqueBlobNameValidator()]],
    publicAccess: ['None'],
  });
  protected queueAddForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(63), this.uniqueQueueNameValidator()]],
  });
  protected tableAddForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(63), this.uniqueTableNameValidator()]],
  });

  private uniqueBlobNameValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      const name = control.value?.trim().toLowerCase();
      if (!name) return null;
      const exists = this.storageBlobContainers().some(b => b.name.toLowerCase() === name);
      return exists ? { duplicateName: true } : null;
    };
  }

  private uniqueQueueNameValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      const name = control.value?.trim().toLowerCase();
      if (!name) return null;
      const exists = this.storageQueues().some(q => q.name.toLowerCase() === name);
      return exists ? { duplicateName: true } : null;
    };
  }

  private uniqueTableNameValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      const name = control.value?.trim().toLowerCase();
      if (!name) return null;
      const exists = this.storageTables().some(t => t.name.toLowerCase() === name);
      return exists ? { duplicateName: true } : null;
    };
  }

  protected readonly publicAccessOptions = [
    { label: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_NONE', value: 'None' },
    { label: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_BLOB', value: 'Blob' },
    { label: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_CONTAINER', value: 'Container' },
  ];

  protected addCorsRule(): void {
    this.addCorsRuleFor('blob');
  }

  protected removeCorsRule(index: number): void {
    this.removeCorsRuleFor('blob', index);
  }

  protected addCorsRuleValue(index: number, field: CorsListField, value: string): void {
    this.addCorsRuleValueFor('blob', index, field, value);
  }

  protected removeCorsRuleValue(index: number, field: CorsListField, valueIndex: number): void {
    this.removeCorsRuleValueFor('blob', index, field, valueIndex);
  }

  protected toggleCorsRuleMethod(index: number, method: string): void {
    this.toggleCorsMethodFor('blob', index, method);
  }

  protected isCorsRuleMethodSelected(rule: CorsRuleEntry, method: string): boolean {
    return rule.allowedMethods.includes(method);
  }

  protected updateCorsRuleMaxAge(index: number, rawValue: string): void {
    this.updateCorsRuleMaxAgeFor('blob', index, rawValue);
  }

  protected setCorsRuleMaxAgePreset(index: number, value: number): void {
    this.setCorsRuleMaxAgePresetFor('blob', index, value);
  }

  protected addTableCorsRule(): void {
    this.addCorsRuleFor('table');
  }

  protected removeTableCorsRule(index: number): void {
    this.removeCorsRuleFor('table', index);
  }

  protected addTableCorsRuleValue(index: number, field: CorsListField, value: string): void {
    this.addCorsRuleValueFor('table', index, field, value);
  }

  protected removeTableCorsRuleValue(index: number, field: CorsListField, valueIndex: number): void {
    this.removeCorsRuleValueFor('table', index, field, valueIndex);
  }

  protected toggleTableCorsRuleMethod(index: number, method: string): void {
    this.toggleCorsMethodFor('table', index, method);
  }

  protected updateTableCorsRuleMaxAge(index: number, rawValue: string): void {
    this.updateCorsRuleMaxAgeFor('table', index, rawValue);
  }

  protected setTableCorsRuleMaxAgePreset(index: number, value: number): void {
    this.setCorsRuleMaxAgePresetFor('table', index, value);
  }

  // ─── Lifecycle Rules ───

  protected addLifecycleRule(): void {
    this.lifecycleRulesDraft.update(rules => [
      ...rules,
      { ruleName: '', containerNames: [], timeToLiveInDays: 30 },
    ]);
    this.formsDirty.set(true);
  }

  protected removeLifecycleRule(index: number): void {
    this.lifecycleRulesDraft.update(rules => rules.filter((_, i) => i !== index));
    this.formsDirty.set(true);
  }

  protected updateLifecycleRuleName(index: number, name: string): void {
    this.lifecycleRulesDraft.update(rules =>
      rules.map((r, i) => (i === index ? { ...r, ruleName: name } : r)),
    );
    this.formsDirty.set(true);
  }

  protected updateLifecycleRuleTtl(index: number, rawValue: string): void {
    const parsed = parseInt(rawValue, 10);
    if (isNaN(parsed)) return;
    this.lifecycleRulesDraft.update(rules =>
      rules.map((r, i) => (i === index ? { ...r, timeToLiveInDays: parsed } : r)),
    );
    this.formsDirty.set(true);
  }

  protected addLifecycleRuleContainer(index: number, containerName: string): void {
    const trimmed = containerName.trim();
    if (!trimmed) return;
    this.lifecycleRulesDraft.update(rules =>
      rules.map((r, i) => {
        if (i !== index || r.containerNames.includes(trimmed)) return r;
        return { ...r, containerNames: [...r.containerNames, trimmed] };
      }),
    );
    this.formsDirty.set(true);
  }

  protected removeLifecycleRuleContainer(ruleIndex: number, containerIndex: number): void {
    this.lifecycleRulesDraft.update(rules =>
      rules.map((r, i) => {
        if (i !== ruleIndex) return r;
        return { ...r, containerNames: r.containerNames.filter((_, ci) => ci !== containerIndex) };
      }),
    );
    this.formsDirty.set(true);
  }

  protected toggleLifecycleRuleContainer(ruleIndex: number, containerName: string): void {
    this.lifecycleRulesDraft.update(rules =>
      rules.map((r, i) => {
        if (i !== ruleIndex) return r;
        const selected = r.containerNames.includes(containerName);
        return {
          ...r,
          containerNames: selected
            ? r.containerNames.filter(cn => cn !== containerName)
            : [...r.containerNames, containerName],
        };
      }),
    );
    this.formsDirty.set(true);
  }

  protected isLifecycleContainerSelected(rule: { containerNames: string[] }, containerName: string): boolean {
    return rule.containerNames.includes(containerName);
  }

  protected corsFieldError(service: CorsServiceKey, index: number, field: CorsFieldKey): string {
    return this.corsFieldErrors()[this.buildCorsErrorKey(service, index, field)] ?? '';
  }

  protected corsHeaderSuggestions(field: 'allowedHeaders' | 'exposedHeaders'): readonly string[] {
    return field === 'allowedHeaders' ? this.corsAllowedHeaderSuggestions : this.corsExposedHeaderSuggestions;
  }

  protected corsMethodTrack(method: string): string {
    return method;
  }

  private readonly publicAccessI18nMap: Record<string, string> = {
    None: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_NONE',
    Blob: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_BLOB',
    Container: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_CONTAINER',
  };

  protected publicAccessI18nKey(value: string): string {
    return this.publicAccessI18nMap[value] ?? value;
  }

  protected async updateBlobPublicAccess(blobId: string, newAccess: string): Promise<void> {
    this.storageActionLoading.set(true);
    this.storageActionError.set('');
    try {
      const updated = await this.storageAccountService.updateBlobContainerPublicAccess(
        this.resourceId, blobId, { publicAccess: newAccess }
      );
      this.resource.set(updated);
    } catch {
      this.storageActionError.set('RESOURCE_EDIT.STORAGE_SERVICES.ACTION_ERROR');
    } finally {
      this.storageActionLoading.set(false);
    }
  }

  protected async addBlobContainer(): Promise<void> {
    if (this.blobAddForm.invalid || this.storageActionLoading()) return;
    this.storageActionLoading.set(true);
    this.storageActionError.set('');
    try {
      const { name, publicAccess } = this.blobAddForm.getRawValue();
      const updated = await this.storageAccountService.addBlobContainer(this.resourceId, {
        name: name!,
        publicAccess: publicAccess ?? 'None',
      });
      this.resource.set(updated);
      this.blobAddForm.reset({ name: '', publicAccess: 'None' });
      this.showBlobAddForm.set(false);
    } catch {
      this.storageActionError.set('RESOURCE_EDIT.STORAGE_SERVICES.ACTION_ERROR');
    } finally {
      this.storageActionLoading.set(false);
    }
  }

  protected async addQueue(): Promise<void> {
    if (this.queueAddForm.invalid || this.storageActionLoading()) return;
    this.storageActionLoading.set(true);
    this.storageActionError.set('');
    try {
      const { name } = this.queueAddForm.getRawValue();
      const updated = await this.storageAccountService.addQueue(this.resourceId, { name: name! });
      this.resource.set(updated);
      this.queueAddForm.reset({ name: '' });
      this.showQueueAddForm.set(false);
    } catch {
      this.storageActionError.set('RESOURCE_EDIT.STORAGE_SERVICES.ACTION_ERROR');
    } finally {
      this.storageActionLoading.set(false);
    }
  }

  protected async addTable(): Promise<void> {
    if (this.tableAddForm.invalid || this.storageActionLoading()) return;
    this.storageActionLoading.set(true);
    this.storageActionError.set('');
    try {
      const { name } = this.tableAddForm.getRawValue();
      const updated = await this.storageAccountService.addTable(this.resourceId, { name: name! });
      this.resource.set(updated);
      this.tableAddForm.reset({ name: '' });
      this.showTableAddForm.set(false);
    } catch {
      this.storageActionError.set('RESOURCE_EDIT.STORAGE_SERVICES.ACTION_ERROR');
    } finally {
      this.storageActionLoading.set(false);
    }
  }

  protected openRemoveStorageItemDialog(type: 'BlobContainer' | 'Queue' | 'Table', id: string, name: string): void {
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
      await this.removeStorageItem(type, id);
    });
  }

  private async removeStorageItem(type: 'BlobContainer' | 'Queue' | 'Table', id: string): Promise<void> {
    this.storageActionLoading.set(true);
    this.storageActionError.set('');
    try {
      switch (type) {
        case 'BlobContainer':
          await this.storageAccountService.removeBlobContainer(this.resourceId, id);
          break;
        case 'Queue':
          await this.storageAccountService.removeQueue(this.resourceId, id);
          break;
        case 'Table':
          await this.storageAccountService.removeTable(this.resourceId, id);
          break;
      }
      const updated = await this.storageAccountService.getById(this.resourceId);
      this.resource.set(updated);
    } catch {
      this.storageActionError.set('RESOURCE_EDIT.STORAGE_SERVICES.ACTION_ERROR');
    } finally {
      this.storageActionLoading.set(false);
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
      };
    });
  }

  private buildStorageAccountCorsRules(): CorsRuleEntry[] {
    return this.storageCorsRulesDraft().map(rule => ({
      allowedOrigins: [...rule.allowedOrigins],
      allowedMethods: [...rule.allowedMethods],
      allowedHeaders: [...rule.allowedHeaders],
      exposedHeaders: [...rule.exposedHeaders],
      maxAgeInSeconds: rule.maxAgeInSeconds,
    }));
  }

  private buildStorageAccountTableCorsRules(): CorsRuleEntry[] {
    return this.storageTableCorsRulesDraft().map(rule => ({
      allowedOrigins: [...rule.allowedOrigins],
      allowedMethods: [...rule.allowedMethods],
      allowedHeaders: [...rule.allowedHeaders],
      exposedHeaders: [...rule.exposedHeaders],
      maxAgeInSeconds: rule.maxAgeInSeconds,
    }));
  }

  private buildLifecycleRules(): BlobLifecycleRuleEntry[] {
    return this.lifecycleRulesDraft().map(rule => ({
      ruleName: rule.ruleName,
      containerNames: [...rule.containerNames],
      timeToLiveInDays: rule.timeToLiveInDays,
    }));
  }

  private addCorsRuleFor(service: CorsServiceKey): void {
    this.updateCorsRules(service, rules => [
      ...rules,
      {
        allowedOrigins: [],
        allowedMethods: ['GET'],
        allowedHeaders: [],
        exposedHeaders: [],
        maxAgeInSeconds: 3600,
      },
    ]);
    this.formsDirty.set(true);
  }

  private removeCorsRuleFor(service: CorsServiceKey, index: number): void {
    this.updateCorsRules(service, rules => rules.filter((_, ruleIndex) => ruleIndex !== index));
    this.clearCorsRuleErrors(service, index);
    this.formsDirty.set(true);
  }

  private addCorsRuleValueFor(service: CorsServiceKey, index: number, field: CorsListField, value: string): void {
    const normalized = field === 'allowedOrigins'
      ? this.normalizeCorsOrigin(value)
      : this.normalizeCorsHeader(value);

    const validationError = field === 'allowedOrigins'
      ? this.validateCorsOrigin(value)
      : this.validateCorsHeader(value, field);

    if (validationError) {
      this.setCorsFieldError(service, index, field, validationError);
      return;
    }

    const rules = this.corsRulesFor(service);
    const rule = rules[index];
    if (!rule) return;

    if (field !== 'allowedOrigins') {
      const prefixedCount = rule[field].filter(header => header.endsWith('*')).length;
      const literalCount = rule[field].length - prefixedCount;
      if (normalized.endsWith('*') && prefixedCount >= 2) {
        this.setCorsFieldError(service, index, field, 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_TOO_MANY_PREFIX_HEADERS');
        return;
      }

      if (!normalized.endsWith('*') && literalCount >= 64) {
        this.setCorsFieldError(service, index, field, 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_TOO_MANY_LITERAL_HEADERS');
        return;
      }
    }

    if (rule[field].some(existing => existing.toLowerCase() === normalized.toLowerCase())) {
      this.setCorsFieldError(service, index, field, 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_DUPLICATE_VALUE');
      return;
    }

    this.updateCorsRules(service, currentRules => currentRules.map((currentRule, currentIndex) =>
      currentIndex === index ? { ...currentRule, [field]: [...currentRule[field], normalized] } : currentRule
    ));
    this.clearCorsFieldError(service, index, field);
    this.formsDirty.set(true);
  }

  private removeCorsRuleValueFor(service: CorsServiceKey, index: number, field: CorsListField, valueIndex: number): void {
    this.updateCorsRules(service, rules => rules.map((rule, currentIndex) =>
      currentIndex === index ? { ...rule, [field]: rule[field].filter((_, currentValueIndex) => currentValueIndex !== valueIndex) } : rule
    ));
    this.clearCorsFieldError(service, index, field);
    this.formsDirty.set(true);
  }

  private toggleCorsMethodFor(service: CorsServiceKey, index: number, method: string): void {
    const rules = this.corsRulesFor(service);
    const rule = rules[index];
    if (!rule) return;

    const nextMethods = rule.allowedMethods.includes(method)
      ? rule.allowedMethods.filter(existing => existing !== method)
      : [...rule.allowedMethods, method].sort((left, right) => this.corsMethodOptions.indexOf(left) - this.corsMethodOptions.indexOf(right));

    this.updateCorsRules(service, currentRules => currentRules.map((currentRule, currentIndex) =>
      currentIndex === index ? { ...currentRule, allowedMethods: nextMethods } : currentRule
    ));

    if (nextMethods.length === 0) {
      this.setCorsFieldError(service, index, 'allowedMethods', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_METHOD_REQUIRED');
    } else {
      this.clearCorsFieldError(service, index, 'allowedMethods');
    }

    this.formsDirty.set(true);
  }

  private updateCorsRuleMaxAgeFor(service: CorsServiceKey, index: number, rawValue: string): void {
    const trimmed = rawValue.trim();
    const maxAgeInSeconds = Number(trimmed);
    if (!trimmed || !Number.isInteger(maxAgeInSeconds) || maxAgeInSeconds < 0) {
      this.setCorsFieldError(service, index, 'maxAgeInSeconds', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_MAX_AGE');
      return;
    }

    this.updateCorsRules(service, rules => rules.map((rule, ruleIndex) =>
      ruleIndex === index ? { ...rule, maxAgeInSeconds } : rule
    ));
    this.clearCorsFieldError(service, index, 'maxAgeInSeconds');
    this.formsDirty.set(true);
  }

  private setCorsRuleMaxAgePresetFor(service: CorsServiceKey, index: number, value: number): void {
    this.updateCorsRules(service, rules => rules.map((rule, ruleIndex) =>
      ruleIndex === index ? { ...rule, maxAgeInSeconds: value } : rule
    ));
    this.clearCorsFieldError(service, index, 'maxAgeInSeconds');
    this.formsDirty.set(true);
  }

  private corsRulesFor(service: CorsServiceKey): CorsRuleEntry[] {
    return service === 'blob' ? this.storageCorsRulesDraft() : this.storageTableCorsRulesDraft();
  }

  private updateCorsRules(service: CorsServiceKey, updater: (rules: CorsRuleEntry[]) => CorsRuleEntry[]): void {
    if (service === 'blob') {
      this.storageCorsRulesDraft.update(updater);
      return;
    }

    this.storageTableCorsRulesDraft.update(updater);
  }

  private validateAllStorageCorsRules(): boolean {
    let isValid = true;
    this.corsFieldErrors.set({});

    for (const service of ['blob', 'table'] as const) {
      this.corsRulesFor(service).forEach((rule, index) => {
        if (rule.allowedOrigins.length === 0) {
          this.setCorsFieldError(service, index, 'allowedOrigins', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_ORIGIN_REQUIRED');
          isValid = false;
        }

        if (rule.allowedMethods.length === 0) {
          this.setCorsFieldError(service, index, 'allowedMethods', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_METHOD_REQUIRED');
          isValid = false;
        }

        if (!Number.isInteger(rule.maxAgeInSeconds) || rule.maxAgeInSeconds < 0) {
          this.setCorsFieldError(service, index, 'maxAgeInSeconds', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_MAX_AGE');
          isValid = false;
        }

        for (const origin of rule.allowedOrigins) {
          if (this.validateCorsOrigin(origin)) {
            this.setCorsFieldError(service, index, 'allowedOrigins', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN');
            isValid = false;
            break;
          }
        }

        for (const field of ['allowedHeaders', 'exposedHeaders'] as const) {
          for (const header of rule[field]) {
            const headerError = this.validateCorsHeader(header, field);
            if (headerError) {
              this.setCorsFieldError(service, index, field, headerError);
              isValid = false;
              break;
            }
          }
        }
      });
    }

    return isValid;
  }

  private validateCorsOrigin(value: string): string {
    const normalized = value.trim();
    if (!normalized) {
      return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_EMPTY_VALUE';
    }

    if (normalized.length > 256) {
      return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_TOO_LONG';
    }

    if (normalized === '*') {
      return '';
    }

    const withoutTrailingSlash = normalized.replace(/\/+$/, '');
    if (withoutTrailingSlash.includes('*.')) {
      const wildcardPattern = /^https?:\/\/\*\.[a-z0-9-]+(?:\.[a-z0-9-]+)*(?::\d{1,5})?$/i;
      return wildcardPattern.test(withoutTrailingSlash)
        ? ''
        : 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN';
    }

    try {
      const url = new URL(withoutTrailingSlash);
      const isHttp = url.protocol === 'http:' || url.protocol === 'https:';
      const hasNoPath = url.pathname === '' || url.pathname === '/';
      const noSearch = !url.search;
      const noHash = !url.hash;
      return isHttp && hasNoPath && noSearch && noHash
        ? ''
        : 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN';
    } catch {
      return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN';
    }
  }

  private normalizeCorsOrigin(value: string): string {
    const normalized = value.trim().replace(/\/+$/, '');
    if (normalized === '*' || normalized.includes('*.')) {
      return normalized.toLowerCase();
    }

    try {
      const url = new URL(normalized);
      return `${url.protocol}//${url.host}`.toLowerCase();
    } catch {
      return normalized;
    }
  }

  private validateCorsHeader(value: string, field: 'allowedHeaders' | 'exposedHeaders'): string {
    const normalized = value.trim();
    if (!normalized) {
      return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_EMPTY_VALUE';
    }

    if (normalized.length > 256) {
      return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_TOO_LONG';
    }

    const headerPattern = /^[A-Za-z0-9!#$%&'*+.^_`|~-]+(?:-[A-Za-z0-9!#$%&'*+.^_`|~-]+)*\*?$/;
    if (!headerPattern.test(normalized)) {
      return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_HEADER';
    }

    void field;
    return '';
  }

  private normalizeCorsHeader(value: string): string {
    return value.trim().toLowerCase();
  }

  private buildCorsErrorKey(service: CorsServiceKey, index: number, field: CorsFieldKey): string {
    return `${service}:${index}:${field}`;
  }

  private setCorsFieldError(service: CorsServiceKey, index: number, field: CorsFieldKey, errorKey: string): void {
    this.corsFieldErrors.update(errors => ({
      ...errors,
      [this.buildCorsErrorKey(service, index, field)]: errorKey,
    }));
  }

  private clearCorsFieldError(service: CorsServiceKey, index: number, field: CorsFieldKey): void {
    this.corsFieldErrors.update(errors => {
      const updated = { ...errors };
      delete updated[this.buildCorsErrorKey(service, index, field)];
      return updated;
    });
  }

  private clearCorsRuleErrors(service: CorsServiceKey, index: number): void {
    this.corsFieldErrors.update(errors => Object.fromEntries(
      Object.entries(errors).filter(([key]) => !key.startsWith(`${service}:${index}:`))
    ));
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
        dockerImageTag: raw.dockerImageTag || null,
      };
    });
  }

  private buildFunctionAppEnvSettings(): FunctionAppEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        httpsOnly: raw.httpsOnly ?? null,
        maxInstanceCount: raw.maxInstanceCount != null ? Number(raw.maxInstanceCount) : null,
        dockerImageTag: raw.dockerImageTag || null,
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
      };
    });
  }

  private buildContainerAppEnvSettings(): ContainerAppEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
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

  private buildServiceBusNamespaceEnvSettings(): ServiceBusNamespaceEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        capacity: raw.capacity != null ? Number(raw.capacity) : null,
        zoneRedundant: raw.zoneRedundant ?? null,
        disableLocalAuth: raw.disableLocalAuth ?? null,
        minimumTlsVersion: raw.minimumTlsVersion || null,
      };
    });
  }

  private buildContainerRegistryEnvSettings(): ContainerRegistryEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        adminUserEnabled: raw.adminUserEnabled ?? null,
        publicNetworkAccess: raw.publicNetworkAccess || null,
        zoneRedundancy: raw.zoneRedundancy ?? null,
      };
    });
  }

  private buildSqlServerEnvSettings(): SqlServerEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        minimalTlsVersion: raw.minimalTlsVersion || null,
      };
    });
  }

  private buildSqlDatabaseEnvSettings(): SqlDatabaseEnvironmentConfigEntry[] {
    return this.envForms().map(ef => {
      const raw = ef.form.getRawValue();
      return {
        environmentName: ef.envName,
        sku: raw.sku || null,
        maxSizeGb: raw.maxSizeGb != null ? Number(raw.maxSizeGb) : null,
        zoneRedundant: raw.zoneRedundant ?? null,
      };
    });
  }

  // ─── Secure Parameter Mappings (SqlServer password config) ───

  private async loadSecureParamMappings(): Promise<void> {
    try {
      const mappings = await this.secureParamMappingService.getByResourceId(this.resourceId);
      this.secureParamMappings.set(mappings);
      const pwdMapping = mappings.find(m => m.secureParameterName === 'administratorLoginPassword');
      if (pwdMapping?.variableGroupId) {
        this.passwordMode.set('variableGroup');
        this.passwordSelectedVgId.set(pwdMapping.variableGroupId);
        this.passwordPipelineVariableName.set(pwdMapping.pipelineVariableName ?? '');
        this.passwordSavedState.set({ mode: 'variableGroup', variableGroupId: pwdMapping.variableGroupId, pipelineVariableName: pwdMapping.pipelineVariableName ?? null });
      } else {
        this.passwordMode.set('random');
        this.passwordSavedState.set({ mode: 'random', variableGroupId: null, pipelineVariableName: null });
      }
      await this.loadPasswordVgOptions();
    } catch {
      // Non-blocking
    }
  }

  private async loadPasswordVgOptions(): Promise<void> {
    const projectId = this.config()?.projectId;
    if (!projectId) return;
    this.passwordVgLoading.set(true);
    try {
      const groups = await this.projectService.getPipelineVariableGroups(projectId);
      this.passwordVgOptions.set(groups);
    } catch {
      this.passwordVgOptions.set([]);
    } finally {
      this.passwordVgLoading.set(false);
    }
  }

  protected onPasswordVgSelectionChange(value: string): void {
    if (value === '__create_new__') {
      this.passwordIsCreatingNewGroup.set(true);
      this.passwordSelectedVgId.set(null);
      this.passwordNewGroupScope.set('project');
    } else {
      this.passwordIsCreatingNewGroup.set(false);
      this.passwordSelectedVgId.set(value);
      this.passwordNewGroupName.set('');
    }
  }

  protected async savePasswordConfig(): Promise<void> {
    this.passwordSaving.set(true);
    this.passwordSaveSuccess.set(false);
    try {
      let variableGroupId: string | null = null;

      if (this.passwordMode() === 'variableGroup') {
        if (this.passwordIsCreatingNewGroup()) {
          const projectId = this.config()?.projectId;
          if (!projectId) return;
          const newGroup = await this.projectService.addPipelineVariableGroup(
            projectId, { groupName: this.passwordNewGroupName() });
          variableGroupId = newGroup.id;
          this.passwordVgOptions.update(groups => [...groups, newGroup]);
          this.passwordSelectedVgId.set(newGroup.id);
          this.passwordIsCreatingNewGroup.set(false);
        } else {
          variableGroupId = this.passwordSelectedVgId();
        }
      }

      await this.secureParamMappingService.set(this.resourceId, {
        secureParameterName: 'administratorLoginPassword',
        variableGroupId: this.passwordMode() === 'variableGroup' ? variableGroupId : null,
        pipelineVariableName: this.passwordMode() === 'variableGroup'
          ? this.passwordPipelineVariableName()
          : null,
      });
      this.passwordSavedState.set({
        mode: this.passwordMode(),
        variableGroupId: this.passwordMode() === 'variableGroup' ? (variableGroupId ?? this.passwordSelectedVgId()) : null,
        pipelineVariableName: this.passwordMode() === 'variableGroup' ? this.passwordPipelineVariableName() : null,
      });
      this.passwordSaveSuccess.set(true);
      setTimeout(() => this.passwordSaveSuccess.set(false), 3000);
    } catch {
      // Error handled silently
    } finally {
      this.passwordSaving.set(false);
    }
  }

  // ─── Custom Domains ───

  private async loadCustomDomains(): Promise<void> {
    this.customDomainsLoading.set(true);
    this.customDomainsError.set('');
    try {
      const domains = await this.customDomainService.getByResourceId(this.resourceId);
      this.customDomains.set(domains);
    } catch {
      this.customDomainsError.set('RESOURCE_EDIT.CUSTOM_DOMAINS.LOAD_ERROR');
    } finally {
      this.customDomainsLoading.set(false);
    }
  }

  protected openAddCustomDomainDialog(preselectedEnv?: string): void {
    const environments = this.environments();
    const data: AddCustomDomainDialogData = {
      environments,
      existingDomains: this.customDomains(),
      preselectedEnvironment: preselectedEnv,
    };
    const dialogRef = this.dialog.open(AddCustomDomainDialogComponent, {
      width: '520px',
      data,
    });
    dialogRef.afterClosed().subscribe(async (result: AddCustomDomainRequest | null) => {
      if (!result) return;
      this.customDomainsLoading.set(true);
      this.customDomainsError.set('');
      try {
        await this.customDomainService.add(this.resourceId, result);
        await this.loadCustomDomains();
      } catch {
        this.customDomainsError.set('RESOURCE_EDIT.CUSTOM_DOMAINS.ADD_ERROR');
      } finally {
        this.customDomainsLoading.set(false);
      }
    });
  }

  protected removeCustomDomain(domain: CustomDomainResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.CUSTOM_DOMAINS.REMOVE_CONFIRM_TITLE',
        messageKey: 'RESOURCE_EDIT.CUSTOM_DOMAINS.REMOVE_CONFIRM_MESSAGE',
        messageParams: { domain: domain.domainName },
        confirmKey: 'COMMON.DELETE',
        cancelKey: 'COMMON.CANCEL',
      } as ConfirmDialogData,
    });
    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      this.customDomainsLoading.set(true);
      this.customDomainsError.set('');
      try {
        await this.customDomainService.remove(this.resourceId, domain.id);
        this.customDomains.update(list => list.filter(d => d.id !== domain.id));
      } catch {
        this.customDomainsError.set('RESOURCE_EDIT.CUSTOM_DOMAINS.REMOVE_ERROR');
      } finally {
        this.customDomainsLoading.set(false);
      }
    });
  }

}
