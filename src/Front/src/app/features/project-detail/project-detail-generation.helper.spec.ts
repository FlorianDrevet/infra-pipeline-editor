import { ProjectResponse } from '../../shared/interfaces/project.interface';

import {
  ensureProjectArchiveEntrySizeWithinLimits,
  ensureProjectArchiveSourceSizeWithinLimits,
  resolveProjectDetailSplitRepoTargets,
  tryGetProjectArchiveEntryUncompressedSize,
} from './project-detail-generation.helper';
import { ProjectRepositoryResponse } from '../../shared/interfaces/project-repository.interface';

describe('project detail generation helper', () => {
  it('returns the infrastructure and application repository ids for split infra/code layouts', () => {
    const targets = resolveProjectDetailSplitRepoTargets(createProject({
      repositories: [
        createRepository({ id: 'repo-infra', repositoryName: 'infra', contentKinds: ['Infrastructure'] }),
        createRepository({ id: 'repo-app', repositoryName: 'app', contentKinds: ['ApplicationCode'] }),
      ],
    }));

    expect(targets).toEqual({
      infraRepositoryId: 'repo-infra',
      codeRepositoryId: 'repo-app',
      infraRepositoryLabel: 'org/infra',
      codeRepositoryLabel: 'org/app',
    });
  });

  it('returns null when one split repository slot is missing', () => {
    const targets = resolveProjectDetailSplitRepoTargets(createProject({
      repositories: [
        createRepository({ id: 'repo-infra', repositoryName: 'infra', contentKinds: ['Infrastructure'] }),
      ],
    }));

    expect(targets).toBeNull();
  });

  it('reads the uncompressed size when JSZip exposes it on the entry metadata', () => {
    const uncompressedSize = tryGetProjectArchiveEntryUncompressedSize({
      _data: { uncompressedSize: 512 },
    });

    expect(uncompressedSize).toBe(512);
  });

  it('rejects compressed source archives that exceed the allowed size', () => {
    expect(() => ensureProjectArchiveSourceSizeWithinLimits(10 * 1024 * 1024 + 1)).toThrowError(
      'Generated artifact archive exceeds the maximum allowed compressed size.',
    );
  });

  it('rejects archive entries that exceed per-file or cumulative limits', () => {
    expect(() => ensureProjectArchiveEntrySizeWithinLimits(2 * 1024 * 1024 + 1, 0)).toThrowError(
      'Generated artifact archive contains a file that exceeds the maximum allowed size.',
    );

    expect(() => ensureProjectArchiveEntrySizeWithinLimits(1024, 25 * 1024 * 1024)).toThrowError(
      'Generated artifact archive exceeds the maximum allowed extracted size.',
    );
  });
});

function createProject(overrides: Partial<ProjectResponse>): ProjectResponse {
  return {
    id: 'project-1',
    name: 'Project 1',
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    usedResourceTypes: [],
    layoutPreset: 'AllInOne',
    repositories: [],
    agentPoolName: null,
    ...overrides,
  };
}

function createRepository(overrides: Partial<ProjectRepositoryResponse>): ProjectRepositoryResponse {
  return {
    id: 'repo-1',
    providerType: 'AzureDevOps',
    repositoryUrl: 'https://dev.azure.com/org/project/_git/repository',
    owner: 'org',
    repositoryName: 'repository',
    defaultBranch: 'main',
    isConfigured: true,
    contentKinds: ['Infrastructure'],
    ...overrides,
  };
}