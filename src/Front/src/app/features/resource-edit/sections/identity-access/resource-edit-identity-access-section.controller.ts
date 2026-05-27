import { computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { CompactSelectOption } from '../../../../shared/components/compact-select/compact-select.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { InfrastructureConfigResponse } from '../../../../shared/interfaces/infra-config.interface';
import {
  AzureRoleDefinitionResponse,
  IdentityRoleAssignmentResponse,
  RoleAssignmentImpactResponse,
  RoleAssignmentResponse,
  UpdateRoleAssignmentIdentityRequest,
} from '../../../../shared/interfaces/role-assignment.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../../../shared/services/infra-config.service';
import { ProjectService } from '../../../../shared/services/project.service';
import { ResourceGroupService } from '../../../../shared/services/resource-group.service';
import { RoleAssignmentService } from '../../../../shared/services/role-assignment.service';
import { UserAssignedIdentityService } from '../../../../shared/services/user-assigned-identity.service';
import {
  AddRoleAssignmentDialogComponent,
  AddRoleAssignmentDialogData,
  AddRoleAssignmentDialogResult,
} from '../../add-role-assignment-dialog/add-role-assignment-dialog.component';
import {
  RoleAssignmentImpactDialogComponent,
  RoleAssignmentImpactDialogData,
} from '../../role-assignment-impact-dialog/role-assignment-impact-dialog.component';
import { ResourceEditIdentityAccessSection } from './resource-edit-identity-access-section.interface';

interface ResourceEditIdentityAccessSectionControllerDependencies {
  getConfigId(): string;
  getConfig(): InfrastructureConfigResponse | null;
  getResourceId(): string;
  getResource(): { name?: string; resourceGroupId?: string | null; location?: string | null } | null;
  isUserAssignedIdentity(): boolean;
  isAcrEnabled(): boolean;
  checkAcrPullAccess(): Promise<void>;
  getAcrPullIdentityId(): string | null;
  supportsAppSettings(): boolean;
  reloadAppSettings(): Promise<void>;
  supportsConfigKeys(): boolean;
  reloadConfigKeys(): Promise<void>;
}

export function createResourceEditIdentityAccessSectionController(
  dependencies: ResourceEditIdentityAccessSectionControllerDependencies,
): ResourceEditIdentityAccessSection {
  const dialog = inject(MatDialog);
  const infraConfigService = inject(InfraConfigService);
  const projectService = inject(ProjectService);
  const resourceGroupService = inject(ResourceGroupService);
  const roleAssignmentService = inject(RoleAssignmentService);
  const userAssignedIdentityService = inject(UserAssignedIdentityService);

  const allResources = signal<AzureResourceResponse[]>([]);
  const assignedIdentityId = signal<string | null>(null);
  const assignedIdentityName = signal<string | null>(null);
  const availableRoleDefs = signal<AzureRoleDefinitionResponse[]>([]);
  const expandedUaiIds = signal<Set<string>>(new Set<string>());

  const roleAssignments = signal<RoleAssignmentResponse[]>([]);
  const roleAssignmentsLoading = signal(false);
  const roleAssignmentsError = signal('');

  const identityRoleAssignments = signal<IdentityRoleAssignmentResponse[]>([]);
  const identityRoleAssignmentsLoading = signal(false);
  const identityRoleAssignmentsError = signal('');

  const availableUserAssignedIdentities = computed(() =>
    allResources().filter((resource) => resource.resourceType === 'UserAssignedIdentity'),
  );

  const uaiOptions = computed<CompactSelectOption[]>(() =>
    availableUserAssignedIdentities().map((identity) => ({ value: identity.id, label: identity.name })),
  );

  const assignedUai = computed(() => {
    const identityId = assignedIdentityId();
    const identityName = assignedIdentityName();
    if (!identityId) {
      return null;
    }

    return {
      identityId,
      identityName: identityName ?? identityId,
    };
  });

  const switchableIdentities = computed(() => {
    const assignedIdentity = assignedUai();
    if (!assignedIdentity) {
      return availableUserAssignedIdentities();
    }

    return availableUserAssignedIdentities().filter((identity) => identity.id !== assignedIdentity.identityId);
  });

  const availableContainerRegistries = computed(() =>
    allResources().filter((resource) => resource.resourceType === 'ContainerRegistry'),
  );

  const availableLogAnalyticsWorkspaces = computed(() =>
    allResources().filter((resource) => resource.resourceType === 'LogAnalyticsWorkspace'),
  );

  const resolveIdentityName = (identityId: string): string => {
    const identity = allResources().find((resource) => resource.id === identityId);
    if (identity) {
      return identity.name;
    }

    const isUuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(identityId);
    return isUuid ? `⚠ ${identityId.substring(0, 8)}…` : identityId;
  };

  const groupedRoleAssignments = computed(() => {
    const systemAssigned: RoleAssignmentResponse[] = [];
    const uaiGroups = new Map<string, { identityId: string; identityName: string; assignments: RoleAssignmentResponse[] }>();
    const acrPullIdentityId = dependencies.getAcrPullIdentityId();

    const assignedIdentity = assignedUai();
    if (assignedIdentity && assignedIdentity.identityId !== acrPullIdentityId) {
      uaiGroups.set(assignedIdentity.identityId, {
        identityId: assignedIdentity.identityId,
        identityName: assignedIdentity.identityName,
        assignments: [],
      });
    }

    for (const assignment of roleAssignments()) {
      // Hide role assignments belonging to the ACR pull identity (managed in ACR Authentication section)
      if (assignment.managedIdentityType === 'UserAssigned'
        && assignment.userAssignedIdentityId
        && assignment.userAssignedIdentityId === acrPullIdentityId) {
        continue;
      }

      if (assignment.managedIdentityType === 'UserAssigned' && assignment.userAssignedIdentityId) {
        const existingGroup = uaiGroups.get(assignment.userAssignedIdentityId);
        if (existingGroup) {
          existingGroup.assignments.push(assignment);
        } else {
          uaiGroups.set(assignment.userAssignedIdentityId, {
            identityId: assignment.userAssignedIdentityId,
            identityName: resolveIdentityName(assignment.userAssignedIdentityId),
            assignments: [assignment],
          });
        }

        continue;
      }

      systemAssigned.push(assignment);
    }

    return {
      systemAssigned,
      uaiGroups: [...uaiGroups.values()],
    };
  });

  const usedByResources = computed(() => {
    const groupedResources = new Map<string, {
      sourceResourceId: string;
      sourceResourceName: string;
      sourceResourceType: string;
      assignments: IdentityRoleAssignmentResponse[];
    }>();

    for (const assignment of identityRoleAssignments()) {
      if (assignment.sourceResourceId === dependencies.getResourceId()) {
        continue;
      }

      const existingGroup = groupedResources.get(assignment.sourceResourceId);
      if (existingGroup) {
        existingGroup.assignments.push(assignment);
      } else {
        groupedResources.set(assignment.sourceResourceId, {
          sourceResourceId: assignment.sourceResourceId,
          sourceResourceName: assignment.sourceResourceName,
          sourceResourceType: assignment.sourceResourceType,
          assignments: [assignment],
        });
      }
    }

    return [...groupedResources.values()];
  });

  const appendCreatedIdentities = (createdIdentities: AzureResourceResponse[]): void => {
    allResources.update((currentResources) => {
      const existingIds = new Set(currentResources.map((resourceItem) => resourceItem.id));
      const missingResources = createdIdentities.filter((identity) => !existingIds.has(identity.id));
      return missingResources.length > 0 ? [...currentResources, ...missingResources] : currentResources;
    });
  };

  const loadAllResources = async (): Promise<void> => {
    try {
      const projectId = dependencies.getConfig()?.projectId;

      // Start project resources fetch in parallel with per-group resources
      const projectResourcesPromise = projectId
        ? projectService.getProjectResources(projectId).catch(() => null)
        : Promise.resolve(null);

      const resourceGroups = await infraConfigService.getResourceGroups(dependencies.getConfigId());
      const resourceResults = await Promise.all(resourceGroups.map((resourceGroup) => resourceGroupService.getResources(resourceGroup.id)));
      const localResources = resourceResults.flat().filter((resource) => resource.id !== dependencies.getResourceId());

      if (!projectId) {
        allResources.set(localResources);
        return;
      }

      const projectResources = await projectResourcesPromise;
      if (!projectResources) {
        allResources.set(localResources);
        return;
      }

      const localResourceIds = new Set(localResources.map((resource) => resource.id));
      const crossConfigResources = projectResources
        .filter((projectResource) => (
          projectResource.configId !== dependencies.getConfigId()
          && !localResourceIds.has(projectResource.resourceId)
          && projectResource.resourceId !== dependencies.getResourceId()
        ))
        .map((projectResource) => ({
          id: projectResource.resourceId,
          resourceType: projectResource.resourceType,
          name: `${projectResource.resourceName} (${projectResource.configName})`,
          location: '',
        }));

      allResources.set([...localResources, ...crossConfigResources]);
    } catch {
      allResources.set([]);
    }
  };

  const loadRoleAssignments = async (): Promise<void> => {
    roleAssignmentsLoading.set(true);
    roleAssignmentsError.set('');

    try {
      const result = await roleAssignmentService.getByResourceId(dependencies.getResourceId());
      roleAssignments.set(result.roleAssignments);
      assignedIdentityId.set(result.assignedUserAssignedIdentityId);
      assignedIdentityName.set(result.assignedUserAssignedIdentityName);

      const assignedIdentityIdFromResult = result.assignedUserAssignedIdentityId;
      if (assignedIdentityIdFromResult) {
        expandedUaiIds.update((currentExpanded) => {
          if (currentExpanded.has(assignedIdentityIdFromResult)) {
            return currentExpanded;
          }

          const nextExpanded = new Set(currentExpanded);
          nextExpanded.add(assignedIdentityIdFromResult);
          return nextExpanded;
        });
      }

      const targetResourceIds = [...new Set(result.roleAssignments.map((assignment) => assignment.targetResourceId))];

      const roleDefinitionResults = await Promise.all(
        targetResourceIds.map((targetResourceId) =>
          roleAssignmentService.getAvailableRoleDefinitions(targetResourceId).catch(() => [] as AzureRoleDefinitionResponse[]),
        ),
      );
      const roleDefinitions = roleDefinitionResults.flat();

      availableRoleDefs.set(
        roleDefinitions.filter(
          (definition, index, definitions) => definitions.findIndex((candidate) => candidate.id === definition.id) === index,
        ),
      );
    } catch {
      roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.LOAD_ERROR');
    } finally {
      roleAssignmentsLoading.set(false);
    }
  };

  const loadIdentityRoleAssignments = async (): Promise<void> => {
    identityRoleAssignmentsLoading.set(true);
    identityRoleAssignmentsError.set('');

    try {
      identityRoleAssignments.set(await userAssignedIdentityService.getGrantedRoleAssignments(dependencies.getResourceId()));
    } catch {
      identityRoleAssignmentsError.set('RESOURCE_EDIT.GRANTED_RIGHTS.LOAD_ERROR');
    } finally {
      identityRoleAssignmentsLoading.set(false);
    }
  };

  const toggleUaiExpand = (identityId: string): void => {
    expandedUaiIds.update((currentExpanded) => {
      const nextExpanded = new Set(currentExpanded);
      if (nextExpanded.has(identityId)) {
        nextExpanded.delete(identityId);
      } else {
        nextExpanded.add(identityId);
      }

      return nextExpanded;
    });
  };

  const isUaiExpanded = (identityId: string): boolean => expandedUaiIds().has(identityId);

  const resolveTargetName = (targetResourceId: string): string =>
    allResources().find((resource) => resource.id === targetResourceId)?.name ?? targetResourceId;

  const resolveTargetType = (targetResourceId: string): string =>
    allResources().find((resource) => resource.id === targetResourceId)?.resourceType ?? '';

  const resolveRoleName = (roleDefinitionId: string): string =>
    availableRoleDefs().find((definition) => definition.id === roleDefinitionId)?.name ?? roleDefinitionId;

  const resolveRoleDocUrl = (roleDefinitionId: string): string =>
    availableRoleDefs().find((definition) => definition.id === roleDefinitionId)?.documentationUrl ?? '';

  const roleRequiresUserAssignedIdentity = (roleDefinitionId: string): boolean =>
    availableRoleDefs().find((definition) => definition.id === roleDefinitionId)?.requiresUserAssignedIdentity ?? false;

  const refreshAfterRoleMutation = async (): Promise<void> => {
    if (dependencies.isAcrEnabled()) {
      await dependencies.checkAcrPullAccess();
    }

    if (dependencies.supportsAppSettings()) {
      await dependencies.reloadAppSettings();
    }

    if (dependencies.supportsConfigKeys()) {
      await dependencies.reloadConfigKeys();
    }
  };

  const openAddRoleAssignmentDialog = (): void => {
    const resource = dependencies.getResource();
    const baseData = {
      sourceResourceId: dependencies.getResourceId(),
      currentResourceName: resource?.name ?? '',
      siblingResources: allResources(),
      resourceGroupId: resource?.resourceGroupId ?? '',
      configLocation: resource?.location ?? 'EastUS2',
    } satisfies Pick<AddRoleAssignmentDialogData, 'sourceResourceId' | 'currentResourceName' | 'siblingResources' | 'resourceGroupId' | 'configLocation'>;

    const dialogData: AddRoleAssignmentDialogData = dependencies.isUserAssignedIdentity()
      ? {
          ...baseData,
          isFromUserAssignedIdentity: true,
          userAssignedIdentityId: dependencies.getResourceId(),
          userAssignedIdentityName: resource?.name ?? '',
        }
      : {
          ...baseData,
        };

    const assignedIdentity = assignedUai();
    if (!dependencies.isUserAssignedIdentity() && assignedIdentity) {
      dialogData.assignedUserAssignedIdentityId = assignedIdentity.identityId;
      dialogData.assignedUserAssignedIdentityName = assignedIdentity.identityName;
    }

    const dialogRef = dialog.open(AddRoleAssignmentDialogComponent, {
      data: dialogData,
      width: '520px',
      maxHeight: '85vh',
    });

    dialogRef.afterClosed().subscribe(async (dialogResult?: AddRoleAssignmentDialogResult) => {
      if (!dialogResult) {
        return;
      }

      const addedRoleAssignment = dialogResult.roleAssignment;

      if (dialogResult.createdIdentities.length > 0) {
        appendCreatedIdentities(dialogResult.createdIdentities);
      }

      if (dependencies.isUserAssignedIdentity()) {
        await loadIdentityRoleAssignments();
      } else {
        roleAssignments.update((currentAssignments) => [...currentAssignments, addedRoleAssignment]);
        await loadRoleAssignments();
        await refreshAfterRoleMutation();
      }

      if (addedRoleAssignment.userAssignedIdentityId && !allResources().some((resourceItem) => resourceItem.id === addedRoleAssignment.userAssignedIdentityId)) {
        await loadAllResources();
      }
    });
  };

  const assignUaiToResource = async (identityId: string): Promise<void> => {
    roleAssignmentsError.set('');

    try {
      await roleAssignmentService.assignIdentity(dependencies.getResourceId(), identityId);
      await loadRoleAssignments();
    } catch {
      roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UPDATE_IDENTITY_ERROR');
    }
  };

  const unassignUaiFromResource = async (): Promise<void> => {
    roleAssignmentsError.set('');

    try {
      await roleAssignmentService.unassignIdentity(dependencies.getResourceId());
      await loadRoleAssignments();
    } catch {
      roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_ERROR');
    }
  };

  const openUnassignUaiDialog = (uai: { identityId: string; identityName: string }): void => {
    const dialogRef = dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_TITLE',
        messageKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_MESSAGE',
        messageParams: { identity: uai.identityName },
        confirmKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_CONFIRM',
        cancelKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.UNASSIGN_UAI_CANCEL',
      } satisfies ConfirmDialogData,
      width: '480px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (confirmed) {
        await unassignUaiFromResource();
      }
    });
  };

  const switchToSystemAssigned = async (assignment: RoleAssignmentResponse): Promise<void> => {
    roleAssignmentsError.set('');

    try {
      const updatedAssignment = await roleAssignmentService.updateIdentity(
        dependencies.getResourceId(),
        assignment.id,
        { managedIdentityType: 'SystemAssigned' },
      );

      roleAssignments.update((currentAssignments) =>
        currentAssignments.map((currentAssignment) => currentAssignment.id === updatedAssignment.id ? updatedAssignment : currentAssignment),
      );
    } catch {
      roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UPDATE_IDENTITY_ERROR');
    }
  };

  const switchToUserAssignedIdentity = async (assignment: RoleAssignmentResponse, identityId: string): Promise<void> => {
    roleAssignmentsError.set('');

    try {
      const updateRequest: UpdateRoleAssignmentIdentityRequest = {
        managedIdentityType: 'UserAssigned',
        userAssignedIdentityId: identityId,
      };

      const updatedAssignment = await roleAssignmentService.updateIdentity(
        dependencies.getResourceId(),
        assignment.id,
        updateRequest,
      );

      roleAssignments.update((currentAssignments) =>
        currentAssignments.map((currentAssignment) => currentAssignment.id === updatedAssignment.id ? updatedAssignment : currentAssignment),
      );
    } catch {
      roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.UPDATE_IDENTITY_ERROR');
    }
  };

  const removeRoleAssignment = async (roleAssignmentId: string): Promise<void> => {
    roleAssignmentsError.set('');

    try {
      await roleAssignmentService.remove(dependencies.getResourceId(), roleAssignmentId);
      roleAssignments.update((currentAssignments) => currentAssignments.filter((assignment) => assignment.id !== roleAssignmentId));
      await refreshAfterRoleMutation();
    } catch {
      roleAssignmentsError.set('RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_ERROR');
    }
  };

  const openRemoveRoleAssignmentDialog = async (assignment: RoleAssignmentResponse): Promise<void> => {
    const roleName = resolveRoleName(assignment.roleDefinitionId);
    const targetName = resolveTargetName(assignment.targetResourceId);

    let impactResult: RoleAssignmentImpactResponse;
    try {
      impactResult = await roleAssignmentService.analyzeImpact(dependencies.getResourceId(), assignment.id);
    } catch {
      const fallbackDialogRef = dialog.open(ConfirmDialogComponent, {
        data: {
          titleKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_TITLE',
          messageKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_MESSAGE',
          messageParams: { role: roleName, target: targetName },
          confirmKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_YES',
          cancelKey: 'RESOURCE_EDIT.ROLE_ASSIGNMENTS.REMOVE_CANCEL',
        } satisfies ConfirmDialogData,
        width: '420px',
      });

      fallbackDialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
        if (confirmed) {
          await removeRoleAssignment(assignment.id);
        }
      });

      return;
    }

    const filteredImpactResult = {
      ...impactResult,
      impacts: impactResult.impacts.filter((impact) => impact.impactType !== 'LastRoleToTarget'),
    };

    if (filteredImpactResult.impacts.length === 0) {
      await removeRoleAssignment(assignment.id);
      return;
    }

    const dialogRef = dialog.open(RoleAssignmentImpactDialogComponent, {
      data: {
        roleName,
        targetResourceName: targetName,
        impactResult: filteredImpactResult,
      } satisfies RoleAssignmentImpactDialogData,
      width: '520px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (confirmed) {
        await removeRoleAssignment(assignment.id);
      }
    });
  };

  const unlinkAllAssignmentsForResource = async (sourceResourceId: string): Promise<void> => {
    identityRoleAssignmentsError.set('');

    try {
      await userAssignedIdentityService.unlinkResource(dependencies.getResourceId(), sourceResourceId);
      await loadIdentityRoleAssignments();
    } catch {
      identityRoleAssignmentsError.set('RESOURCE_EDIT.USED_BY.UNLINK_ERROR');
    }
  };

  const openUnlinkResourceFromIdentityDialog = (group: {
    sourceResourceId: string;
    sourceResourceName: string;
    sourceResourceType: string;
    assignments: IdentityRoleAssignmentResponse[];
  }): void => {
    const dialogRef = dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.USED_BY.UNLINK_RESOURCE_TITLE',
        messageKey: 'RESOURCE_EDIT.USED_BY.UNLINK_RESOURCE_MESSAGE',
        messageParams: { resource: group.sourceResourceName },
        confirmKey: 'RESOURCE_EDIT.USED_BY.UNLINK_YES',
        cancelKey: 'RESOURCE_EDIT.USED_BY.UNLINK_CANCEL',
      } satisfies ConfirmDialogData,
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (confirmed) {
        await unlinkAllAssignmentsForResource(group.sourceResourceId);
      }
    });
  };

  return {
    allResources,
    availableContainerRegistries,
    availableLogAnalyticsWorkspaces,
    availableUserAssignedIdentities,
    uaiOptions,
    assignedUai,
    switchableIdentities,
    roleAssignments,
    roleAssignmentsLoading,
    roleAssignmentsError,
    groupedRoleAssignments,
    identityRoleAssignments,
    identityRoleAssignmentsLoading,
    identityRoleAssignmentsError,
    usedByResources,
    loadAllResources,
    loadRoleAssignments,
    loadIdentityRoleAssignments,
    toggleUaiExpand,
    isUaiExpanded,
    resolveTargetName,
    resolveTargetType,
    resolveRoleName,
    resolveRoleDocUrl,
    roleRequiresUserAssignedIdentity,
    openAddRoleAssignmentDialog,
    assignUaiToResource,
    openUnassignUaiDialog,
    switchToSystemAssigned,
    switchToUserAssignedIdentity,
    openRemoveRoleAssignmentDialog,
    openUnlinkResourceFromIdentityDialog,
  };
}