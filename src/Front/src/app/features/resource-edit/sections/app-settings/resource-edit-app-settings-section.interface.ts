import { Signal } from '@angular/core';

import { CompactSelectOption } from '../../../../shared/components/compact-select/compact-select.component';
import { AppSettingResponse } from '../../../../shared/interfaces/app-setting.interface';
import { ResourceEditKvMissingRoleEntry } from '../shared/resource-edit-kv-missing-role-entry.interface';

export interface ResourceEditAppSettingsSection {
  readonly appSettings: Signal<AppSettingResponse[]>;
  readonly isLoading: Signal<boolean>;
  readonly errorKey: Signal<string>;
  readonly hasWarning: Signal<boolean>;
  readonly kvMissingRoleEntries: Signal<ResourceEditKvMissingRoleEntry[]>;
  readonly groupedSettings: Signal<{
    byVg: Array<{ vgName: string; vgId: string; settings: AppSettingResponse[] }>;
    outputs: AppSettingResponse[];
    statics: AppSettingResponse[];
  }>;

  load(): Promise<void>;
  openAddDialog(): void;
  openImportDialog(): void;
  openEditStaticDialog(setting: AppSettingResponse): void;
  openRemoveDialog(setting: AppSettingResponse): void;
  resolveSourceName(sourceResourceId: string): string;
  resolveSourceType(sourceResourceId: string): string;
  assignedUai(): { identityId: string; identityName: string } | null;
  uaiOptions(): CompactSelectOption[];
  setKvEntryIdentityType(kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void;
  setKvEntryUaiId(kvId: string, uaiId: string | null): void;
  createNewUaiForEntry(kvId: string): void;
  assignKvRole(entry: ResourceEditKvMissingRoleEntry): Promise<void>;
}