/**
 * In-memory draft model for the multi-step "Create project" wizard.
 * Persisted to sessionStorage between steps and across reloads.
 */
export type LayoutPreset = 'AllInOne' | 'SplitInfraCode' | 'MultiRepo';

export interface EnvironmentDraft {
  name: string;
  shortName: string;
  prefix: string;
  suffix: string;
  location: string;
  subscriptionId: string;
  order: number;
  requiresApproval: boolean;
}

export interface RepositoryDraft {
  alias: string;
  contentKinds: string[];
  providerType: '' | 'GitHub' | 'AzureDevOps';
  repositoryUrl: string;
  defaultBranch: string;
}

export interface CreateProjectWizardDraft {
  name: string;
  description: string;
  layoutPreset: LayoutPreset | '';
  environments: EnvironmentDraft[];
  repositories: RepositoryDraft[];
  updatedAt: number;
}

export const EMPTY_DRAFT: CreateProjectWizardDraft = {
  name: '',
  description: '',
  layoutPreset: '',
  environments: [],
  repositories: [],
  updatedAt: 0,
};

export function createEmptyEnvironment(order: number): EnvironmentDraft {
  return {
    name: '',
    shortName: '',
    prefix: '',
    suffix: '',
    location: 'WestEurope',
    subscriptionId: '',
    order,
    requiresApproval: false,
  };
}

export function createEmptyRepository(contentKinds: string[]): RepositoryDraft {
  return {
    alias: '',
    contentKinds,
    providerType: '',
    repositoryUrl: '',
    defaultBranch: '',
  };
}

export function isDraftMeaningful(draft: CreateProjectWizardDraft): boolean {
  if (draft.name.trim() || draft.description.trim() || draft.layoutPreset) {
    return true;
  }
  if (draft.environments.some((env) => env.name.trim() || env.shortName.trim())) {
    return true;
  }
  if (draft.repositories.some((repo) => repo.alias.trim() || repo.repositoryUrl.trim())) {
    return true;
  }
  return false;
}
