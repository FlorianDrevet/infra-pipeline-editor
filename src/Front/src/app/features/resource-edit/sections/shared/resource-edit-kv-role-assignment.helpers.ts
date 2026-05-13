import { Signal, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import {
  CompactSelectOption,
} from '../../../../shared/components/compact-select/compact-select.component';
import { CheckKeyVaultAccessResponse } from '../../../../shared/interfaces/app-setting.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { UserAssignedIdentityResponse } from '../../../../shared/interfaces/user-assigned-identity.interface';
import { RoleAssignmentService } from '../../../../shared/services/role-assignment.service';
import { CreateUaiDialogComponent } from '../../create-uai-dialog/create-uai-dialog.component';
import { ResourceEditKvMissingRoleEntry } from './resource-edit-kv-missing-role-entry.interface';

type ResourceEditManagedIdentityType = ResourceEditKvMissingRoleEntry['selectedIdentityType'];

interface ResourceEditKvAccessSourceItem {
  readonly isKeyVaultReference: boolean;
  readonly keyVaultResourceId?: string | null;
  readonly hasKeyVaultAccess?: boolean | null;
}

interface ResourceEditKvRoleAssignmentHelperDependencies {
  readonly dialog: MatDialog;
  readonly roleAssignmentService: RoleAssignmentService;
  readonly resourceId: string;
  readonly getAllResources: () => AzureResourceResponse[];
  readonly getAssignedUai: () => { identityId: string; identityName: string } | null;
  readonly getUaiOptions: () => CompactSelectOption[];
  readonly getResourceContext: () => { resourceGroupId: string; location: string } | null;
  readonly reloadRoleAssignments: () => Promise<void>;
  readonly reloadAllResources: () => Promise<void>;
  readonly reloadEntries: () => Promise<void>;
  readonly checkKeyVaultAccess: (resourceId: string, keyVaultResourceId: string) => Promise<CheckKeyVaultAccessResponse>;
}

interface ResourceEditKvRoleAssignmentState {
  readonly kvMissingRoleEntries: Signal<ResourceEditKvMissingRoleEntry[]>;
  readonly resolveSourceName: (sourceResourceId: string) => string;
  readonly resolveSourceType: (sourceResourceId: string) => string;
  readonly assignedUai: () => { identityId: string; identityName: string } | null;
  readonly uaiOptions: () => CompactSelectOption[];
  readonly refreshKvMissingRoleEntries: <TItem extends ResourceEditKvAccessSourceItem>(items: ReadonlyArray<TItem>) => Promise<void>;
  readonly setKvEntryIdentityType: (kvId: string, type: ResourceEditManagedIdentityType) => void;
  readonly setKvEntryUaiId: (kvId: string, uaiId: string | null) => void;
  readonly createNewUaiForEntry: (kvId: string) => void;
  readonly assignKvRole: (entry: ResourceEditKvMissingRoleEntry) => Promise<void>;
}

export function createResourceEditKvRoleAssignmentState(
  dependencies: ResourceEditKvRoleAssignmentHelperDependencies,
): ResourceEditKvRoleAssignmentState {
  const kvMissingRoleEntries = signal<ResourceEditKvMissingRoleEntry[]>([]);

  const resolveSourceName = (sourceResourceId: string): string =>
    dependencies.getAllResources().find((resource) => resource.id === sourceResourceId)?.name ?? sourceResourceId;

  const resolveSourceType = (sourceResourceId: string): string =>
    dependencies.getAllResources().find((resource) => resource.id === sourceResourceId)?.resourceType ?? '';

  const assignedUai = (): { identityId: string; identityName: string } | null => dependencies.getAssignedUai();
  const uaiOptions = (): CompactSelectOption[] => dependencies.getUaiOptions();

  const refreshKvMissingRoleEntries = async <TItem extends ResourceEditKvAccessSourceItem>(items: ReadonlyArray<TItem>): Promise<void> => {
    const missingByKeyVault = new Map<string, TItem[]>();

    for (const item of items) {
      if (!item.isKeyVaultReference || !item.keyVaultResourceId || item.hasKeyVaultAccess !== false) {
        continue;
      }

      const existingItems = missingByKeyVault.get(item.keyVaultResourceId);
      if (existingItems) {
        existingItems.push(item);
      } else {
        missingByKeyVault.set(item.keyVaultResourceId, [item]);
      }
    }

    if (missingByKeyVault.size === 0) {
      kvMissingRoleEntries.set([]);
      return;
    }

    const initialEntries = [...missingByKeyVault.entries()].map(([keyVaultResourceId, affectedItems]) => ({
      keyVaultResourceId,
      keyVaultName: resolveSourceName(keyVaultResourceId),
      missingRoleName: null,
      missingRoleDefinitionId: null,
      affectedSettingsCount: affectedItems.length,
      checking: true,
      assigning: false,
      selectedIdentityType: 'UserAssigned' as const,
      selectedUaiId: resolveDefaultSelectedUaiId(assignedUai(), uaiOptions()),
    } satisfies ResourceEditKvMissingRoleEntry));

    kvMissingRoleEntries.set(initialEntries);

    const resolvedEntries = await Promise.all(initialEntries.map(async (entry) => {
      try {
        const result = await dependencies.checkKeyVaultAccess(dependencies.resourceId, entry.keyVaultResourceId);
        return {
          ...entry,
          missingRoleName: result.missingRoleName ?? null,
          missingRoleDefinitionId: result.missingRoleDefinitionId ?? null,
          checking: false,
        } satisfies ResourceEditKvMissingRoleEntry;
      } catch {
        return {
          ...entry,
          checking: false,
        } satisfies ResourceEditKvMissingRoleEntry;
      }
    }));

    kvMissingRoleEntries.set(resolvedEntries);
  };

  const setKvEntryIdentityType = (kvId: string, type: ResourceEditManagedIdentityType): void => {
    kvMissingRoleEntries.update((currentEntries) => currentEntries.map((entry) => (
      entry.keyVaultResourceId === kvId
        ? {
            ...entry,
            selectedIdentityType: type,
            selectedUaiId: type === 'SystemAssigned' ? null : entry.selectedUaiId,
          }
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

    const dialogRef = dependencies.dialog.open(CreateUaiDialogComponent, {
      data: resourceContext,
      width: '420px',
    });

    const handleDialogClose = async (result: UserAssignedIdentityResponse | undefined): Promise<void> => {
      if (!result) {
        return;
      }

      await dependencies.reloadAllResources();
      setKvEntryUaiId(kvId, result.id);
    };

    dialogRef.afterClosed().subscribe((result: UserAssignedIdentityResponse | undefined) => {
      handleDialogClose(result).catch(() => undefined);
    });
  };

  const assignKvRole = async (entry: ResourceEditKvMissingRoleEntry): Promise<void> => {
    const roleDefinitionId = entry.missingRoleDefinitionId;
    if (!roleDefinitionId) {
      return;
    }

    kvMissingRoleEntries.update((currentEntries) => currentEntries.map((currentEntry) => (
      currentEntry.keyVaultResourceId === entry.keyVaultResourceId
        ? { ...currentEntry, assigning: true }
        : currentEntry
    )));

    try {
      await dependencies.roleAssignmentService.add(dependencies.resourceId, {
        targetResourceId: entry.keyVaultResourceId,
        managedIdentityType: entry.selectedIdentityType,
        roleDefinitionId,
        userAssignedIdentityId: entry.selectedIdentityType === 'UserAssigned'
          ? resolveSelectedUaiId(entry, assignedUai())
          : undefined,
      });

      await dependencies.reloadEntries();
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
    kvMissingRoleEntries,
    resolveSourceName,
    resolveSourceType,
    assignedUai,
    uaiOptions,
    refreshKvMissingRoleEntries,
    setKvEntryIdentityType,
    setKvEntryUaiId,
    createNewUaiForEntry,
    assignKvRole,
  };
}

function resolveDefaultSelectedUaiId(
  assignedIdentity: { identityId: string; identityName: string } | null,
  availableUaiOptions: ReadonlyArray<CompactSelectOption>,
): string | null {
  if (assignedIdentity) {
    return assignedIdentity.identityId;
  }

  if (availableUaiOptions.length !== 1) {
    return null;
  }

  const onlyOptionValue = availableUaiOptions[0]?.value;
  return onlyOptionValue === null || onlyOptionValue === undefined
    ? null
    : String(onlyOptionValue);
}

function resolveSelectedUaiId(
  entry: ResourceEditKvMissingRoleEntry,
  assignedIdentity: { identityId: string; identityName: string } | null,
): string | undefined {
  const selectedIdentityId = assignedIdentity?.identityId ?? entry.selectedUaiId;
  return selectedIdentityId ?? undefined;
}