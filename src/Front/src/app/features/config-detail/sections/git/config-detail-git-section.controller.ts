import { computed, inject, signal, Signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { InfrastructureConfigResponse } from '../../../../shared/interfaces/infra-config.interface';
import {
  ConfigLayoutMode,
  InfraConfigRepositoryResponse,
} from '../../../../shared/interfaces/infra-config-repository.interface';
import { RepositoryContentKind } from '../../../../shared/interfaces/project-repository.interface';
import { InfraConfigService } from '../../../../shared/services/infra-config.service';
import { ProjectService } from '../../../../shared/services/project.service';
import {
  InfraConfigRepositoryDialogComponent,
  InfraConfigRepositoryDialogData,
} from '../../infra-config-repository-dialog/infra-config-repository-dialog.component';
import { ConfigDetailGitSectionViewModel } from './config-detail-git-section.view-model';

export interface ConfigDetailGitSectionController {
  readonly viewModel: Signal<ConfigDetailGitSectionViewModel | null>;

  reset(): void;
}

interface ConfigDetailGitSectionControllerDependencies {
  getConfig(): InfrastructureConfigResponse | null;
  canWrite(): boolean;
  updateConfig(config: InfrastructureConfigResponse): void;
}

export function createConfigDetailGitSectionController(
  dependencies: ConfigDetailGitSectionControllerDependencies,
): ConfigDetailGitSectionController {
  const dialog = inject(MatDialog);
  const infraConfigService = inject(InfraConfigService);
  const projectService = inject(ProjectService);

  const gitActionError = signal('');
  const configRepoActionId = signal<string | null>(null);
  const configLayoutModeSaving = signal(false);

  const configLayoutMode = computed<ConfigLayoutMode | null>(() => {
    const mode = dependencies.getConfig()?.layoutMode;
    return mode === 'AllInOne' || mode === 'SplitInfraCode' ? mode : null;
  });

  const configRepositories = computed<InfraConfigRepositoryResponse[]>(() => dependencies.getConfig()?.repositories ?? []);

  const configAllInOneRepo = computed<InfraConfigRepositoryResponse | null>(() => configRepositories()[0] ?? null);

  const configSplitSlots = computed(() => {
    const repositories = configRepositories();
    return [
      {
        kind: 'Infrastructure',
        labelKey: 'CONFIG_DETAIL.REPOSITORIES.SLOT_INFRASTRUCTURE',
        repo: repositories.find((repository) => repository.contentKinds.includes('Infrastructure')) ?? null,
      },
      {
        kind: 'ApplicationCode',
        labelKey: 'CONFIG_DETAIL.REPOSITORIES.SLOT_APPLICATION_CODE',
        repo: repositories.find((repository) => repository.contentKinds.includes('ApplicationCode')) ?? null,
      },
    ] satisfies Array<{
      kind: RepositoryContentKind;
      labelKey: string;
      repo: InfraConfigRepositoryResponse | null;
    }>;
  });

  const reloadConfig = async (configId: string): Promise<void> => {
    dependencies.updateConfig(await infraConfigService.getById(configId));
  };

  const setConfigLayoutMode = async (mode: ConfigLayoutMode): Promise<void> => {
    const config = dependencies.getConfig();
    if (!config || configLayoutMode() === mode) {
      return;
    }

    const previousConfig = config;
    gitActionError.set('');
    configLayoutModeSaving.set(true);
    dependencies.updateConfig({
      ...config,
      layoutMode: mode,
      repositories: [],
    });

    try {
      await projectService.setConfigLayoutMode(config.projectId, config.id, mode);
      await reloadConfig(config.id);
    } catch {
      dependencies.updateConfig(previousConfig);
      gitActionError.set('CONFIG_DETAIL.REPOSITORIES.LAYOUT_MODE_SAVE_ERROR');
    } finally {
      configLayoutModeSaving.set(false);
    }
  };

  const openConfigAllInOneDialog = (): void => {
    const config = dependencies.getConfig();
    if (!config) {
      return;
    }

    const existingRepository = configAllInOneRepo();
    const dialogData: InfraConfigRepositoryDialogData = {
      projectId: config.projectId,
      configId: config.id,
      mode: existingRepository ? 'edit' : 'create',
      existing: existingRepository ?? undefined,
      lockedKinds: ['Infrastructure', 'ApplicationCode'],
    };

    const dialogRef = dialog.open(InfraConfigRepositoryDialogComponent, {
      data: dialogData,
      width: '560px',
    });

    dialogRef.afterClosed().subscribe(async (result) => {
      if (result) {
        await reloadConfig(config.id);
      }
    });
  };

  const openConfigSlotDialog = (kind: RepositoryContentKind, repository: InfraConfigRepositoryResponse | null): void => {
    const config = dependencies.getConfig();
    if (!config) {
      return;
    }

    const dialogData: InfraConfigRepositoryDialogData = {
      projectId: config.projectId,
      configId: config.id,
      mode: repository ? 'edit' : 'create',
      existing: repository ?? undefined,
      lockedKinds: [kind],
    };

    const dialogRef = dialog.open(InfraConfigRepositoryDialogComponent, {
      data: dialogData,
      width: '560px',
    });

    dialogRef.afterClosed().subscribe(async (result) => {
      if (result) {
        await reloadConfig(config.id);
      }
    });
  };

  const removeConfigRepository = async (repository: InfraConfigRepositoryResponse): Promise<void> => {
    const config = dependencies.getConfig();
    if (!config) {
      return;
    }

    configRepoActionId.set(repository.id);
    try {
      await projectService.removeConfigRepository(config.projectId, config.id, repository.id);
      await reloadConfig(config.id);
    } catch {
      gitActionError.set('CONFIG_DETAIL.REPOSITORIES.DELETE_ERROR');
    } finally {
      configRepoActionId.set(null);
    }
  };

  const viewModel = computed<ConfigDetailGitSectionViewModel | null>(() => {
    if (!dependencies.getConfig()) {
      return null;
    }

    return {
      gitActionError: gitActionError(),
      layoutMode: configLayoutMode(),
      layoutModeSaving: configLayoutModeSaving(),
      canWrite: dependencies.canWrite(),
      actionRepoId: configRepoActionId(),
      allInOneRepo: configAllInOneRepo(),
      splitSlots: configSplitSlots(),
      onSetLayoutMode: (mode) => void setConfigLayoutMode(mode),
      onOpenAllInOne: openConfigAllInOneDialog,
      onOpenSlot: openConfigSlotDialog,
      onRemoveRepository: (repository) => void removeConfigRepository(repository),
    };
  });

  const reset = (): void => {
    gitActionError.set('');
    configRepoActionId.set(null);
    configLayoutModeSaving.set(false);
  };

  return {
    viewModel,
    reset,
  };
}