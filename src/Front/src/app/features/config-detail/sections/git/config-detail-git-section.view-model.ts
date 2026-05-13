import { InfraConfigRepositoryResponse, ConfigLayoutMode } from '../../../../shared/interfaces/infra-config-repository.interface';
import { RepositoryContentKind } from '../../../../shared/interfaces/project-repository.interface';

export interface ConfigDetailGitSectionViewModel {
  gitActionError: string;
  layoutMode: ConfigLayoutMode | null;
  layoutModeSaving: boolean;
  canWrite: boolean;
  actionRepoId: string | null;
  allInOneRepo: InfraConfigRepositoryResponse | null;
  splitSlots: ReadonlyArray<{
    kind: RepositoryContentKind;
    labelKey: string;
    repo: InfraConfigRepositoryResponse | null;
  }>;
  onSetLayoutMode: (mode: ConfigLayoutMode) => void;
  onOpenAllInOne: () => void;
  onOpenSlot: (kind: RepositoryContentKind, repo: InfraConfigRepositoryResponse | null) => void;
  onRemoveRepository: (repo: InfraConfigRepositoryResponse) => void;
}