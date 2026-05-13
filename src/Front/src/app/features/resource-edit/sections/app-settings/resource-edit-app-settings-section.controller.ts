import { computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DsSelectOption } from '../../../../shared/components/ds';
import { AppSettingResponse } from '../../../../shared/interfaces/app-setting.interface';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { UserAssignedIdentityResponse } from '../../../../shared/interfaces/user-assigned-identity.interface';
import { AppSettingService } from '../../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../../shared/services/role-assignment.service';
import {
  AddAppSettingDialogComponent,
  AddAppSettingDialogData,
} from '../../add-app-setting-dialog/add-app-setting-dialog.component';
import { CreateUaiDialogComponent } from '../../create-uai-dialog/create-uai-dialog.component';
import {
  EditStaticAppSettingDialogComponent,
  EditStaticAppSettingDialogData,
} from '../../edit-static-app-setting-dialog/edit-static-app-setting-dialog.component';
import {
  ImportAppSettingsDialogComponent,
  ImportAppSettingsDialogData,
} from '../../import-app-settings-dialog/import-app-settings-dialog.component';
import { ResourceEditAppSettingsSection } from './resource-edit-app-settings-section.interface';
import { ResourceEditKvMissingRoleEntry } from '../shared/resource-edit-kv-missing-role-entry.interface';

interface ResourceEditAppSettingsSectionControllerDependencies {
  getResourceId(): string;
  getResourceName(): string;
  getResourceType(): string;
  getProjectId(): string;
  getEnvironments(): EnvironmentDefinitionResponse[];
  getAllResources(): AzureResourceResponse[];
  getAssignedUai(): { identityId: string; identityName: string } | null;
  getUaiOptions(): DsSelectOption[];
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
  const kvMissingRoleEntries = signal<ResourceEditKvMissingRoleEntry[]>([]);

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

  const hasWarning = computed(() => kvMissingRoleEntries().length > 0);

  const resolveSourceName = (sourceResourceId: string): string =>
    dependencies.getAllResources().find((resource) => resource.id === sourceResourceId)?.name ?? sourceResourceId;

  const resolveSourceType = (sourceResourceId: string): string =>
    dependencies.getAllResources().find((resource) => resource.id === sourceResourceId)?.resourceType ?? '';

  const assignedUai = (): { identityId: string; identityName: string } | null => dependencies.getAssignedUai();
  const uaiOptions = (): DsSelectOption[] => dependencies.getUaiOptions();

  const checkKvAccessForAppSettings = async (settings: AppSettingResponse[]): Promise<void> => {
    const missingByKv = new Map<string, AppSettingResponse[]>();

    for (const setting of settings) {
      if (setting.isKeyVaultReference && setting.keyVaultResourceId && setting.hasKeyVaultAccess === false) {
        const existing = missingByKv.get(setting.keyVaultResourceId);
        if (existing) {
          existing.push(setting);
        } else {
          missingByKv.set(setting.keyVaultResourceId, [setting]);
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
      const settings = await appSettingService.getByResourceId(dependencies.getResourceId());
      appSettings.set(settings);
      await checkKvAccessForAppSettings(settings);
    } catch {
      errorKey.set('RESOURCE_EDIT.APP_SETTINGS.LOAD_ERROR');
    } finally {
      isLoading.set(false);
    }
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

    dialogRef.afterClosed().subscribe((result?: AppSettingResponse) => {
      if (result) {
        appSettings.update((currentSettings) => [...currentSettings, result]);
      }
    });
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

    dialogRef.afterClosed().subscribe((result?: AppSettingResponse[]) => {
      if (result?.length) {
        appSettings.update((currentSettings) => [...currentSettings, ...result]);
      }
    });
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

    dialogRef.afterClosed().subscribe((result?: AppSettingResponse) => {
      if (result) {
        appSettings.update((currentSettings) => currentSettings.map((currentSetting) => currentSetting.id === result.id ? result : currentSetting));
      }
    });
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

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (confirmed) {
        await removeAppSetting(setting.id);
      }
    });
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
    appSettings,
    isLoading,
    errorKey,
    hasWarning,
    kvMissingRoleEntries,
    groupedSettings,
    load,
    openAddDialog,
    openImportDialog,
    openEditStaticDialog,
    openRemoveDialog,
    resolveSourceName,
    resolveSourceType,
    assignedUai,
    uaiOptions,
    setKvEntryIdentityType,
    setKvEntryUaiId,
    createNewUaiForEntry,
    assignKvRole,
  };
}