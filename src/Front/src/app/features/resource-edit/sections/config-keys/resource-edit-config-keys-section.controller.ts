import { computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { CompactSelectOption } from '../../../../shared/components/compact-select/compact-select.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../../shared/services/role-assignment.service';
import {
  AddAppConfigKeyDialogComponent,
  AddAppConfigKeyDialogData,
} from '../../add-app-config-key-dialog/add-app-config-key-dialog.component';
import { AppConfigurationKeyService } from '../../services/app-configuration-key.service';
import { AppConfigurationKeyResponse } from '../../models/app-configuration-key.interface';
import { createResourceEditKvRoleAssignmentState } from '../shared/resource-edit-kv-role-assignment.helpers';
import { ResourceEditConfigKeysSection } from './resource-edit-config-keys-section.interface';

interface ResourceEditConfigKeysSectionControllerDependencies {
  getResourceId(): string;
  getProjectId(): string;
  getEnvironments(): EnvironmentDefinitionResponse[];
  getAllResources(): AzureResourceResponse[];
  getAssignedUai(): { identityId: string; identityName: string } | null;
  getUaiOptions(): CompactSelectOption[];
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

  const kvRoleAssignmentState = createResourceEditKvRoleAssignmentState({
    dialog,
    roleAssignmentService,
    resourceId: dependencies.getResourceId(),
    getAllResources: dependencies.getAllResources,
    getAssignedUai: dependencies.getAssignedUai,
    getUaiOptions: dependencies.getUaiOptions,
    getResourceContext: dependencies.getResourceContext,
    reloadRoleAssignments: dependencies.reloadRoleAssignments,
    reloadAllResources: dependencies.reloadAllResources,
    reloadEntries: () => load(),
    checkKeyVaultAccess: (resourceId: string, keyVaultResourceId: string) => appSettingService.checkKeyVaultAccess(resourceId, keyVaultResourceId),
  });

  const hasWarning = computed(() => kvRoleAssignmentState.kvMissingRoleEntries().length > 0);

  const load = async (): Promise<void> => {
    isLoading.set(true);
    errorKey.set('');
    try {
      const keys = await configKeyService.list(dependencies.getResourceId());
      configKeys.set(keys);
      await kvRoleAssignmentState.refreshKvMissingRoleEntries(keys);
    } catch {
      errorKey.set('RESOURCE_EDIT.CONFIG_KEYS.LOAD_ERROR');
    } finally {
      isLoading.set(false);
    }
  };

  const handleAddDialogClose = (result?: AppConfigurationKeyResponse): void => {
    if (!result) {
      return;
    }

    configKeys.update((currentKeys) => [...currentKeys, result]);
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

    dialogRef.afterClosed().subscribe(handleAddDialogClose);
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

    dialogRef.afterClosed().subscribe((confirmed?: boolean) => {
      if (!confirmed) {
        return;
      }

      removeConfigKey(configKey.id).catch(() => undefined);
    });
  };

  const getConfigKeyType = (configKey: AppConfigurationKeyResponse): string => {
    if (configKey.isOutputReference) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_OUTPUT';
    if (configKey.isKeyVaultReference) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_KV';
    if (configKey.isViaVariableGroup) return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_VG';
    return 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_STATIC';
  };

  return {
    configKeys,
    isLoading,
    errorKey,
    hasWarning,
    kvMissingRoleEntries: kvRoleAssignmentState.kvMissingRoleEntries,
    groupedKeys,
    load,
    openAddDialog,
    openRemoveDialog,
    resolveSourceName: kvRoleAssignmentState.resolveSourceName,
    resolveSourceType: kvRoleAssignmentState.resolveSourceType,
    assignedUai: kvRoleAssignmentState.assignedUai,
    uaiOptions: kvRoleAssignmentState.uaiOptions,
    getConfigKeyType,
    setKvEntryIdentityType: kvRoleAssignmentState.setKvEntryIdentityType,
    setKvEntryUaiId: kvRoleAssignmentState.setKvEntryUaiId,
    createNewUaiForEntry: kvRoleAssignmentState.createNewUaiForEntry,
    assignKvRole: kvRoleAssignmentState.assignKvRole,
  };
}