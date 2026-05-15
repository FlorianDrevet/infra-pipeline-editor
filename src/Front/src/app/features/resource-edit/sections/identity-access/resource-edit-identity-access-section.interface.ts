import { Signal } from '@angular/core';

import { CompactSelectOption } from '../../../../shared/components/compact-select/compact-select.component';
import {
  IdentityRoleAssignmentResponse,
  RoleAssignmentResponse,
} from '../../../../shared/interfaces/role-assignment.interface';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';

export interface ResourceEditIdentityAccessSection {
  readonly allResources: Signal<AzureResourceResponse[]>;
  readonly availableContainerRegistries: Signal<AzureResourceResponse[]>;
  readonly availableLogAnalyticsWorkspaces: Signal<AzureResourceResponse[]>;
  readonly availableUserAssignedIdentities: Signal<AzureResourceResponse[]>;
  readonly uaiOptions: Signal<CompactSelectOption[]>;
  readonly assignedUai: Signal<{ identityId: string; identityName: string } | null>;
  readonly switchableIdentities: Signal<AzureResourceResponse[]>;

  readonly roleAssignments: Signal<RoleAssignmentResponse[]>;
  readonly roleAssignmentsLoading: Signal<boolean>;
  readonly roleAssignmentsError: Signal<string>;
  readonly groupedRoleAssignments: Signal<{
    systemAssigned: RoleAssignmentResponse[];
    uaiGroups: Array<{
      identityId: string;
      identityName: string;
      assignments: RoleAssignmentResponse[];
    }>;
  }>;

  readonly identityRoleAssignments: Signal<IdentityRoleAssignmentResponse[]>;
  readonly identityRoleAssignmentsLoading: Signal<boolean>;
  readonly identityRoleAssignmentsError: Signal<string>;
  readonly usedByResources: Signal<Array<{
    sourceResourceId: string;
    sourceResourceName: string;
    sourceResourceType: string;
    assignments: IdentityRoleAssignmentResponse[];
  }>>;

  loadAllResources(): Promise<void>;
  loadRoleAssignments(): Promise<void>;
  loadIdentityRoleAssignments(): Promise<void>;
  toggleUaiExpand(identityId: string): void;
  isUaiExpanded(identityId: string): boolean;
  resolveTargetName(targetResourceId: string): string;
  resolveTargetType(targetResourceId: string): string;
  resolveRoleName(roleDefinitionId: string): string;
  resolveRoleDocUrl(roleDefinitionId: string): string;
  roleRequiresUserAssignedIdentity(roleDefinitionId: string): boolean;
  openAddRoleAssignmentDialog(): void;
  assignUaiToResource(identityId: string): Promise<void>;
  openUnassignUaiDialog(uai: { identityId: string; identityName: string }): void;
  switchToSystemAssigned(assignment: RoleAssignmentResponse): Promise<void>;
  switchToUserAssignedIdentity(assignment: RoleAssignmentResponse, identityId: string): Promise<void>;
  openRemoveRoleAssignmentDialog(assignment: RoleAssignmentResponse): Promise<void>;
  openUnlinkResourceFromIdentityDialog(group: {
    sourceResourceId: string;
    sourceResourceName: string;
    sourceResourceType: string;
    assignments: IdentityRoleAssignmentResponse[];
  }): void;
}