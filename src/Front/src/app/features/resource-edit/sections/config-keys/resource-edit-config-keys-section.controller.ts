import { computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DsSelectOption } from '../../../../shared/components/ds';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { UserAssignedIdentityResponse } from '../../../../shared/interfaces/user-assigned-identity.interface';
import { AppSettingService } from '../../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../../shared/services/role-assignment.service';
import { CreateUaiDialogComponent } from '../../create-uai-dialog/create-uai-dialog.component';
import {
  AddAppConfigKeyDialogComponent,
  AddAppConfigKeyDialogData,
} from '../../add-app-config-key-dialog/add-app-config-key-dialog.component';
import { AppConfigurationKeyService } from '../../services/app-configuration-key.service';
import { AppConfigurationKeyResponse } from '../../models/app-configuration-key.interface';
import { ResourceEditConfigKeysSection } from './resource-edit-config-keys-section.interface';
import { ResourceEditKvMissingRoleEntry } from '../shared/resource-edit-kv-missing-role-entry.interface';

interface ResourceEditConfigKeysSectionControllerDependencies {
  getResourceId(): string;
  getProjectId(): string;
  getEnvironments(): EnvironmentDefinitionResponse[];
  getAllResources(): AzureResourceResponse[];
  getAssignedUai(): { identityId: string; identityName: string } | null;
  getUaiOptions(): DsSelectOption[];
  getResourceContext(): { resourceGroupId: string; location: string } | null;
  reloadRoleAssignments(): Promise<void>;
  reloadAllResources(): Promise<void>;
}

export function createResourceEditConfigKeysSectionController(
  dependencies: ResourceEditConfigKeysSectionControllerDependencies,
): ResourceEditConfigKeysSection {
  const dialog = inject(MatDialog);
  const appSettingService = inject(AppSettingService);
  const roleAssignmentService = inject(RoleAssignmentService);
  const configKeyService = inject(AppConfigurationKeyService);

  const configKeys = signal<AppConfigurationKeyResponse[]>([]);
  const isLoading = signal(false);
  const errorKey = signal('');
  const kvMissingRoleEntries = signal<ResourceEditKvMissingRoleEntry[]>([]);

  const groupedKeys = computed(() => {
    const allKeys = configKeys();
    const byVg = new Map<string, { vgName: string; vgId: string; keys: AppConfigurationKeyResponse[] }>();
    const outputs: AppConfigurationKeyResponse[] = [];
    const statics: AppConfigurationKeyResponse[] = [];

    for (const configKey of allKeys) {
      if (configKey.isViaVariableGroup && configKey.variableGroupId) {
        const vgId = configKey.variableGroupId;
        if (!byVg.has(vgId)) {
          byVg.set(vgId, { vgName: configKey.variableGroupName ?? vgId, vgId, keys: [] });
        }

        byVg.get(vgId)?.keys.push(configKey);
      } else if (configKey.isOutputReference) {
        outputs.push(configKey);
      } else {
        statics.push(configKey);
      }
    }

    return {
      byVg: [...byVg.values()],
      outputs,
      statics,
    };
  });

  const hasWarning = computed(() => kvMissingRoleEntries().length > 0);

  const resolveSourceName = (sourceResourceId: string): string =>
    dependencies.getAllResources().find((resource) => resource.id === sourceResourceId)?.name ?? sourceResourceId;

  const resolveSourceType = (sourceResourceId: string): string =>
    dependencies.getAllResources().find((resource) => resource.id === sourceResourceId)?.resourceType ?? '';

  const assignedUai = (): { identityId: string; identityName: string } | null => dependencies.getAssignedUai();
  const uaiOptions = (): DsSelectOption[] => dependencies.getUaiOptions();

  const checkKvAccessForConfigKeys = async (keys: AppConfigurationKeyResponse[]): Promise<void> => {
    const missingByKv = new Map<string, AppConfigurationKeyResponse[]>();

    for (const configKey of keys) {
      if (configKey.isKeyVaultReference && configKey.keyVaultResourceId && configKey.hasKeyVaultAccess === false) {
        const existing = missingByKv.get(configKey.keyVaultResourceId);
        if (existing) {
          existing.push(configKey);
        } else {
          missingByKv.set(configKey.keyVaultResourceId, [configKey]);
        }
      }
    }

    if (missingByKv.size === 0) {
      kvMissingRoleEntries.set([]);
      return;
    }

    const assignedIdentity = assignedUai();
    const availableUaiOptions = uaiOptions();
    const singleUaiId = assignedIdentity ? assignedIdentity.identityId : (availableUaiOptions.length === 1 ? String(availableUaiOptions[0].value) : null);
    const entries: ResourceEditKvMissingRoleEntry[] = [...missingByKv.entries()].map(([keyVaultResourceId, affected]) => ({
      keyVaultResourceId,
      keyVaultName: resolveSourceName(keyVaultResourceId),
      missingRoleName: null,
      missingRoleDefinitionId: null,
      affectedSettingsCount: affected.length,
      checking: true,
      assigning: false,
      selectedIdentityType: 'UserAssigned',
      selectedUaiId: singleUaiId,
    }));

    kvMissingRoleEntries.set(entries);

    for (const entry of entries) {
      try {
        const result = await appSettingService.checkKeyVaultAccess(dependencies.getResourceId(), entry.keyVaultResourceId);
        kvMissingRoleEntries.update((currentEntries) => currentEntries.map((currentEntry) => (
          currentEntry.keyVaultResourceId === entry.keyVaultResourceId
            ? {
                ...currentEntry,
                missingRoleName: result.missingRoleName ?? null,
                missingRoleDefinitionId: result.missingRoleDefinitionId ?? null,
                checking: false,
              }
            : currentEntry
        )));
      } catch {
        kvMissingRoleEntries.update((currentEntries) => currentEntries.map((currentEntry) => (
          currentEntry.keyVaultResourceId === entry.keyVaultResourceId
            ? { ...currentEntry, checking: false }
            : currentEntry
        )));
      }
    }
  };

  const load = async (): Promise<void> => {
    isLoading.set(true);
    errorKey.set('');
    try {
      const keys = await configKeyService.list(dependencies.getResourceId());
      configKeys.set(keys);
      await checkKvAccessForConfigKeys(keys);
    } catch {
      errorKey.set('RESOURCE_EDIT.CONFIG_KEYS.LOAD_ERROR');
    } finally {
      isLoading.set(false);
    }
  };

  const openAddDialog = (): void => {
    const dialogRef = dialog.open(AddAppConfigKeyDialogComponent, {
      data: {
        appConfigurationId: dependencies.getResourceId(),
        siblingResources: dependencies.getAllResources(),
        environments: dependencies.getEnvironments().map((environment) => ({ name: environment.name })),
        projectId: dependencies.getProjectId(),
      } satisfies AddAppConfigKeyDialogData,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe((result?: AppConfigurationKeyResponse) => {
      if (result) {
        configKeys.update((currentKeys) => [...currentKeys, result]);
      }
    });
  };

  const removeConfigKey = async (configKeyId: string): Promise<void> => {
    errorKey.set('');
    try {
      await configKeyService.remove(dependencies.getResourceId(), configKeyId);
      configKeys.update((currentKeys) => currentKeys.filter((configKey) => configKey.id !== configKeyId));
    } catch {
      errorKey.set('RESOURCE_EDIT.CONFIG_KEYS.REMOVE_ERROR');
    }
  };

  const openRemoveDialog = (configKey: AppConfigurationKeyResponse): void => {
    const dialogRef = dialog.open(ConfirmDialogComponent, {
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
      if (confirmed) {
        await removeConfigKey(configKey.id);
      }
    });
  };

  const getConfigKeyType = (configKey: AppConfigurationKeyResponse): string => {
    if (configKey.isOutputReference) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_OUTPUT';
    if (configKey.isKeyVaultReference) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_KV';
    if (configKey.isViaVariableGroup) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_VG';
    return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_STATIC';
  };

  const setKvEntryIdentityType = (kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void => {
    kvMissingRoleEntries.update((currentEntries) => currentEntries.map((entry) => (
      entry.keyVaultResourceId === kvId
        ? { ...entry, selectedIdentityType: type, selectedUaiId: type === 'SystemAssigned' ? null : entry.selectedUaiId }
        : entry
    )));
  };

  const setKvEntryUaiId = (kvId: string, uaiId: string | null): void => {
    kvMissingRoleEntries.update((currentEntries) => currentEntries.map((entry) => (
      entry.keyVaultResourceId === kvId
        ? { ...entry, selectedUaiId: uaiId }
        : entry
    )));
  };

  const createNewUaiForEntry = (kvId: string): void => {
    const resourceContext = dependencies.getResourceContext();
    if (!resourceContext) {
      return;
    }

    const dialogRef = dialog.open(CreateUaiDialogComponent, {
      data: resourceContext,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (result: UserAssignedIdentityResponse | undefined) => {
      if (!result) {
        return;
      }

      await dependencies.reloadAllResources();
      setKvEntryUaiId(kvId, result.id);
    });
  };

  const assignKvRole = async (entry: ResourceEditKvMissingRoleEntry): Promise<void> => {
    kvMissingRoleEntries.update((currentEntries) => currentEntries.map((currentEntry) => (
      currentEntry.keyVaultResourceId === entry.keyVaultResourceId
        ? { ...currentEntry, assigning: true }
        : currentEntry
    )));

    try {
      await roleAssignmentService.add(dependencies.getResourceId(), {
        targetResourceId: entry.keyVaultResourceId,
        managedIdentityType: entry.selectedIdentityType,
        roleDefinitionId: entry.missingRoleDefinitionId!,
        userAssignedIdentityId: entry.selectedIdentityType === 'UserAssigned'
          ? (assignedUai()?.identityId ?? entry.selectedUaiId!)
          : undefined,
      });

      await load();
      await dependencies.reloadRoleAssignments();
    } catch {
      kvMissingRoleEntries.update((currentEntries) => currentEntries.map((currentEntry) => (
        currentEntry.keyVaultResourceId === entry.keyVaultResourceId
          ? { ...currentEntry, assigning: false }
          : currentEntry
      )));
    }
  };

  return {
    configKeys,
    isLoading,
    errorKey,
    hasWarning,
    kvMissingRoleEntries,
    groupedKeys,
    load,
    openAddDialog,
    openRemoveDialog,
    resolveSourceName,
    resolveSourceType,
    assignedUai,
    uaiOptions,
    getConfigKeyType,
    setKvEntryIdentityType,
    setKvEntryUaiId,
    createNewUaiForEntry,
    assignKvRole,
  };
}