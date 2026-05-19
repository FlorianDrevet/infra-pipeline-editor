import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { ProjectRepositoryResponse } from '../../shared/interfaces/project-repository.interface';

export interface ProjectDetailSplitRepoTargets {
  readonly infraRepositoryId: string;
  readonly codeRepositoryId: string;
  readonly infraRepositoryLabel: string;
  readonly codeRepositoryLabel: string;
}

const MAX_PROJECT_ARCHIVE_SOURCE_BYTES = 10 * 1024 * 1024;
const MAX_PROJECT_ARCHIVE_ENTRY_BYTES = 2 * 1024 * 1024;
const MAX_PROJECT_ARCHIVE_TOTAL_BYTES = 25 * 1024 * 1024;

export function resolveProjectDetailSplitRepoTargets(project: ProjectResponse): ProjectDetailSplitRepoTargets | null {
  const repositories = project.repositories ?? [];
  const infraRepository = repositories.find((repository) => repository.contentKinds?.includes('Infrastructure'));
  const codeRepository = repositories.find((repository) => repository.contentKinds?.includes('ApplicationCode'));

  return infraRepository && codeRepository
    ? {
        infraRepositoryId: infraRepository.id,
        codeRepositoryId: codeRepository.id,
        infraRepositoryLabel: getProjectDetailRepositoryLabel(infraRepository),
        codeRepositoryLabel: getProjectDetailRepositoryLabel(codeRepository),
      }
    : null;
}

function getProjectDetailRepositoryLabel(repository: ProjectRepositoryResponse): string {
  if (repository.owner && repository.repositoryName) {
    return `${repository.owner}/${repository.repositoryName}`;
  }

  return repository.repositoryName ?? repository.repositoryUrl ?? repository.id;
}

export function tryGetProjectArchiveEntryUncompressedSize(entry: object): number | null {
  const uncompressedSize = (entry as { _data?: { uncompressedSize?: number } })._data?.uncompressedSize;
  return typeof uncompressedSize === 'number' && Number.isFinite(uncompressedSize)
    ? uncompressedSize
    : null;
}

export function ensureProjectArchiveSourceSizeWithinLimits(sourceSize: number): void {
  if (sourceSize > MAX_PROJECT_ARCHIVE_SOURCE_BYTES) {
    throw new Error('Generated artifact archive exceeds the maximum allowed compressed size.');
  }
}

export function ensureProjectArchiveEntrySizeWithinLimits(entrySize: number, currentTotalExtractedBytes: number): void {
  if (entrySize > MAX_PROJECT_ARCHIVE_ENTRY_BYTES) {
    throw new Error('Generated artifact archive contains a file that exceeds the maximum allowed size.');
  }

  if (currentTotalExtractedBytes + entrySize > MAX_PROJECT_ARCHIVE_TOTAL_BYTES) {
    throw new Error('Generated artifact archive exceeds the maximum allowed extracted size.');
  }
}