export type GeneratedArtifactArchiveKind = 'bicep' | 'pipeline' | 'bootstrap';
export type GeneratedArtifactBucketPrefix = 'infra' | 'app';

export interface GeneratedArtifactArchiveSourceSpec {
  archiveKind: GeneratedArtifactArchiveKind;
  filterPrefix?: GeneratedArtifactBucketPrefix;
}

export type GeneratedArtifactEntryPathResolutionStatus = 'resolved' | 'ignored' | 'unsafe';

export interface GeneratedArtifactEntryPathResolution {
  status: GeneratedArtifactEntryPathResolutionStatus;
  path?: string;
}

export function resolveGeneratedArtifactEntryPath(
  entryPath: string,
  source: GeneratedArtifactArchiveSourceSpec,
): GeneratedArtifactEntryPathResolution {
  const sanitizedEntryPath = sanitizeGeneratedArtifactArchivePath(entryPath);
  if (!sanitizedEntryPath) {
    return { status: 'unsafe' };
  }

  const bucketRelativePath = stripGeneratedArtifactBucketPrefix(sanitizedEntryPath, source.filterPrefix);
  if (!bucketRelativePath) {
    return { status: 'ignored' };
  }

  const normalizedPath = normalizeGeneratedArtifactRepoRelativePath(bucketRelativePath, source.archiveKind);
  return normalizedPath
    ? { status: 'resolved', path: normalizedPath }
    : { status: 'unsafe' };
}

export function tryResolveGeneratedArtifactEntryPath(
  entryPath: string,
  source: GeneratedArtifactArchiveSourceSpec,
): string | null {
  const resolution = resolveGeneratedArtifactEntryPath(entryPath, source);
  return resolution.status === 'resolved'
    ? resolution.path ?? null
    : null;
}

export function normalizeGeneratedArtifactRepoRelativePath(
  relativePath: string,
  archiveKind: GeneratedArtifactArchiveKind,
): string | null {
  const normalizedPath = sanitizeGeneratedArtifactArchivePath(relativePath);
  if (!normalizedPath) {
    return null;
  }

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
  if (!filterPrefix) {
    return entryPath;
  }

  const bucketPrefix = `${filterPrefix}/`;
  if (!entryPath.startsWith(bucketPrefix)) {
    return null;
  }

  const strippedPath = entryPath.slice(bucketPrefix.length);
  return strippedPath.length > 0 ? strippedPath : null;
}

function sanitizeGeneratedArtifactArchivePath(path: string): string | null {
  if (!path) {
    return null;
  }

  const normalizedSeparators = path.split('\\').join('/');
  if (normalizedSeparators.startsWith('/') || hasWindowsDrivePrefix(normalizedSeparators)) {
    return null;
  }

  const sanitizedSegments: string[] = [];
  for (const segment of normalizedSeparators.split('/')) {
    if (!segment || segment === '.') {
      continue;
    }

    if (segment === '..' || !isSafeGeneratedArtifactPathSegment(segment)) {
      return null;
    }

    sanitizedSegments.push(segment);
  }

  return sanitizedSegments.length > 0 ? sanitizedSegments.join('/') : null;
}

function hasWindowsDrivePrefix(path: string): boolean {
  return path.length >= 3
    && isAsciiLetter(path[0])
    && path[1] === ':'
    && path[2] === '/';
}

function isAsciiLetter(character: string): boolean {
  const code = character.charCodeAt(0);
  return (code >= 65 && code <= 90) || (code >= 97 && code <= 122);
}

function isSafeGeneratedArtifactPathSegment(segment: string): boolean {
  if (segment.endsWith(' ') || segment.endsWith('.')) {
    return false;
  }

  for (const character of segment) {
    const code = character.charCodeAt(0);
    if (code < 32 || code === 127 || character === ':') {
      return false;
    }
  }

  return true;
}