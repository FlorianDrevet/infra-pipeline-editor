import { Component, DestroyRef, OnInit, OnDestroy, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { EMPTY, Subscription, catchError, debounceTime, distinctUntilChanged, filter, switchMap, tap } from 'rxjs';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsTabsComponent } from '../../shared/components/ds/ds-tabs/ds-tabs.component';
import type { DsTabDefinition } from '../../shared/components/ds/ds-tabs/ds-tabs.types';
import { LanguageService } from '../../shared/services/language.service';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { KeyVaultService } from '../../shared/services/key-vault.service';
import { RedisCacheService } from '../../shared/services/redis-cache.service';
import { StorageAccountService } from '../../shared/services/storage-account.service';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { RoleAssignmentService } from '../../shared/services/role-assignment.service';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { KeyVaultResponse } from '../../shared/interfaces/key-vault.interface';
import { RedisCacheResponse } from '../../shared/interfaces/redis-cache.interface';
import { StorageAccountResponse, BlobContainerResponse, StorageQueueResponse, StorageTableResponse, CorsRuleEntry, BlobLifecycleRuleEntry } from '../../shared/interfaces/storage-account.interface';
import { AppServicePlanResponse } from '../../shared/interfaces/app-service-plan.interface';
import { WebAppResponse } from '../../shared/interfaces/web-app.interface';
import { FunctionAppResponse } from '../../shared/interfaces/function-app.interface';
import { AppConfigurationResponse } from '../../shared/interfaces/app-configuration.interface';
import { ContainerAppEnvironmentResponse } from '../../shared/interfaces/container-app-environment.interface';
import { ContainerAppResponse } from '../../shared/interfaces/container-app.interface';
import { LogAnalyticsWorkspaceResponse } from '../../shared/interfaces/log-analytics-workspace.interface';
import { ApplicationInsightsResponse } from '../../shared/interfaces/application-insights.interface';
import { CosmosDbResponse } from '../../shared/interfaces/cosmos-db.interface';
import { ServiceBusNamespaceResponse } from '../../shared/interfaces/service-bus-namespace.interface';
import { AcrAuthMode, ContainerRegistryResponse } from '../../shared/interfaces/container-registry.interface';
import { SqlServerResponse } from '../../shared/interfaces/sql-server.interface';
import { SqlDatabaseResponse } from '../../shared/interfaces/sql-database.interface';
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
import { PipelineDetectionService } from '../../shared/services/pipeline-detection.service';
import { DetectedPipelineOptionsResponse } from '../../shared/interfaces/pipeline-detection.interface';
import { EnvironmentNameAvailabilityResponseItem } from '../../shared/interfaces/name-availability.interface';
import { InfrastructureConfigResponse, EnvironmentDefinitionResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectResponse, ProjectPipelineVariableGroupResponse } from '../../shared/interfaces/project.interface';
import { RESOURCE_TYPE_ICONS } from '../../shared/resource-metadata/resource-type.metadata';
import { LOCATION_OPTIONS } from '../../shared/enums/location.enum';
import { OS_TYPE_OPTIONS } from '../../shared/resource-metadata/os-type.metadata';
import { RUNTIME_STACK_OPTIONS } from '../../shared/resource-metadata/runtime-stack.metadata';
import { FUNCTION_APP_RUNTIME_STACK_OPTIONS } from '../../shared/resource-metadata/function-app-runtime-stack.metadata';
import { APP_SERVICE_PLAN_SKU_OPTIONS } from '../../shared/resource-metadata/app-service-plan-sku.metadata';
import { SecureParameterMappingService } from '../../shared/services/secure-parameter-mapping.service';
import { SecureParameterMappingResponse } from '../../shared/interfaces/secure-parameter-mapping.interface';
import { CreateUaiDialogComponent } from './create-uai-dialog/create-uai-dialog.component';
import { PageContextService } from '../../shared/services/page-context.service';
import { DeploymentConfigComponent } from '../../shared/components/deployment-config/deployment-config.component';
import { ResourceEditAppSettingsSectionComponent } from './sections/app-settings/resource-edit-app-settings-section.component';
import { createResourceEditAppSettingsSectionController } from './sections/app-settings/resource-edit-app-settings-section.controller';
import { ResourceEditConfigKeysSectionComponent } from './sections/config-keys/resource-edit-config-keys-section.component';
import { createResourceEditConfigKeysSectionController } from './sections/config-keys/resource-edit-config-keys-section.controller';
import { ResourceEditCustomDomainsSectionComponent } from './sections/custom-domains/resource-edit-custom-domains-section.component';
import { createResourceEditCustomDomainsSectionController } from './sections/custom-domains/resource-edit-custom-domains-section.controller';
import { createResourceEditIdentityAccessSectionController } from './sections/identity-access/resource-edit-identity-access-section.controller';
import { ResourceEditGrantedRightsSectionComponent } from './sections/identity-access/resource-edit-granted-rights-section.component';
import { ResourceEditRoleAssignmentsSectionComponent } from './sections/identity-access/resource-edit-role-assignments-section.component';
import { ResourceEditUsedBySectionComponent } from './sections/identity-access/resource-edit-used-by-section.component';
import { ToggleSectionCardComponent } from '../../shared/components/toggle-section-card/toggle-section-card.component';
import { DsButtonComponent, DsTextFieldComponent, DsSelectComponent, DsSelectOption, DsToggleComponent, DsIconButtonComponent, DsSegmentedControlComponent, DsSegmentedOption, DsTooltipDirective, DsRadioGroupComponent, DsRadioOption } from '../../shared/components/ds';
import { DockerfilePickerComponent } from '../../shared/components/dockerfile-picker/dockerfile-picker.component';
import { BuildContextPickerComponent } from '../../shared/components/build-context-picker/build-context-picker.component';
import { ContainerAppAcrServiceConnectionsComponent } from './components/container-app-acr-service-connections/container-app-acr-service-connections.component';
import { PipelineOptionsComponent } from './components/pipeline-options/pipeline-options.component';
import { NetworkingTabComponent } from './components/networking-tab/networking-tab.component';
import { PipelineStepOptions } from './models/pipeline-step-options.model';
import {
  ResourceEditEnvironmentFormEntry,
  buildAppConfigurationEnvironmentSettings,
  buildAppServicePlanEnvironmentSettings,
  buildApplicationInsightsEnvironmentSettings,
  buildBlobLifecycleRules,
  buildContainerAppEnvironmentResourceSettings,
  buildContainerAppEnvironmentSettings,
  buildContainerRegistryEnvironmentSettings,
  buildCosmosDbEnvironmentSettings,
  buildFunctionAppEnvironmentSettings,
  buildKeyVaultEnvironmentSettings,
  buildLogAnalyticsWorkspaceEnvironmentSettings,
  buildRedisCacheEnvironmentSettings,
  buildServiceBusNamespaceEnvironmentSettings,
  buildSqlDatabaseEnvironmentSettings,
  buildSqlServerEnvironmentSettings,
  buildStorageAccountCorsRules,
  buildStorageAccountEnvironmentSettings,
  buildWebAppEnvironmentSettings,
  toNullableNumber,
} from './helpers/resource-edit-environment-settings.helpers';
import { buildResourceEditEnvironmentForms, buildResourceEditGeneralForm } from './helpers/resource-edit-form-builders.helpers';
import {
  buildCorsErrorKey,
  normalizeCorsHeader,
  normalizeCorsOrigin,
  validateCorsHeader,
  validateCorsOrigin,
  validateStorageCorsRules,
} from './helpers/resource-edit-storage-cors.helpers';
import {
  ACR_PUBLIC_NETWORK_OPTIONS,
  ACR_SKU_OPTIONS,
  AI_INGESTION_MODE_OPTIONS,
  AI_RETENTION_OPTIONS,
  APP_CONFIGURATION_PUBLIC_NETWORK_OPTIONS,
  APP_CONFIGURATION_SKU_OPTIONS,
  CA_MEMORY_OPTIONS,
  CA_TRANSPORT_OPTIONS,
  CA_CPU_OPTIONS,
  CAE_SKU_OPTIONS,
  CAE_WORKLOAD_PROFILE_OPTIONS,
  COSMOS_API_TYPE_OPTIONS,
  COSMOS_BACKUP_POLICY_OPTIONS,
  COSMOS_CONSISTENCY_LEVEL_OPTIONS,
  FUNCTIONAPP_RUNTIME_VERSION_MAP,
  KEY_VAULT_SKU_OPTIONS,
  LAW_SKU_OPTIONS,
  REDIS_EVICTION_OPTIONS,
  REDIS_SKU_OPTIONS,
  REDIS_TLS_OPTIONS,
  REDIS_VERSION_OPTIONS,
  SQL_MIN_TLS_OPTIONS,
  SQL_SERVER_VERSION_OPTIONS,
  STORAGE_ACCESS_TIER_OPTIONS,
  STORAGE_CORS_ALLOWED_HEADER_SUGGESTIONS,
  STORAGE_CORS_EXPOSED_HEADER_SUGGESTIONS,
  STORAGE_CORS_MAX_AGE_PRESETS,
  STORAGE_CORS_METHOD_OPTIONS,
  STORAGE_KIND_OPTIONS,
  STORAGE_SKU_OPTIONS,
  STORAGE_TLS_OPTIONS,
  WEBAPP_RUNTIME_VERSION_MAP,
} from './resource-edit.constants';

/** Union type for any loaded resource */
type ResourceData = KeyVaultResponse | RedisCacheResponse | StorageAccountResponse | AppServicePlanResponse | WebAppResponse | FunctionAppResponse | UserAssignedIdentityResponse | AppConfigurationResponse | ContainerAppEnvironmentResponse | ContainerAppResponse | LogAnalyticsWorkspaceResponse | ApplicationInsightsResponse | CosmosDbResponse | ServiceBusNamespaceResponse | ContainerRegistryResponse | SqlServerResponse | SqlDatabaseResponse;

type CorsServiceKey = 'blob' | 'table';
type CorsListField = 'allowedOrigins' | 'allowedHeaders' | 'exposedHeaders';
type CorsMethodField = 'allowedMethods';
type CorsFieldKey = CorsListField | CorsMethodField | 'maxAgeInSeconds';

const RESOURCE_EDIT_MAIN_TAB_IDS = [
  'general',
  'environments',
  'identity-access',
  'networking',
  'storage',
  'app-settings',
  'config-keys',
  'granted-rights',
  'used-by',
  'app-pipeline',
] as const;
type MainTabId = typeof RESOURCE_EDIT_MAIN_TAB_IDS[number];
type StorageSubTabId = 'blob_containers' | 'queues' | 'tables';

@Component({
  selector: 'app-resource-edit',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    DsSpinnerComponent,
    DsTabsComponent,
    DsRadioGroupComponent,
    DsToggleComponent,
    MatTooltipModule,
    MatExpansionModule,
    DeploymentConfigComponent,
    ResourceEditAppSettingsSectionComponent,
    ResourceEditConfigKeysSectionComponent,
    ResourceEditCustomDomainsSectionComponent,
    ResourceEditGrantedRightsSectionComponent,
    ResourceEditRoleAssignmentsSectionComponent,
    ResourceEditUsedBySectionComponent,
    ToggleSectionCardComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    DsSegmentedControlComponent,
    DsTooltipDirective,
    DsTextFieldComponent,
    DockerfilePickerComponent,
    BuildContextPickerComponent,
    DsSelectComponent,
    ContainerAppAcrServiceConnectionsComponent,
    PipelineOptionsComponent,
    NetworkingTabComponent,
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
  private readonly secureParamMappingService = inject(SecureParameterMappingService);
  private readonly nameAvailabilityService = inject(NameAvailabilityService);
  private readonly pipelineDetectionService = inject(PipelineDetectionService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly translate = inject(TranslateService);
  private readonly languageService = inject(LanguageService);
  private readonly pageContextService = inject(PageContextService);

  // ─── Route params ───
  protected configId = '';
  protected resourceType = '';
  protected resourceId = '';

  // ─── State ───
  protected readonly resource = signal<ResourceData | null>(null);
  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly customDomainsSection = createResourceEditCustomDomainsSectionController({
    getResourceId: () => this.resourceId,
    getResourceType: () => this.resourceType,
    getEnvironments: () => this.environments(),
  });
  protected readonly isLoading = signal(false);
  protected readonly isSaving = signal(false);
  protected readonly loadError = signal('');
  protected readonly saveError = signal('');
  protected readonly formsDirty = signal(false);
  private formSubscriptions: Subscription[] = [];
  protected readonly saveSuccess = signal(false);

  // ─── Storage Services ───
  protected readonly activeStorageSubTabId = signal<StorageSubTabId>('blob_containers');
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

  private static readonly PE_SUPPORTED_TYPES = new Set<string>([
    'KeyVault', 'StorageAccount', 'AppConfiguration', 'CosmosDb', 'SqlServer',
    'RedisCache', 'ServiceBusNamespace', 'EventHubNamespace', 'ContainerRegistry',
    'WebApp', 'FunctionApp', 'ApplicationInsights', 'LogAnalyticsWorkspace',
  ]);
  protected readonly supportsNetworking = computed(() => ResourceEditComponent.PE_SUPPORTED_TYPES.has(this.resourceType));
  protected readonly isExistingResource = computed(() => (this.resource() as { isExisting?: boolean } | null)?.isExisting === true);

  // ─── App Pipeline ───

  private static readonly CONTAINER_APP_RESOURCE_TYPE = 'ContainerApp';
  private static readonly ACR_ADMIN_CREDENTIALS_AUTH_MODE: AcrAuthMode = 'AdminCredentials';

  protected readonly pipelineStepOptions = signal<PipelineStepOptions | null>(null);

  protected readonly showContainerAppAcrServiceConnections = computed(() =>
    this.resourceType === ResourceEditComponent.CONTAINER_APP_RESOURCE_TYPE &&
    !this.isExistingResource() &&
    !!this.selectedContainerRegistryId() &&
    this.acrAuthMode() !== ResourceEditComponent.ACR_ADMIN_CREDENTIALS_AUTH_MODE,
  );

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

  // ─── Identity Access ───
  protected readonly identityAccessSection = createResourceEditIdentityAccessSectionController({
    getConfigId: () => this.configId,
    getConfig: () => this.config(),
    getResourceId: () => this.resourceId,
    getResource: () => this.resource(),
    isUserAssignedIdentity: () => this.isUserAssignedIdentity(),
    isAcrEnabled: () => this.isAcrEnabled(),
    checkAcrPullAccess: () => this.checkAcrPullAccess(),
    getAcrPullIdentityId: () => this.getCurrentAcrPullIdentityId(),
    supportsAppSettings: () => this.supportsAppSettings(),
    reloadAppSettings: () => this.appSettingsSection.load(),
    supportsConfigKeys: () => this.supportsConfigKeys(),
    reloadConfigKeys: () => this.configKeysSection.load(),
  });

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

  /** True when name availability blocks saving (invalid always blocks, unavailable/checking can be overridden). */
  protected readonly isSaveBlockedByNameAvailability = computed(() => {
    if (!this.isNameAvailabilityCheckEnabled()) return false;
    const state = this.nameAvailabilityOverallState();
    // Invalid names always block — no override possible (naming constraints violation)
    if (state === 'has-invalid') return true;
    // Unavailable names (conflict) can be overridden ("it's my resource")
    if (state === 'has-unavailable') {
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

  protected readonly environmentsTabHasWarning = computed(() => {
    const forms = this.envForms();
    if (forms.length === 0) return false;
    return forms.some(ef => {
      const controls = ef.form.controls;
      return Object.keys(controls).length > 0 && Object.values(controls).every(c => c.value === null || c.value === '');
    });
  });

  // ─── App Settings ───
  protected readonly supportsAppSettings = computed(() =>
    ['WebApp', 'FunctionApp', 'ContainerApp'].includes(this.resourceType)
  );

  // ─── Custom Domains ───
  protected readonly supportsCustomDomains = computed(() =>
    ['WebApp', 'FunctionApp', 'ContainerApp'].includes(this.resourceType)
  );
  protected readonly appSettingsSection = createResourceEditAppSettingsSectionController({
    getResourceId: () => this.resourceId,
    getResourceName: () => this.resource()?.name ?? '',
    getResourceType: () => this.resourceType,
    getProjectId: () => this.config()?.projectId ?? '',
    getEnvironments: () => this.environments(),
    getAllResources: () => this.identityAccessSection.allResources(),
    getAssignedUai: () => this.identityAccessSection.assignedUai(),
    getUaiOptions: () => this.identityAccessSection.uaiOptions(),
    getResourceContext: () => {
      const resource = this.resource();
      if (!resource) {
        return null;
      }

      return {
        resourceGroupId: resource.resourceGroupId ?? '',
        location: resource.location ?? 'EastUS2',
      };
    },
    getDeploymentMode: () => (this.generalForm?.get('deploymentMode')?.value as string | null | undefined) ?? this.deploymentMode(),
    getRuntimeStack: () => (this.generalForm?.get('runtimeStack')?.value as string | null | undefined) ?? null,
    reloadRoleAssignments: () => this.identityAccessSection.loadRoleAssignments(),
    reloadAllResources: () => this.identityAccessSection.loadAllResources(),
  });

  // ─── Configuration Keys (AppConfiguration) ───
  protected readonly supportsConfigKeys = computed(() => this.resourceType === 'AppConfiguration');
  protected readonly configKeysSection = createResourceEditConfigKeysSectionController({
    getResourceId: () => this.resourceId,
    getProjectId: () => this.config()?.projectId ?? '',
    getEnvironments: () => this.environments(),
    getAllResources: () => this.identityAccessSection.allResources(),
    getAssignedUai: () => this.identityAccessSection.assignedUai(),
    getUaiOptions: () => this.identityAccessSection.uaiOptions(),
    getResourceContext: () => {
      const resource = this.resource();
      if (!resource) {
        return null;
      }

      return {
        resourceGroupId: resource.resourceGroupId ?? '',
        location: resource.location ?? 'EastUS2',
      };
    },
    reloadRoleAssignments: () => this.identityAccessSection.loadRoleAssignments(),
    reloadAllResources: () => this.identityAccessSection.loadAllResources(),
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

  /** ServiceBus SKU options (used inline in env tab) */
  protected readonly serviceBusSkuOptions: DsSelectOption[] = [
    { label: 'Basic', value: 'Basic' },
    { label: 'Standard', value: 'Standard' },
    { label: 'Premium', value: 'Premium' },
  ];

  /** ServiceBus minimum TLS options (used inline in env tab) */
  protected readonly serviceBusMinTlsOptions: DsSelectOption[] = [
    { label: 'TLS 1.0', value: '1.0' },
    { label: 'TLS 1.1', value: '1.1' },
    { label: 'TLS 1.2', value: '1.2' },
  ];

  /** Translated public access options for the DS select */
  protected readonly publicAccessSelectOptions = computed<DsSelectOption[]>(() =>
    this.publicAccessOptions.map(o => ({ value: o.value, label: this.translate.instant(o.label) })),
  );

  /** SQL Server options computed from allResources for SqlDatabase picker */
  protected readonly sqlServerOptionsForSelect = computed<DsSelectOption[]>(() =>
    this.identityAccessSection.allResources()
      .filter(r => r.resourceType === 'SqlServer')
      .map(r => ({ value: r.id, label: r.name })),
  );

  /** Log Analytics Workspace options with explicit None entry */
  protected readonly lawOptionsForSelect = computed<DsSelectOption[]>(() => [
    { value: null, label: this.translate.instant('RESOURCE_EDIT.GENERAL.NONE') },
    ...this.identityAccessSection.availableLogAnalyticsWorkspaces().map(law => ({ value: law.id, label: law.name })),
  ]);

  /** Variable Group options for the SQL password selector, with a sentinel "create new" entry. */
  protected readonly passwordVgSelectOptions = computed<DsSelectOption[]>(() => [
    ...this.passwordVgOptions().map(vg => ({ value: vg.id, label: vg.groupName })),
    { value: '__create_new__', label: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.CREATE_NEW_GROUP') },
  ]);

  /** Segmented control options for the new Variable Group scope (project vs configuration). */
  protected readonly passwordNewGroupScopeOptions = computed<DsSegmentedOption[]>(() => [
    { value: 'project', label: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.SCOPE_PROJECT'), icon: 'folder_shared' },
    { value: 'configuration', label: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.SCOPE_CONFIGURATION'), icon: 'settings' },
  ]);

  // ─── Secure Parameter Mappings (SqlServer password config) ───
  protected readonly secureParamMappings = signal<SecureParameterMappingResponse[]>([]);
  protected readonly passwordMode = signal<'random' | 'variableGroup'>('random');

  /** Radio options for the password mode selector. */
  protected readonly passwordModeOptions = computed<DsRadioOption[]>(() => [
    {
      value: 'random',
      label: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.RANDOM'),
      description: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.RANDOM_HINT'),
    },
    {
      value: 'variableGroup',
      label: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.FROM_VARIABLE_GROUP'),
      description: this.translate.instant('RESOURCE_EDIT.SECURE_PARAM.FROM_VARIABLE_GROUP_HINT'),
    },
  ]);
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
  protected envForms = signal<ResourceEditEnvironmentFormEntry[]>([]);

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

  // ─── Main tab state (DS tabs) ───
  protected readonly activeMainTabId = signal<MainTabId>('general');

  protected readonly mainTabs = computed<readonly DsTabDefinition[]>(() => {
    this.languageService.currentLanguage();
    const t = (k: string): string => this.translate.instant(k) as string;
    const tabs: DsTabDefinition[] = [
      { id: 'general', label: t('RESOURCE_EDIT.TABS.GENERAL'), icon: 'settings' },
    ];
    if (!this.isUserAssignedIdentity() && !this.isExistingResource()) {
      tabs.push({
        id: 'environments',
        label: t('RESOURCE_EDIT.TABS.ENVIRONMENTS'),
        icon: 'cloud_queue',
        badge: String(this.environments().length),
      });
    }
    if (!this.isUserAssignedIdentity()) {
      const count = this.identityAccessSection.roleAssignments().length;
      tabs.push({
        id: 'identity-access',
        label: t('RESOURCE_EDIT.TABS.IDENTITY_ACCESS'),
        icon: 'security',
        badge: count > 0 ? String(count) : undefined,
      });
    }
    if (this.supportsNetworking()) {
      tabs.push({ id: 'networking', label: t('RESOURCE_EDIT.TABS.NETWORKING'), icon: 'lan' });
    }
    if (this.isStorageAccount()) {
      tabs.push({
        id: 'storage',
        label: t('RESOURCE_EDIT.TABS.STORAGE_SERVICES'),
        icon: 'storage',
        badge: String(this.storageBlobContainers().length + this.storageQueues().length + this.storageTables().length),
      });
    } else if (this.supportsAppSettings()) {
      const count = this.appSettingsSection.appSettings().length;
      tabs.push({
        id: 'app-settings',
        label: t('RESOURCE_EDIT.TABS.APP_SETTINGS'),
        icon: 'data_object',
        badge: count > 0 ? String(count) : undefined,
      });
    } else if (this.supportsConfigKeys()) {
      const count = this.configKeysSection.configKeys().length;
      tabs.push({
        id: 'config-keys',
        label: t('RESOURCE_EDIT.TABS.CONFIG_KEYS'),
        icon: 'settings',
        badge: count > 0 ? String(count) : undefined,
      });
    } else if (this.isUserAssignedIdentity()) {
      const count = this.identityAccessSection.identityRoleAssignments().length;
      tabs.push({
        id: 'granted-rights',
        label: t('RESOURCE_EDIT.TABS.GRANTED_RIGHTS'),
        icon: 'verified_user',
        badge: count > 0 ? String(count) : undefined,
      });
      const usedByCount = this.identityAccessSection.usedByResources().length;
      tabs.push({
        id: 'used-by',
        label: t('RESOURCE_EDIT.TABS.USED_BY'),
        icon: 'device_hub',
        badge: usedByCount > 0 ? String(usedByCount) : undefined,
      });
    }
    if (this.supportsAppPipeline() && !this.isExistingResource()) {
      tabs.push({ id: 'app-pipeline', label: t('RESOURCE_EDIT.TABS.APP_PIPELINE'), icon: 'terminal' });
    }
    return tabs;
  });

  protected readonly storageSubTabs = computed<readonly DsTabDefinition[]>(() => {
    this.languageService.currentLanguage();
    const t = (k: string): string => this.translate.instant(k) as string;
    return [
      { id: 'blob_containers', label: t('RESOURCE_EDIT.STORAGE_SERVICES.BLOB_CONTAINERS'), icon: 'folder', badge: String(this.storageBlobContainers().length) },
      { id: 'queues', label: t('RESOURCE_EDIT.STORAGE_SERVICES.QUEUES'), icon: 'queue', badge: String(this.storageQueues().length) },
      { id: 'tables', label: t('RESOURCE_EDIT.STORAGE_SERVICES.TABLES'), icon: 'table_chart', badge: String(this.storageTables().length) },
    ];
  });

  protected onMainTabChange(tabId: string): void {
    if (RESOURCE_EDIT_MAIN_TAB_IDS.includes(tabId as MainTabId)) {
      this.activeMainTabId.set(tabId as MainTabId);
    }
  }

  protected onStorageSubTabChange(tabId: string): void {
    if (tabId === 'blob_containers' || tabId === 'queues' || tabId === 'tables') {
      this.activeStorageSubTabId.set(tabId);
    }
  }

  // ─── Save bar visibility (only on saveable tabs when dirty) ───
  protected readonly showSaveBar = computed(() => {
    const tabId = this.activeMainTabId();
    const isOnSaveableTab = this.isUserAssignedIdentity()
      ? tabId === 'general'
      : tabId === 'general' || tabId === 'environments' || (this.isStorageAccount() && tabId === 'storage') || (this.supportsAppPipeline() && tabId === 'app-pipeline');
    return this.formsDirty() && isOnSaveableTab && this.canWrite();
  });

  // ─── Breadcrumb (top-bar) ───
  // Wave 5: Projects > ProjectName > ConfigName > ResourceType : ResourceName.
  private readonly breadcrumbEffect = effect(() => {
    const project = this.project();
    const config = this.config();
    const resource = this.resource();
    const projectsLabel = this.translate.instant('NAV.BREADCRUMB.PROJECTS') as string;
    const segments: { label: string; routerLink?: string }[] = [
      { label: projectsLabel, routerLink: '/' },
    ];
    if (project) {
      segments.push({ label: project.name, routerLink: `/projects/${project.id}` });
    }
    if (config) {
      segments.push({ label: config.name, routerLink: `/config/${config.id}` });
    }
    if (resource) {
      const typeLabel = this.resourceType || '';
      const last = typeLabel ? `${typeLabel} : ${resource.name}` : resource.name;
      segments.push({ label: last });
    }
    this.pageContextService.setBreadcrumb(segments);
  });

  ngOnInit(): void {
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
      this.activeMainTabId.set('storage');
      if (tab === 'blob_containers' || tab === 'queues' || tab === 'tables') {
        this.activeStorageSubTabId.set(tab);
      }
    }

    void this.loadData();
  }

  private async loadData(): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      // Load config + resource in parallel
      const [config, resource] = await Promise.all([
        this.infraConfigService.getById(this.configId),
        this.loadResource(),
      ]);
      this.config.set(config);
      this.resource.set(resource);

      // Start background loads immediately — they only need configId/resourceId, not project
      void this.identityAccessSection.loadRoleAssignments();
      void this.identityAccessSection.loadAllResources();
      if (this.supportsAppSettings()) {
        void this.appSettingsSection.load();
      }
      if (this.supportsCustomDomains()) {
        void this.customDomainsSection.load();
      }
      if (this.supportsConfigKeys()) {
        void this.configKeysSection.load();
      }
      if (this.isUserAssignedIdentity()) {
        void this.identityAccessSection.loadIdentityRoleAssignments();
      }
      if (this.resourceType === 'SqlServer') {
        this.loadSecureParamMappings();
      }

      // Load parent project (required for environment forms + permissions)
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

      // Initialize runtime version options for WebApp/FunctionApp
      if (this.resourceType === 'WebApp') {
        const stack = this.generalForm.get('runtimeStack')?.value;
        this.runtimeVersionOptions.set(WEBAPP_RUNTIME_VERSION_MAP[stack] ?? []);
      } else if (this.resourceType === 'FunctionApp') {
        const stack = this.generalForm.get('runtimeStack')?.value;
        this.runtimeVersionOptions.set(FUNCTIONAPP_RUNTIME_VERSION_MAP[stack] ?? []);
      } else {
        this.runtimeVersionOptions.set([]);
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
        throw new Error(`Unsupported resource type: ${this.resourceType}`);
    }
  }

  private buildGeneralForm(resource: ResourceData): void {
    const buildResult = buildResourceEditGeneralForm({
      fb: this.fb,
      resourceType: this.resourceType,
      resource,
      resolveAcrAuthMode: (containerRegistryId, acrAuthMode) => this.resolveAcrAuthMode(containerRegistryId, acrAuthMode),
    });

    this.generalForm = buildResult.form;
    this.deploymentMode.set(buildResult.deploymentMode);
    this.selectedContainerRegistryId.set(buildResult.selectedContainerRegistryId);
    this.setAcrAuthMode(buildResult.acrAuthMode);
    this.storageCorsRulesDraft.set(buildResult.storageCorsRulesDraft);
    this.storageTableCorsRulesDraft.set(buildResult.storageTableCorsRulesDraft);
    this.lifecycleRulesDraft.set(buildResult.lifecycleRulesDraft);
    this.acrSelectedUaiId.set(this.getCurrentAcrPullIdentityId());

    // Hydrate pipeline step options for compute resources
    if (this.supportsAppPipeline()) {
      const res = resource as { pipelineStepOptions?: PipelineStepOptions | null };
      this.pipelineStepOptions.set(res.pipelineStepOptions ?? null);
    }
  }

  private buildEnvForms(resource: ResourceData): void {
    this.envForms.set(
      buildResourceEditEnvironmentForms(this.fb, this.resourceType, resource, this.environments()),
    );
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

  /** User confirms the unavailable name is theirs and overrides the save block. */
  protected overrideNameAvailability(): void {
    this.nameAvailabilityOverridden.set(true);
  }

  protected onPipelineOptionsChanged(options: PipelineStepOptions): void {
    this.pipelineStepOptions.set(options);
    this.formsDirty.set(true);
  }

  protected async onDetectPipelineOptions(): Promise<void> {
    try {
      const result = await this.pipelineDetectionService.detect(this.resourceId);
      const patch = this.buildPipelineDetectionPatch(result);

      const merged = { ...(this.pipelineStepOptions() ?? {}), ...patch } as PipelineStepOptions;
      this.pipelineStepOptions.set(merged);
      this.formsDirty.set(true);
    } catch {
      // Detection failed silently — user can configure manually
    }
  }

  private buildPipelineDetectionPatch(result: DetectedPipelineOptionsResponse): Partial<PipelineStepOptions> {
    const patch: Partial<PipelineStepOptions> = {};

    if (result.testFramework) {
      patch.runUnitTests = true;
      patch.testFramework = result.testFramework;
    }
    if (result.suggestedTestCommand) {
      patch.testCommand = result.suggestedTestCommand;
    }
    if (result.suggestedTestResultsFormat) {
      patch.testResultsFormat = result.suggestedTestResultsFormat;
      patch.publishTestResults = true;
    }
    if (result.suggestedCoverageTool) {
      patch.publishCodeCoverage = true;
      patch.coverageTool = result.suggestedCoverageTool;
    }
    if (result.suggestedCoverageReportPath) {
      patch.coverageReportPath = result.suggestedCoverageReportPath;
    }
    if (result.lintingAvailable) {
      patch.runLinting = true;
    }
    if (result.suggestedLintCommand) {
      patch.lintCommand = result.suggestedLintCommand;
    }
    if (result.sonarConfigDetected) {
      patch.runSonarAnalysis = true;
    }
    if (result.suggestedSonarProjectKey) {
      patch.sonarProjectKey = result.suggestedSonarProjectKey;
    }
    if (result.dependencyScanAvailable) {
      patch.runDependencyScan = true;
    }
    if (result.suggestedDependencyScanTool) {
      patch.dependencyScanTool = result.suggestedDependencyScanTool;
    }

    return patch;
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

  private getCurrentAcrPullIdentityId(): string | null {
    const identityId = this.generalForm?.get('acrPullIdentityId')?.value;
    return typeof identityId === 'string' && identityId.length > 0 ? identityId : null;
  }

  private resolveAcrPullIdentityId(containerRegistryId: string | null, acrAuthMode: AcrAuthMode | null): string | null {
    return containerRegistryId && acrAuthMode === 'ManagedIdentity' ? this.getCurrentAcrPullIdentityId() : null;
  }

  private patchAcrPullIdentityId(identityId: string | null, emitEvent = true): void {
    this.acrSelectedUaiId.set(identityId);
    this.generalForm.get('acrPullIdentityId')?.setValue(identityId, { emitEvent });
  }

  private resetAcrPullAccessState(preserveSelectedIdentity = false): void { // NOSONAR S3776 - tracked under test-debt #22
    this.acrHasAccess.set(null);
    this.acrMissingRoleName.set(null);
    this.acrMissingRoleDefinitionId.set(null);
    this.acrAssignedUaiId.set(null);
    this.acrAssignedUaiName.set(null);
    this.acrHasUai.set(false);
    if (!preserveSelectedIdentity) {
      this.acrSelectedUaiId.set(null);
    }
  }

  protected async onSave(): Promise<void> {
    if (this.generalForm.invalid || this.isSaving() || this.isSaveBlockedByAcr() || this.isSaveBlockedByNameAvailability()) return;

    this.isSaving.set(true);
    this.saveError.set('');
    this.saveSuccess.set(false);

    const general = this.generalForm.getRawValue();
    const envForms = this.envForms();

    try {
      let updated: ResourceData;

      switch (this.resourceType) {
        case 'KeyVault':
          updated = await this.keyVaultService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            enableRbacAuthorization: general.enableRbacAuthorization,
            enabledForDeployment: general.enabledForDeployment,
            enabledForDiskEncryption: general.enabledForDiskEncryption,
            enabledForTemplateDeployment: general.enabledForTemplateDeployment,
            enablePurgeProtection: general.enablePurgeProtection,
            enableSoftDelete: general.enableSoftDelete,
            environmentSettings: buildKeyVaultEnvironmentSettings(envForms),
          });
          break;
        case 'RedisCache':
          updated = await this.redisCacheService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            redisVersion: toNullableNumber(general.redisVersion),
            enableNonSslPort: general.enableNonSslPort ?? false,
            minimumTlsVersion: general.minimumTlsVersion || null,
            disableAccessKeyAuthentication: general.disableAccessKeyAuthentication ?? false,
            enableAadAuth: general.enableAadAuth ?? false,
            environmentSettings: buildRedisCacheEnvironmentSettings(envForms),
          });
          break;
        case 'StorageAccount':
          if (!this.validateAllStorageCorsRules()) {
            this.saveError.set('RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.FIX_VALIDATION_ERRORS');
            this.isSaving.set(false);
            return;
          }

          updated = await this.storageAccountService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            kind: general.kind,
            accessTier: general.accessTier,
            allowBlobPublicAccess: general.allowBlobPublicAccess ?? false,
            enableHttpsTrafficOnly: general.enableHttpsTrafficOnly ?? true,
            minimumTlsVersion: general.minimumTlsVersion,
            corsRules: buildStorageAccountCorsRules(this.storageCorsRulesDraft()),
            tableCorsRules: buildStorageAccountCorsRules(this.storageTableCorsRulesDraft()),
            lifecycleRules: buildBlobLifecycleRules(this.lifecycleRulesDraft()),
            environmentSettings: buildStorageAccountEnvironmentSettings(envForms),
          });
          break;
        case 'AppServicePlan':
          updated = await this.appServicePlanService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            osType: general.osType,
            environmentSettings: buildAppServicePlanEnvironmentSettings(envForms),
          });
          break;
        case 'WebApp': {
          const acrAuthMode = general.deploymentMode === 'Container'
            ? this.resolveAcrAuthMode(general.containerRegistryId || null, general.acrAuthMode as AcrAuthMode | null | undefined)
            : null;

          updated = await this.webAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            appServicePlanId: general.appServicePlanId,
            deploymentMode: general.deploymentMode || 'Code',
            containerRegistryId: general.deploymentMode === 'Container' ? (general.containerRegistryId || null) : null,
            acrAuthMode,
            acrPullIdentityId: general.deploymentMode === 'Container' && acrAuthMode === 'ManagedIdentity' ? this.resolveAcrPullIdentityId(general.containerRegistryId, acrAuthMode) : null,
            dockerImageName: general.deploymentMode === 'Container' ? (general.dockerImageName || null) : null,
            dockerImageValidated: general.deploymentMode === 'Container' ? (general.dockerImageValidated ?? false) : false,
            dockerfilePath: general.dockerfilePath || null,
            sourceCodePath: general.sourceCodePath || null,
            buildCommand: general.buildCommand || null,
            applicationName: general.applicationName || null,
            runtimeStack: general.runtimeStack,
            runtimeVersion: general.runtimeVersion,
            alwaysOn: general.alwaysOn,
            httpsOnly: general.httpsOnly,
            pipelineStepOptions: this.pipelineStepOptions(),
            environmentSettings: buildWebAppEnvironmentSettings(envForms),
          });
          break;
        }
        case 'FunctionApp': {
          const acrAuthMode = general.deploymentMode === 'Container'
            ? this.resolveAcrAuthMode(general.containerRegistryId || null, general.acrAuthMode as AcrAuthMode | null | undefined)
            : null;

          updated = await this.functionAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            appServicePlanId: general.appServicePlanId,
            deploymentMode: general.deploymentMode || 'Code',
            containerRegistryId: general.deploymentMode === 'Container' ? (general.containerRegistryId || null) : null,
            acrAuthMode,
            acrPullIdentityId: general.deploymentMode === 'Container' && acrAuthMode === 'ManagedIdentity' ? this.resolveAcrPullIdentityId(general.containerRegistryId, acrAuthMode) : null,
            dockerImageName: general.deploymentMode === 'Container' ? (general.dockerImageName || null) : null,
            dockerImageValidated: general.deploymentMode === 'Container' ? (general.dockerImageValidated ?? false) : false,
            dockerfilePath: general.dockerfilePath || null,
            sourceCodePath: general.sourceCodePath || null,
            buildCommand: general.buildCommand || null,
            applicationName: general.applicationName || null,
            runtimeStack: general.runtimeStack,
            runtimeVersion: general.runtimeVersion,
            httpsOnly: general.httpsOnly,
            pipelineStepOptions: this.pipelineStepOptions(),
            environmentSettings: buildFunctionAppEnvironmentSettings(envForms),
          });
          break;
        }
        case 'UserAssignedIdentity':
          updated = await this.userAssignedIdentityService.update(this.resourceId, {
            name: general.name,
            location: general.location,
          });
          break;
        case 'AppConfiguration':
          updated = await this.appConfigurationService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: buildAppConfigurationEnvironmentSettings(envForms),
          });
          break;
        case 'ContainerAppEnvironment':
          updated = await this.containerAppEnvironmentService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            logAnalyticsWorkspaceId: general.logAnalyticsWorkspaceId || null,
            environmentSettings: buildContainerAppEnvironmentResourceSettings(envForms),
          });
          break;
        case 'ContainerApp': {
          const acrAuthMode = this.resolveAcrAuthMode(general.containerRegistryId || null, general.acrAuthMode as AcrAuthMode | null | undefined);

          updated = await this.containerAppService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            containerAppEnvironmentId: general.containerAppEnvironmentId,
            containerRegistryId: general.containerRegistryId || null,
            acrAuthMode,
            acrPullIdentityId: general.containerRegistryId && acrAuthMode === 'ManagedIdentity' ? this.resolveAcrPullIdentityId(general.containerRegistryId, acrAuthMode) : null,
            dockerImageName: general.dockerImageName || null,
            dockerImageValidated: general.dockerImageValidated ?? false,
            dockerfilePath: general.dockerfilePath || null,
            sourceCodePath: general.sourceCodePath || null,
            applicationName: general.applicationName || null,
            pipelineStepOptions: this.pipelineStepOptions(),
            environmentSettings: buildContainerAppEnvironmentSettings(envForms),
          });
          break;
        }
        case 'LogAnalyticsWorkspace':
          updated = await this.logAnalyticsWorkspaceService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: buildLogAnalyticsWorkspaceEnvironmentSettings(envForms),
          });
          break;
        case 'ApplicationInsights':
          updated = await this.applicationInsightsService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            logAnalyticsWorkspaceId: general.logAnalyticsWorkspaceId,
            environmentSettings: buildApplicationInsightsEnvironmentSettings(envForms),
          });
          break;
        case 'CosmosDb':
          updated = await this.cosmosDbService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: buildCosmosDbEnvironmentSettings(envForms),
          });
          break;
        case 'ServiceBusNamespace':
          updated = await this.serviceBusNamespaceService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: buildServiceBusNamespaceEnvironmentSettings(envForms),
          });
          break;
        case 'ContainerRegistry':
          updated = await this.containerRegistryService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            environmentSettings: buildContainerRegistryEnvironmentSettings(envForms),
          });
          break;
        case 'SqlServer':
          updated = await this.sqlServerService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            version: general.version,
            administratorLogin: general.administratorLogin,
            environmentSettings: buildSqlServerEnvironmentSettings(envForms),
          });
          break;
        case 'SqlDatabase':
          updated = await this.sqlDatabaseService.update(this.resourceId, {
            name: general.name,
            location: general.location,
            sqlServerId: general.sqlServerId,
            collation: general.collation ?? 'SQL_Latin1_General_CP1_CI_AS',
            environmentSettings: buildSqlDatabaseEnvironmentSettings(envForms),
          });
          break;
        default:
          throw new Error(`Unsupported resource type: ${this.resourceType}`);
      }

      // Use the response directly — no redundant GET needed
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

    if (mode === 'Code') {
      // Clear ACR pull identity when switching to code mode
      this.patchAcrPullIdentityId(null);
    }

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
    const previousRegistryId = this.selectedContainerRegistryId() ?? this.generalForm.get('containerRegistryId')?.value ?? null;
    const registryChanged = previousRegistryId !== acrId;
    const nextAcrAuthMode = this.resolveAcrAuthMode(acrId, this.acrAuthMode() ?? (this.generalForm.get('acrAuthMode')?.value as AcrAuthMode | null | undefined));

    this.selectedContainerRegistryId.set(acrId);
    this.generalForm.patchValue({ containerRegistryId: acrId });
    this.acrAccessChecking.set(false);
    this.resetAcrPullAccessState();
    this.setAcrAuthMode(nextAcrAuthMode, true);

    if (registryChanged || !acrId || nextAcrAuthMode !== 'ManagedIdentity') {
      this.patchAcrPullIdentityId(null);
    }

    if (!acrId || nextAcrAuthMode !== 'ManagedIdentity') return;
    await this.checkAcrPullAccess(acrId, nextAcrAuthMode);
  }

  protected async onAcrAuthModeChange(mode: AcrAuthMode): Promise<void> {
    const currentRegistryId = this.selectedContainerRegistryId() ?? this.generalForm.get('containerRegistryId')?.value ?? null;
    const nextAcrAuthMode = this.resolveAcrAuthMode(currentRegistryId, mode);

    this.acrAccessChecking.set(false);
    this.resetAcrPullAccessState();
    this.setAcrAuthMode(nextAcrAuthMode, true);

    if (nextAcrAuthMode !== 'ManagedIdentity') {
      // Clear ACR pull identity when not in managed identity mode
      this.patchAcrPullIdentityId(null);
    }

    if (!currentRegistryId || nextAcrAuthMode !== 'ManagedIdentity') return;
    await this.checkAcrPullAccess(currentRegistryId, nextAcrAuthMode);
  }

  protected async onAcrSelectedUaiChange(identityId: string | null): Promise<void> {
    this.patchAcrPullIdentityId(identityId);
    this.formsDirty.set(true);

    const registryId = this.selectedContainerRegistryId() ?? this.generalForm.get('containerRegistryId')?.value ?? null;
    const currentAuthMode = this.acrAuthMode();

    if (!registryId || currentAuthMode !== 'ManagedIdentity') {
      this.resetAcrPullAccessState(true);
      return;
    }

    if (!identityId) {
      this.resetAcrPullAccessState(true);
      this.acrHasAccess.set(false);
      return;
    }

    await this.checkAcrPullAccess(registryId, currentAuthMode, identityId);
  }

  protected async checkAcrPullAccess(acrId?: string, acrAuthMode?: AcrAuthMode | null, acrPullIdentityId?: string | null): Promise<void> {
    const registryId = acrId ?? this.generalForm.get('containerRegistryId')?.value;
    const requestedAcrAuthMode = this.resolveAcrAuthMode(registryId, acrAuthMode ?? this.acrAuthMode());
    const selectedIdentityId = acrPullIdentityId ?? this.acrSelectedUaiId() ?? this.getCurrentAcrPullIdentityId();

    if (!registryId || requestedAcrAuthMode !== 'ManagedIdentity') {
      this.resetAcrPullAccessState();
      return;
    }

    this.acrAuthMode.set(requestedAcrAuthMode);
    this.acrAccessChecking.set(true);
    this.resetAcrPullAccessState(true);

    try {
      const result = await this.containerRegistryService.checkAcrPullAccess(
        this.resourceId,
        registryId,
        requestedAcrAuthMode,
        selectedIdentityId,
      );
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

      if (requestedAcrAuthMode === 'ManagedIdentity' && result.assignedUserAssignedIdentityId) {
        this.patchAcrPullIdentityId(result.assignedUserAssignedIdentityId, false);
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
    const uaiId = this.acrSelectedUaiId();
    if (!acrId || !roleDefId || !uaiId) return;

    this.acrRoleAssigning.set(true);
    try {
      await this.roleAssignmentService.add(this.resourceId, {
        targetResourceId: acrId,
        managedIdentityType: 'UserAssigned',
        roleDefinitionId: roleDefId,
        userAssignedIdentityId: uaiId,
      });
      await this.checkAcrPullAccess(acrId, this.acrAuthMode(), uaiId);
      await this.identityAccessSection.loadRoleAssignments();
    } catch {
      // Error handled silently; the UI will still show the missing role
    } finally {
      this.acrRoleAssigning.set(false);
    }
  }

  protected createNewUai(): void {
    const res = this.resource();
    if (!res) return;
    const resourceGroupId = res.resourceGroupId ?? '';
    const location = res.location ?? 'EastUS2';

    const dialogRef = this.dialog.open(CreateUaiDialogComponent, {
      data: { resourceGroupId, location },
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (result: UserAssignedIdentityResponse | undefined) => {
      if (!result) return;
      await this.identityAccessSection.loadAllResources();
      await this.onAcrSelectedUaiChange(result.id);
    });
  }

  ngOnDestroy(): void {
    this.formSubscriptions.forEach(s => s.unsubscribe());
    this.pageContextService.clear();
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
    return this.corsFieldErrors()[buildCorsErrorKey(service, index, field)] ?? '';
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
      ? normalizeCorsOrigin(value)
      : normalizeCorsHeader(value);

    const validationError = field === 'allowedOrigins'
      ? validateCorsOrigin(value)
      : validateCorsHeader(value);

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
    const validationResult = validateStorageCorsRules(
      this.storageCorsRulesDraft(),
      this.storageTableCorsRulesDraft(),
    );
    this.corsFieldErrors.set(validationResult.errors);
    return validationResult.isValid;
  }

  private setCorsFieldError(service: CorsServiceKey, index: number, field: CorsFieldKey, errorKey: string): void {
    this.corsFieldErrors.update(errors => ({
      ...errors,
      [buildCorsErrorKey(service, index, field)]: errorKey,
    }));
  }

  private clearCorsFieldError(service: CorsServiceKey, index: number, field: CorsFieldKey): void {
    this.corsFieldErrors.update(errors => {
      const updated = { ...errors };
      delete updated[buildCorsErrorKey(service, index, field)];
      return updated;
    });
  }

  private clearCorsRuleErrors(service: CorsServiceKey, index: number): void {
    this.corsFieldErrors.update(errors => Object.fromEntries(
      Object.entries(errors).filter(([key]) => !key.startsWith(`${service}:${index}:`))
    ));
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

}
