import { Component, inject, signal, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { merge } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { TranslateModule } from '@ngx-translate/core';
import { LOCATION_OPTIONS } from '../enums/location.enum';
import { RESOURCE_TYPE_OPTIONS, ResourceTypeEnum, RESOURCE_TYPE_ICONS } from '../enums/resource-type.enum';
import { KeyVaultService } from '../../../shared/services/key-vault.service';
import { RedisCacheService } from '../../../shared/services/redis-cache.service';
import { StorageAccountService } from '../../../shared/services/storage-account.service';

export interface AddResourceDialogData {
  resourceGroupId: string;
  location: string;
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

  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly selectedType = signal<ResourceTypeEnum | null>(null);

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

  // Key Vault form
  protected readonly kvForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: [this.data.location, [Validators.required]],
    sku: ['Standard', [Validators.required]],
  });

  // Redis Cache form
  protected readonly redisForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: [this.data.location, [Validators.required]],
    sku: ['Standard', [Validators.required]],
    redisVersion: [6, [Validators.required]],
    enableNonSslPort: [false],
    minimumTlsVersion: ['Tls12', [Validators.required]],
    maxMemoryPolicy: ['NoEviction', [Validators.required]],
    capacity: [1, [Validators.required, Validators.min(0)]],
  });

  // Storage Account form
  protected readonly storageForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(80)]],
    location: [this.data.location, [Validators.required]],
    sku: ['Standard_LRS', [Validators.required]],
    kind: ['StorageV2', [Validators.required]],
    accessTier: ['Hot', [Validators.required]],
    allowBlobPublicAccess: [false],
    enableHttpsTrafficOnly: [true],
    minimumTlsVersion: ['Tls12', [Validators.required]],
  });

  private readonly formStatus = toSignal(
    merge(this.kvForm.statusChanges, this.redisForm.statusChanges, this.storageForm.statusChanges)
  );

  protected readonly isFormValid = computed(() => {
    this.formStatus(); // track form status changes
    const type = this.selectedType();
    if (!type) return false;
    switch (type) {
      case ResourceTypeEnum.KeyVault: return this.kvForm.valid;
      case ResourceTypeEnum.RedisCache: return this.redisForm.valid;
      case ResourceTypeEnum.StorageAccount: return this.storageForm.valid;
      default: return false;
    }
  });

  protected onSelectType(type: ResourceTypeEnum): void {
    this.selectedType.set(type);
    this.errorKey.set('');
  }

  protected onBack(): void {
    this.selectedType.set(null);
    this.errorKey.set('');
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }

  protected async onSubmit(): Promise<void> {
    const type = this.selectedType();
    if (!type) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      switch (type) {
        case ResourceTypeEnum.KeyVault: {
          const v = this.kvForm.getRawValue();
          await this.keyVaultService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: v.name!,
            location: v.location!,
            sku: v.sku!,
          });
          break;
        }
        case ResourceTypeEnum.RedisCache: {
          const v = this.redisForm.getRawValue();
          await this.redisCacheService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: v.name!,
            location: v.location!,
            sku: v.sku!,
            redisVersion: v.redisVersion!,
            enableNonSslPort: v.enableNonSslPort!,
            minimumTlsVersion: v.minimumTlsVersion!,
            maxMemoryPolicy: v.maxMemoryPolicy!,
            capacity: v.capacity!,
          });
          break;
        }
        case ResourceTypeEnum.StorageAccount: {
          const v = this.storageForm.getRawValue();
          await this.storageAccountService.create({
            resourceGroupId: this.data.resourceGroupId,
            name: v.name!,
            location: v.location!,
            sku: v.sku!,
            kind: v.kind!,
            accessTier: v.accessTier!,
            allowBlobPublicAccess: v.allowBlobPublicAccess!,
            enableHttpsTrafficOnly: v.enableHttpsTrafficOnly!,
            minimumTlsVersion: v.minimumTlsVersion!,
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
}
