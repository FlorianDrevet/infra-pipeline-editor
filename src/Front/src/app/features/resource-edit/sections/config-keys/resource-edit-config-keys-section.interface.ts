import { Signal } from '@angular/core';

import { CompactSelectOption } from '../../../../shared/components/compact-select/compact-select.component';
import { AppConfigurationKeyResponse } from '../../models/app-configuration-key.interface';
import { ResourceEditKvMissingRoleEntry } from '../shared/resource-edit-kv-missing-role-entry.interface';

export interface ResourceEditConfigKeysSection {
  readonly configKeys: Signal<AppConfigurationKeyResponse[]>;
  readonly isLoading: Signal<boolean>;
  readonly errorKey: Signal<string>;
  readonly hasWarning: Signal<boolean>;
  readonly kvMissingRoleEntries: Signal<ResourceEditKvMissingRoleEntry[]>;
  readonly groupedKeys: Signal<{
    byVg: Array<{ vgName: string; vgId: string; keys: AppConfigurationKeyResponse[] }>;
    outputs: AppConfigurationKeyResponse[];
    statics: AppConfigurationKeyResponse[];
  }>;

  load(): Promise<void>;
  openAddDialog(): void;
  openRemoveDialog(configKey: AppConfigurationKeyResponse): void;
  resolveSourceName(sourceResourceId: string): string;
  resolveSourceType(sourceResourceId: string): string;
  assignedUai(): { identityId: string; identityName: string } | null;
  uaiOptions(): CompactSelectOption[];
  getConfigKeyType(configKey: AppConfigurationKeyResponse): string;
  setKvEntryIdentityType(kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void;
  setKvEntryUaiId(kvId: string, uaiId: string | null): void;
  createNewUaiForEntry(kvId: string): void;
  assignKvRole(entry: ResourceEditKvMissingRoleEntry): Promise<void>;
}