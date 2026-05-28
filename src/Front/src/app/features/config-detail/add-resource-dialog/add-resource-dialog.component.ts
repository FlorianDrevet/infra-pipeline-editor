import { Component, DestroyRef, inject, OnInit, signal, computed } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, debounceTime, distinctUntilChanged, EMPTY, filter, Observable, switchMap, tap } from 'rxjs';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsTabsComponent } from '../../../shared/components/ds/ds-tabs/ds-tabs.component';
import { DsTabDefinition } from '../../../shared/components/ds/ds-tabs/ds-tabs.types';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LOCATION_OPTIONS } from '../enums/location.enum';
import { RESOURCE_TYPE_OPTIONS, ResourceTypeEnum, RESOURCE_TYPE_ICONS, RESOURCE_TYPE_CATEGORIES } from '../enums/resource-type.enum';
import { hasResourceTypeEnvironmentSettings } from '../../../shared/resource-metadata/resource-type.metadata';
import { OS_TYPE_OPTIONS } from '../enums/os-type.enum';
import { APP_SERVICE_PLAN_SKU_OPTIONS } from '../enums/app-service-plan-sku.enum';
import { RUNTIME_STACK_OPTIONS } from '../enums/runtime-stack.enum';
import { FUNCTION_APP_RUNTIME_STACK_OPTIONS } from '../enums/function-app-runtime-stack.enum';
import { DsTagInputItem } from '../../../shared/components/ds/ds-tag-input/ds-tag-input.types';
import { AcrAuthMode } from '../../../shared/interfaces/container-registry.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { ProjectResourceResponse } from '../../../shared/interfaces/cross-config-reference.interface';
import { NameAvailabilityService } from '../../../shared/services/name-availability.service';
import { EnvironmentNameAvailabilityResponseItem } from '../../../shared/interfaces/name-availability.interface';
import { ToggleSectionCardComponent } from '../../../shared/components/toggle-section-card/toggle-section-card.component';
import { DeploymentConfigComponent } from '../../../shared/components/deployment-config/deployment-config.component';
import { DsButtonComponent, DsTextFieldComponent, DsSelectComponent, DsToggleComponent, DsIconButtonComponent, DsOptionCardComponent, DsPanelActionButtonComponent, DsTagInputComponent } from '../../../shared/components/ds';
import {
  applyAddResourceProbeToggle,
  copyAddResourceEnvironmentSettings,
  createAddResourceEnvironmentFormGroup,
} from './add-resource-dialog-environment-settings.helper';
import {
  patchAddResourceParentResourceSelection,
  resolveAddResourceParentResourceDescriptor,
  resolveAllowedChildResourceTypes,
} from './add-resource-dialog-parent-resource.helper';
import { AddResourceDialogPlanWorkflowService } from './add-resource-dialog-plan-workflow.service';
import { AddResourceDialogResourceSubmitterService } from './add-resource-dialog-resource-submitter.service';
import { VnetHelpDialogComponent } from '../../../shared/components/vnet-help-dialog/vnet-help-dialog.component';
import { createVnetCidrTagValidator, createVnetIpv4TagValidator } from '../../../shared/networking/vnet-tag-input.helpers';

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
    MatDialogModule,
    MatIconModule,
    DsSpinnerComponent,
    DsToggleComponent,
    DsTabsComponent,
    ReactiveFormsModule,
    TranslateModule,
    ToggleSectionCardComponent,
    DeploymentConfigComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    DsOptionCardComponent,
    DsPanelActionButtonComponent,
    DsTagInputComponent,
    DsTextFieldComponent,
    DsSelectComponent,
  ],
  templateUrl: './add-resource-dialog.component.html',
  styleUrl: './add-resource-dialog.component.scss',
  providers: [AddResourceDialogPlanWorkflowService, AddResourceDialogResourceSubmitterService],
})
export class AddResourceDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<AddResourceDialogComponent>);
  private readonly dialog = inject(MatDialog);
  private readonly data: AddResourceDialogData = inject(MAT_DIALOG_DATA);
  private readonly nameAvailabilityService = inject(NameAvailabilityService);
  private readonly planWorkflow = inject(AddResourceDialogPlanWorkflowService);
  private readonly resourceSubmitter = inject(AddResourceDialogResourceSubmitterService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly translate = inject(TranslateService);
  private readonly vnetValidationMessage = (key: string): string => this.translate.instant(key);

  protected readonly step = signal<DialogStep>('type');
  protected readonly selectedType = signal<ResourceTypeEnum | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly envFormsValid = signal(true);
  protected readonly vnetAddressSpaceValidator = createVnetCidrTagValidator(this.vnetValidationMessage);
  protected readonly vnetDnsServerValidator = createVnetIpv4TagValidator(this.vnetValidationMessage);

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

  protected readonly isNameAvailabilityCheckEnabled = computed(() => {
    const type = this.selectedType();
    return type !== null && AddResourceDialogComponent.NAME_AVAILABILITY_TYPES.has(type) && !this.isExistingResource();
  });

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
    // Invalid names always block — no override possible (naming constraints violation)
    if (state === 'has-invalid') return true;
    // Unavailable names (conflict) can be overridden ("it's my resource")
    if (state === 'has-unavailable') {
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
  private readonly allowedChildTypes: ResourceTypeEnum[] | null = resolveAllowedChildResourceTypes(this.parentResource?.resourceType);
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

  protected readonly envTabs: readonly DsTabDefinition[] = this.data.environments.map((env) => ({
    id: env.name,
    label: env.name,
  }));
  protected readonly activeEnvTabId = signal<string | null>(this.data.environments[0]?.name ?? null);

  protected readonly needsEnvironmentSettings = computed(() => {
    return hasResourceTypeEnvironmentSettings(this.selectedType());
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
  protected readonly parentResourceDescriptor = computed(() => resolveAddResourceParentResourceDescriptor(this.selectedType()));

  protected readonly parentResourceSuffix = computed(() => this.parentResourceDescriptor().suffix);

  protected readonly parentResourceIcon = computed(() => this.parentResourceDescriptor().icon);

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
    redisVersion: [6],
    enableNonSslPort: [false],
    disableAccessKeyAuthentication: [false],
    enableAadAuth: [false],
    enableDdosProtection: [false],
    vnetAddressSpacesInput: this.fb.nonNullable.control<DsTagInputItem[]>([]),
    vnetDnsServersInput: this.fb.nonNullable.control<DsTagInputItem[]>([]),
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
  }

  ngOnInit(): void {
    this.initializeParentSelection();
  }

  private initializeParentSelection(): void {
    if (!this.parentResource || !this.allowedChildTypes) {
      return;
    }

    this.selectedPlanId.set(this.parentResource.id);
    this.selectedPlanName.set(this.parentResource.name);

    if (this.allowedChildTypes.length !== 1) {
      return;
    }

    const childType = this.allowedChildTypes[0];
    this.applySelectedType(childType);
    this.patchParentPlanSelection(childType, this.parentResource.id);
    this.step.set('common');

    if (this.requiresContainerRegistryLoading(childType)) {
      void this.loadAvailableContainerRegistries();
    }
  }

  private applySelectedType(type: ResourceTypeEnum): void {
    this.selectedType.set(type);
    this.buildEnvForms(type);
    this.updateCommonFormValidators(type);
  }

  private requiresContainerRegistryLoading(type: ResourceTypeEnum): boolean {
    return resolveAddResourceParentResourceDescriptor(type).requiresContainerRegistryLoading;
  }

  private requiresPlanSelection(type: ResourceTypeEnum): boolean {
    return resolveAddResourceParentResourceDescriptor(type).requiresPlanSelection;
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

  protected openVnetHelpDialog(): void {
    this.dialog.open(VnetHelpDialogComponent, {
      width: '640px',
      data: { context: 'resourceCreate' as const },
    });
  }

  protected getVnetAddressSpacesErrorText(): string | undefined {
    const control = this.commonForm.controls.vnetAddressSpacesInput;
    if (control.hasError('required') && control.touched) {
      return this.translate.instant('COMMON.VNET_HELP_DIALOG.VALIDATION.ADDRESS_SPACE_REQUIRED');
    }

    return undefined;
  }

  private patchParentPlanSelection(type: ResourceTypeEnum | null, planId: string | null): void {
    patchAddResourceParentResourceSelection(this.commonForm, type, planId);
  }

  // ── Type Selection ──
  protected onSelectType(type: ResourceTypeEnum): void {
    this.applySelectedType(type);
    this.errorKey.set('');

    if (this.parentResource) {
      this.patchParentPlanSelection(type, this.parentResource.id);
      this.step.set('common');
      if (this.requiresContainerRegistryLoading(type)) {
        void this.loadAvailableContainerRegistries();
      }
      return;
    }

    if (this.requiresPlanSelection(type)) {
      this.step.set('plan-selection');
      void this.loadExistingPlans();
      return;
    }

    this.step.set('common');
  }

  // ── Plan selection (WebApp flow) ──
  private async loadAvailableContainerRegistries(): Promise<void> {
    this.availableContainerRegistries.set(
      await this.planWorkflow.loadAvailableContainerRegistries(this.data.configId, this.data.projectId),
    );
  }

  private async loadExistingPlans(): Promise<void> {
    this.plansLoading.set(true);
    try {
      const selection = await this.planWorkflow.loadPlanSelection(this.selectedType(), this.data.configId, this.data.projectId);
      this.existingPlans.set(selection.existingPlans);
      this.crossConfigPlans.set(selection.crossConfigPlans);
      this.availableContainerRegistries.set(selection.availableContainerRegistries);
    } catch {
      this.existingPlans.set([]);
      this.crossConfigPlans.set([]);
      this.availableContainerRegistries.set([]);
    } finally {
      this.plansLoading.set(false);
    }
  }

  protected onSelectPlan(plan: AzureResourceResponse): void {
    this.selectedPlanId.set(plan.id);
    this.selectedPlanName.set(plan.name);
    this.patchParentPlanSelection(this.selectedType(), plan.id);
    this.step.set('common');
  }

  protected async onSelectCrossConfigPlan(plan: ProjectResourceResponse): Promise<void> {
    await this.planWorkflow.ensureCrossConfigPlanReference(this.data.configId, plan.resourceId);
    this.selectedPlanId.set(plan.resourceId);
    this.selectedPlanName.set(plan.resourceName);
    this.patchParentPlanSelection(this.selectedType(), plan.resourceId);
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
    this.patchParentPlanSelection(this.selectedType(), null);
    this.step.set('common');
  }

  protected async onCreatePlanAndContinue(): Promise<void> {
    if (this.createPlanForm.invalid) return;
    this.isCreatingPlan.set(true);
    this.errorKey.set('');

    const planData = this.createPlanForm.getRawValue();
    try {
      const createdPlan = await this.planWorkflow.createPlan({
        selectedType: this.selectedType(),
        resourceGroupId: this.data.resourceGroupId,
        planData: {
          name: planData.name ?? '',
          location: planData.location ?? '',
          osType: planData.osType ?? '',
        },
        sqlServerData: {
          name: this.createSqlServerForm.controls.name.value ?? '',
          location: this.createSqlServerForm.controls.location.value ?? '',
          version: this.createSqlServerForm.controls.version.value ?? 'V12',
          administratorLogin: this.createSqlServerForm.controls.administratorLogin.value ?? '',
        },
        environments: this.environments,
      });
      this.selectedPlanId.set(createdPlan.id);
      this.selectedPlanName.set(createdPlan.name);
      this.patchParentPlanSelection(this.selectedType(), createdPlan.id);
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
    if (this.commonForm.invalid || this.isSubmitBlockedByNameAvailability()) {
      this.commonForm.markAllAsTouched();
      return;
    }
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
      this.envFormArray.push(createAddResourceEnvironmentFormGroup(this.fb, type));
    }
    this.envFormsValid.set(this.envFormArray.valid);
  }

  protected getEnvFormGroup(index: number): FormGroup {
    return this.envFormArray.at(index);
  }

  protected onProbeToggle(envIndex: number, probeType: 'readiness' | 'liveness' | 'startup', enabled: boolean): void {
    applyAddResourceProbeToggle(this.envFormArray, envIndex, probeType, enabled);
  }

  protected onEnvTabChange(envName: string): void {
    const index = this.environments.findIndex((env) => env.name === envName);
    if (index < 0) {
      return;
    }
    if (!this.shouldCopyFromFirstEnvironment(index)) {
      return;
    }

    this.copyEnvironmentSettingsFromFirst(index);
  }

  protected copyFromFirst(): void {
    if (this.envFormArray.length < 2) return;
    for (let i = 1; i < this.envFormArray.length; i++) {
      copyAddResourceEnvironmentSettings(this.envFormArray, 0, i);
      this.prefilled.add(i);
    }
  }

  private shouldCopyFromFirstEnvironment(index: number): boolean {
    return index > 0 && !this.prefilled.has(index) && this.envFormArray.length > 1;
  }

  private copyEnvironmentSettingsFromFirst(index: number): void {
    copyAddResourceEnvironmentSettings(this.envFormArray, 0, index);
    this.prefilled.add(index);
  }

  // ── Submit ──
  protected async onSubmit(): Promise<void> {
    const type = this.selectedType();
    if (!type || !this.envFormsValid() || this.isSubmitBlockedByNameAvailability()) return;
    if (this.commonForm.invalid) {
      this.commonForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const common = this.commonForm.getRawValue();

    try {
      await this.submitSelectedResource(type, common);
      this.dialogRef.close(true);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.RESOURCES.ADD_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private async submitSelectedResource(type: ResourceTypeEnum, common: ReturnType<FormGroup['getRawValue']>): Promise<void> {
    await this.resourceSubmitter.submit({
      type,
      common,
      resourceGroupId: this.data.resourceGroupId,
      environments: this.environments,
      envFormArray: this.envFormArray,
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
    } else if (type === ResourceTypeEnum.VirtualNetwork) {
      this.commonForm.controls.vnetAddressSpacesInput.setValidators([Validators.required]);
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
    this.commonForm.controls.vnetAddressSpacesInput.updateValueAndValidity();
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
    this.commonForm.controls.vnetAddressSpacesInput.clearValidators();
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
    this.commonForm.controls.vnetAddressSpacesInput.updateValueAndValidity();
  }
}
