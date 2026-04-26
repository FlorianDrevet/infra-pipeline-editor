export type GeneratedArtifactArchiveKind = 'bicep' | 'pipeline' | 'bootstrap';
export type GeneratedArtifactBucketPrefix = 'infra' | 'app';

export interface GeneratedArtifactArchiveSourceSpec {
  archiveKind: GeneratedArtifactArchiveKind;
  filterPrefix?: GeneratedArtifactBucketPrefix;
}

export function tryResolveGeneratedArtifactEntryPath(
  entryPath: string,
  source: GeneratedArtifactArchiveSourceSpec,
): string | null {
  const bucketRelativePath = stripGeneratedArtifactBucketPrefix(entryPath, source.filterPrefix);
  if (!bucketRelativePath) {
    return null;
  }

  return normalizeGeneratedArtifactRepoRelativePath(bucketRelativePath, source.archiveKind);
}

export function normalizeGeneratedArtifactRepoRelativePath(
  relativePath: string,
  archiveKind: GeneratedArtifactArchiveKind,
): string {
  const normalizedPath = relativePath.replace(/^\/+/, '');

  return archiveKind === 'bootstrap'
    ? toBootstrapRepoRelativePath(normalizedPath)
    : normalizedPath;
}

export function toBootstrapRepoRelativePath(relativePath: string): string {
  const normalizedPath = relativePath.replace(/^\/+/, '');

  return normalizedPath.startsWith('.azuredevops/')
    ? normalizedPath
    : `.azuredevops/${normalizedPath}`;
}

function stripGeneratedArtifactBucketPrefix(
  entryPath: string,
  filterPrefix?: GeneratedArtifactBucketPrefix,
): string | null {
  const normalizedPath = entryPath.replace(/^\/+/, '');

  if (!filterPrefix) {
    return normalizedPath;
  }

  const bucketPrefix = `${filterPrefix}/`;
  if (!normalizedPath.startsWith(bucketPrefix)) {
    return null;
  }

  const strippedPath = normalizedPath.slice(bucketPrefix.length);
  return strippedPath.length > 0 ? strippedPath : null;
}