import { Component, inject, signal, computed } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslateModule } from '@ngx-translate/core';
import { LOCATION_OPTIONS } from '../enums/location.enum';
import { RESOURCE_TYPE_OPTIONS, ResourceTypeEnum, RESOURCE_TYPE_ICONS } from '../enums/resource-type.enum';
import { KeyVaultService } from '../../../shared/services/key-vault.service';
import { RedisCacheService } from '../../../shared/services/redis-cache.service';
import { StorageAccountService } from '../../../shared/services/storage-account.service';
import { ResourceEnvironmentConfigEntry } from '../../../shared/interfaces/resource-environment-config.interface';

export interface AddResourceDialogData {
  resourceGroupId: string;
  location: string;
  environments: { name: string }[];
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

type DialogStep = 'type' | 'common' | 'environments';

@Component({
  selector: 'app-add-resource-dialog',
  standalone: true,
  imports: [
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
    ReactiveFormsModule,
    TranslateModule,
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
  private readonly fb = inject(FormBuilder);

  protected readonly step = signal<DialogStep>('type');
  protected readonly selectedType = signal<ResourceTypeEnum | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly envFormsValid = signal(true);

  protected readonly environments = this.data.environments;
  protected readonly hasEnvironments = this.data.environments.length > 0;

  protected readonly resourceTypeOptions = RESOURCE_TYPE_OPTIONS;
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

  protected readonly ResourceTypeEnum = ResourceTypeEnum;

  // ── Common form (name + location) ──
  protected readonly commonForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: [this.data.location, [Validators.required]],
  });

  // ── Per-environment FormArray ──
  protected readonly envFormArray = new FormArray<FormGroup>([]);

  private readonly commonFormStatus = toSignal(this.commonForm.statusChanges, { initialValue: 'INVALID' as const });

  protected readonly isCommonValid = computed(() => {
    this.commonFormStatus();
    return this.commonForm.valid;
  });

  private readonly _envStatusSub = this.envFormArray.statusChanges
    .pipe(takeUntilDestroyed())
    .subscribe(() => this.envFormsValid.set(this.envFormArray.valid));

  private readonly prefilled = new Set<number>();

  // ── Type Selection ──
  protected onSelectType(type: ResourceTypeEnum): void {
    this.selectedType.set(type);
    this.step.set('common');
    this.errorKey.set('');
    this.buildEnvForms(type);
  }

  // ── Navigation ──
  protected onBackToType(): void {
    this.selectedType.set(null);
    this.step.set('type');
    this.errorKey.set('');
  }

  protected onNextToEnvironments(): void {
    if (this.commonForm.invalid || !this.hasEnvironments) return;
    this.step.set('environments');
  }

  protected onBackToCommon(): void {
    this.step.set('common');
    this.errorKey.set('');
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
          redisVersion: [6, [Validators.required]],
          enableNonSslPort: [false],
          minimumTlsVersion: ['Tls12', [Validators.required]],
          maxMemoryPolicy: ['NoEviction', [Validators.required]],
        });
      case ResourceTypeEnum.StorageAccount:
        return this.fb.group({
          sku: ['Standard_LRS', [Validators.required]],
          kind: ['StorageV2', [Validators.required]],
          accessTier: ['Hot', [Validators.required]],
          allowBlobPublicAccess: [false],
          supportsHttpsTrafficOnly: [true],
          minimumTlsVersion: ['Tls12', [Validators.required]],
        });
    }
  }

  protected getEnvFormGroup(index: number): FormGroup {
    return this.envFormArray.at(index);
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
    if (!type || this.commonForm.invalid || !this.envFormsValid()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const common = this.commonForm.getRawValue();
    const environmentConfigs = this.buildEnvironmentConfigs();

    try {
      switch (type) {
        case ResourceTypeEnum.KeyVault: {
          const firstEnv = this.envFormArray.at(0).getRawValue();
          await this.keyVaultService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            sku: firstEnv.sku,
            environmentConfigs,
          });
          break;
        }
        case ResourceTypeEnum.RedisCache: {
          const firstEnv = this.envFormArray.at(0).getRawValue();
          await this.redisCacheService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            sku: firstEnv.skuName,
            redisVersion: Number(firstEnv.redisVersion),
            enableNonSslPort: firstEnv.enableNonSslPort,
            minimumTlsVersion: firstEnv.minimumTlsVersion,
            maxMemoryPolicy: firstEnv.maxMemoryPolicy,
            capacity: Number(firstEnv.capacity),
            environmentConfigs,
          });
          break;
        }
        case ResourceTypeEnum.StorageAccount: {
          const firstEnv = this.envFormArray.at(0).getRawValue();
          await this.storageAccountService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: common.name!,
            location: common.location!,
            sku: firstEnv.sku,
            kind: firstEnv.kind,
            accessTier: firstEnv.accessTier,
            allowBlobPublicAccess: firstEnv.allowBlobPublicAccess,
            enableHttpsTrafficOnly: firstEnv.supportsHttpsTrafficOnly,
            minimumTlsVersion: firstEnv.minimumTlsVersion,
            environmentConfigs,
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

  private buildEnvironmentConfigs(): ResourceEnvironmentConfigEntry[] {
    return this.environments.map((env, i) => {
      const raw = this.envFormArray.at(i).getRawValue();
      const properties: Record<string, string> = {};
      for (const [key, value] of Object.entries(raw)) {
        properties[key] = String(value);
      }
      return {
        environmentName: env.name,
        properties,
      };
    });
  }
}
