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
import { KeyVaultResponse, KeyVaultEnvironmentConfigEntry } from '../../shared/interfaces/key-vault.interface';
import { RedisCacheResponse, RedisCacheEnvironmentConfigEntry } from '../../shared/interfaces/redis-cache.interface';
import { StorageAccountResponse, StorageAccountEnvironmentConfigEntry } from '../../shared/interfaces/storage-account.interface';
import { InfrastructureConfigResponse, EnvironmentDefinitionResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { RESOURCE_TYPE_ICONS } from '../config-detail/enums/resource-type.enum';
import { LOCATION_OPTIONS } from '../../shared/enums/location.enum';

/** Union type for any loaded resource */
type ResourceData = KeyVaultResponse | RedisCacheResponse | StorageAccountResponse;

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
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);

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
      default:
        throw new Error(`Unknown resource type: ${this.resourceType}`);
    }
  }

  private buildGeneralForm(resource: ResourceData): void {
    this.generalForm = this.fb.group({
      name: [resource.name, [Validators.required, Validators.maxLength(80)]],
      location: [resource.location, [Validators.required]],
    });
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
      }
      this.router.navigate(['/config', this.configId]);
    } catch {
      this.saveError.set('RESOURCE_EDIT.ERROR.DELETE_FAILED');
      this.isSaving.set(false);
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
}
