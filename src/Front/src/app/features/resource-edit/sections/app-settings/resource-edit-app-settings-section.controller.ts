import { computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { CompactSelectOption } from '../../../../shared/components/compact-select/compact-select.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AppSettingResponse } from '../../../../shared/interfaces/app-setting.interface';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../../shared/services/role-assignment.service';
import {
  AddAppSettingDialogComponent,
  AddAppSettingDialogData,
} from '../../add-app-setting-dialog/add-app-setting-dialog.component';
import {
  EditStaticAppSettingDialogComponent,
  EditStaticAppSettingDialogData,
} from '../../edit-static-app-setting-dialog/edit-static-app-setting-dialog.component';
import {
  ImportAppSettingsDialogComponent,
  ImportAppSettingsDialogData,
} from '../../import-app-settings-dialog/import-app-settings-dialog.component';
import { createResourceEditKvRoleAssignmentState } from '../shared/resource-edit-kv-role-assignment.helpers';
import { ResourceEditAppSettingsSection } from './resource-edit-app-settings-section.interface';

interface ResourceEditAppSettingsSectionControllerDependencies {
  getResourceId(): string;
  getResourceName(): string;
  getResourceType(): string;
  getProjectId(): string;
  getEnvironments(): EnvironmentDefinitionResponse[];
  getAllResources(): AzureResourceResponse[];
  getAssignedUai(): { identityId: string; identityName: string } | null;
  getUaiOptions(): CompactSelectOption[];
  getResourceContext(): { resourceGroupId: string; location: string } | null;
  getDeploymentMode(): string | null;
  getRuntimeStack(): string | null;
  reloadRoleAssignments(): Promise<void>;
  reloadAllResources(): Promise<void>;
}

export function createResourceEditAppSettingsSectionController(
  dependencies: ResourceEditAppSettingsSectionControllerDependencies,
): ResourceEditAppSettingsSection {
  const dialog = inject(MatDialog);
  const appSettingService = inject(AppSettingService);
  const roleAssignmentService = inject(RoleAssignmentService);

  const appSettings = signal<AppSettingResponse[]>([]);
  const isLoading = signal(false);
  const errorKey = signal('');

  const groupedSettings = computed(() => {
    const allSettings = appSettings();
    const byVg = new Map<string, { vgName: string; vgId: string; settings: AppSettingResponse[] }>();
    const outputs: AppSettingResponse[] = [];
    const statics: AppSettingResponse[] = [];

    for (const setting of allSettings) {
      if (setting.isViaVariableGroup && setting.variableGroupId) {
        const vgId = setting.variableGroupId;
        if (!byVg.has(vgId)) {
          byVg.set(vgId, { vgName: setting.variableGroupName ?? vgId, vgId, settings: [] });
        }

        byVg.get(vgId)?.settings.push(setting);
      } else if (setting.isOutputReference) {
        outputs.push(setting);
      } else {
        statics.push(setting);
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
      const settings = await appSettingService.getByResourceId(dependencies.getResourceId());
      appSettings.set(settings);
      await kvRoleAssignmentState.refreshKvMissingRoleEntries(settings);
    } catch {
      errorKey.set('RESOURCE_EDIT.APP_SETTINGS.LOAD_ERROR');
    } finally {
      isLoading.set(false);
    }
  };

  const handleAddDialogClose = (result?: AppSettingResponse): void => {
    if (!result) {
      return;
    }

    appSettings.update((currentSettings) => [...currentSettings, result]);
  };

  const openAddDialog = (): void => {
    const dialogRef = dialog.open(AddAppSettingDialogComponent, {
      data: {
        resourceId: dependencies.getResourceId(),
        currentResourceName: dependencies.getResourceName(),
        siblingResources: dependencies.getAllResources(),
        environments: dependencies.getEnvironments().map((environment) => ({ name: environment.name })),
        projectId: dependencies.getProjectId(),
      } satisfies AddAppSettingDialogData,
      width: '520px',
      maxHeight: '85vh',
      panelClass: 'ifs-add-app-setting-dialog',
    });

    dialogRef.afterClosed().subscribe(handleAddDialogClose);
  };

  const handleImportDialogClose = (result?: AppSettingResponse[]): void => {
    if (!result?.length) {
      return;
    }

    appSettings.update((currentSettings) => [...currentSettings, ...result]);
  };

  const openImportDialog = (): void => {
    const dialogRef = dialog.open(ImportAppSettingsDialogComponent, {
      data: {
        resourceId: dependencies.getResourceId(),
        currentResourceName: dependencies.getResourceName(),
        siblingResources: dependencies.getAllResources(),
        environments: dependencies.getEnvironments().map((environment) => ({ name: environment.name })),
        projectId: dependencies.getProjectId(),
        existingSettingNames: appSettings().map((setting) => setting.name),
        resourceType: dependencies.getResourceType(),
        deploymentMode: dependencies.getDeploymentMode(),
        runtimeStack: dependencies.getRuntimeStack(),
      } satisfies ImportAppSettingsDialogData,
      width: '780px',
      maxHeight: '85vh',
      panelClass: 'ifs-import-app-settings-dialog',
    });

    dialogRef.afterClosed().subscribe(handleImportDialogClose);
  };

  const handleEditStaticDialogClose = (result?: AppSettingResponse): void => {
    if (!result) {
      return;
    }

    appSettings.update((currentSettings) => currentSettings.map((currentSetting) => currentSetting.id === result.id ? result : currentSetting));
  };

  const openEditStaticDialog = (setting: AppSettingResponse): void => {
    const dialogRef = dialog.open(EditStaticAppSettingDialogComponent, {
      data: {
        resourceId: dependencies.getResourceId(),
        appSettingId: setting.id,
        currentName: setting.name,
        currentEnvironmentValues: setting.environmentValues ?? {},
        environments: dependencies.getEnvironments().map((environment) => ({ name: environment.name })),
      } satisfies EditStaticAppSettingDialogData,
      width: '480px',
      maxHeight: '80vh',
    });

    dialogRef.afterClosed().subscribe(handleEditStaticDialogClose);
  };

  const removeAppSetting = async (appSettingId: string): Promise<void> => {
    errorKey.set('');
    try {
      await appSettingService.remove(dependencies.getResourceId(), appSettingId);
      appSettings.update((currentSettings) => currentSettings.filter((setting) => setting.id !== appSettingId));
    } catch {
      errorKey.set('RESOURCE_EDIT.APP_SETTINGS.REMOVE_ERROR');
    }
  };

  const openRemoveDialog = (setting: AppSettingResponse): void => {
    const dialogRef = dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_TITLE',
        messageKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_MESSAGE',
        messageParams: { name: setting.name },
        confirmKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_YES',
        cancelKey: 'RESOURCE_EDIT.APP_SETTINGS.REMOVE_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe((confirmed?: boolean) => {
      if (!confirmed) {
        return;
      }

      removeAppSetting(setting.id).catch(() => undefined);
    });
  };

  return {
    appSettings,
    isLoading,
    errorKey,
    hasWarning,
    kvMissingRoleEntries: kvRoleAssignmentState.kvMissingRoleEntries,
    groupedSettings,
    load,
    openAddDialog,
    openImportDialog,
    openEditStaticDialog,
    openRemoveDialog,
    resolveSourceName: kvRoleAssignmentState.resolveSourceName,
    resolveSourceType: kvRoleAssignmentState.resolveSourceType,
    assignedUai: kvRoleAssignmentState.assignedUai,
    uaiOptions: kvRoleAssignmentState.uaiOptions,
    setKvEntryIdentityType: kvRoleAssignmentState.setKvEntryIdentityType,
    setKvEntryUaiId: kvRoleAssignmentState.setKvEntryUaiId,
    createNewUaiForEntry: kvRoleAssignmentState.createNewUaiForEntry,
    assignKvRole: kvRoleAssignmentState.assignKvRole,
  };
}