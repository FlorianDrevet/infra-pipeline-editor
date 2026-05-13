import { ProjectResponse } from '../../shared/interfaces/project.interface';

export interface ProjectDetailSplitRepoAliases {
  readonly infraAlias: string;
  readonly codeAlias: string;
}

const MAX_PROJECT_ARCHIVE_SOURCE_BYTES = 10 * 1024 * 1024;
const MAX_PROJECT_ARCHIVE_ENTRY_BYTES = 2 * 1024 * 1024;
const MAX_PROJECT_ARCHIVE_TOTAL_BYTES = 25 * 1024 * 1024;

export function resolveProjectDetailSplitRepoAliases(project: ProjectResponse): ProjectDetailSplitRepoAliases | null {
  const repositories = project.repositories ?? [];
  const infraAlias = repositories.find((repository) => repository.contentKinds?.includes('Infrastructure'))?.alias;
  const codeAlias = repositories.find((repository) => repository.contentKinds?.includes('ApplicationCode'))?.alias;

  return infraAlias && codeAlias
    ? { infraAlias, codeAlias }
    : null;
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