import { Signal } from '@angular/core';

import { ProjectPipelineVariableGroupResponse } from '../../../../shared/interfaces/project.interface';

export interface ConfigDetailVariableGroupsSection {
  readonly configVariableGroups: Signal<ProjectPipelineVariableGroupResponse[]>;
  readonly isLoading: Signal<boolean>;
  readonly errorKey: Signal<string>;
  readonly isLoaded: Signal<boolean>;

  reset(): void;
  load(): Promise<void>;
  openAddDialog(): void;
  openRemoveDialog(group: ProjectPipelineVariableGroupResponse): void;
}