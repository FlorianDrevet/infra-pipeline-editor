import { computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ProjectPipelineVariableGroupResponse } from '../../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../../shared/services/project.service';
import { AddVariableGroupDialogComponent } from '../../add-variable-group-dialog/add-variable-group-dialog.component';
import { ConfigDetailVariableGroupsSection } from './config-detail-variable-groups-section.interface';

interface ConfigDetailVariableGroupsSectionControllerDependencies {
  getProjectId(): string | null;
  getConfigName(): string;
}

export function createConfigDetailVariableGroupsSectionController(
  dependencies: ConfigDetailVariableGroupsSectionControllerDependencies,
): ConfigDetailVariableGroupsSection {
  const dialog = inject(MatDialog);
  const projectService = inject(ProjectService);

  const variableGroups = signal<ProjectPipelineVariableGroupResponse[]>([]);
  const isLoading = signal(false);
  const errorKey = signal('');
  const isLoaded = signal(false);

  const configVariableGroups = computed(() => {
    const configName = dependencies.getConfigName();
    if (!configName) {
      return variableGroups();
    }

    return variableGroups()
      .map((group) => ({
        ...group,
        variables: group.variables.filter((variable) => variable.configName === configName),
      }))
      .filter((group) => group.variables.length > 0);
  });

  const reset = (): void => {
    variableGroups.set([]);
    isLoading.set(false);
    errorKey.set('');
    isLoaded.set(false);
  };

  const load = async (): Promise<void> => {
    const projectId = dependencies.getProjectId();
    if (!projectId) {
      return;
    }

    isLoading.set(true);
    errorKey.set('');
    try {
      const groups = await projectService.getPipelineVariableGroups(projectId);
      variableGroups.set(groups.map((group) => ({ ...group, variables: group.variables ?? [] })));
      isLoaded.set(true);
    } catch {
      errorKey.set('CONFIG_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
    } finally {
      isLoading.set(false);
    }
  };

  const openAddDialog = (): void => {
    const dialogRef = dialog.open(AddVariableGroupDialogComponent, {
      width: '420px',
    });

    dialogRef.afterClosed().subscribe((groupName?: string) => {
      void handleAddDialogClosed(groupName);
    });
  };

  const handleAddDialogClosed = async (groupName?: string): Promise<void> => {
    const projectId = dependencies.getProjectId();
    if (!groupName || !projectId) {
      return;
    }

    errorKey.set('');
    try {
      const newGroup = await projectService.addPipelineVariableGroup(projectId, { groupName });
      variableGroups.update((currentGroups) => [...currentGroups, { ...newGroup, variables: newGroup.variables ?? [] }]);
    } catch {
      errorKey.set('CONFIG_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
    }
  };

  const openRemoveDialog = (group: ProjectPipelineVariableGroupResponse): void => {
    const dialogData: ConfirmDialogData = {
      titleKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      messageKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.CONFIRM_DELETE_GROUP',
      confirmKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      cancelKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.DIALOG_CANCEL',
    };

    const dialogRef = dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: dialogData,
    });

    dialogRef.afterClosed().subscribe((confirmed?: boolean) => {
      void handleRemoveDialogClosed(group, confirmed);
    });
  };

  const handleRemoveDialogClosed = async (
    group: ProjectPipelineVariableGroupResponse,
    confirmed?: boolean,
  ): Promise<void> => {
      const projectId = dependencies.getProjectId();
      if (!confirmed || !projectId) {
        return;
      }

      errorKey.set('');
      try {
        await projectService.removePipelineVariableGroup(projectId, group.id);
        variableGroups.update((currentGroups) => currentGroups.filter((currentGroup) => currentGroup.id !== group.id));
      } catch {
        errorKey.set('CONFIG_DETAIL.PIPELINE_VARIABLES.ERROR_REMOVE_GROUP');
      }
    };
  };

  return {
    configVariableGroups,
    isLoading,
    errorKey,
    isLoaded,
    reset,
    load,
    openAddDialog,
    openRemoveDialog,
  };
}