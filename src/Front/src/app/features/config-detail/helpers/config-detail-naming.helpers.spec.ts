import { EnvironmentDefinitionResponse, InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS } from '../../../shared/resource-metadata/resource-type.metadata';
import { getMissingEnvironmentNames, resolveNamingPreview } from './config-detail-naming.helpers';

describe('config detail naming helpers', () => {
  it('prefers config-level resource templates and abbreviation overrides when building a preview', () => {
    const preview = resolveNamingPreview({
      resourceName: 'orders',
      resourceType: 'StorageAccount',
      environment: createEnvironment(),
      config: createConfig({
        defaultNamingTemplate: '{prefix}-{resourceAbbr}-{name}-{suffix}',
        resourceNamingTemplates: [
          {
            id: 'tmpl-storage',
            resourceType: 'StorageAccount',
            template: '{prefix}-{resourceAbbr}-{envShort}-{name}',
          },
        ],
        resourceAbbreviationOverrides: [
          {
            id: 'abbr-storage',
            resourceType: 'StorageAccount',
            abbreviation: 'store',
          },
        ],
      }),
      project: createProject({
        resourceAbbreviations: [
          {
            id: 'project-storage',
            resourceType: 'StorageAccount',
            abbreviation: 'ignored',
          },
        ],
      }),
    });

    expect(preview).toBe('dev-store-dev-orders');
  });

  it('falls back to inherited project naming when the config uses project conventions', () => {
    const preview = resolveNamingPreview({
      resourceName: 'api',
      resourceType: 'WebApp',
      environment: createEnvironment({ suffix: 'web' }),
      config: createConfig({
        useProjectNamingConventions: true,
        defaultNamingTemplate: null,
        resourceNamingTemplates: [],
      }),
      project: createProject({
        defaultNamingTemplate: '{prefix}-{resourceAbbr}-{name}-{suffix}',
        resourceAbbreviations: [
          {
            id: 'project-web-app',
            resourceType: 'WebApp',
            abbreviation: 'site',
          },
        ],
      }),
    });

    expect(preview).toBe('dev-site-api-web');
  });

  it('computes missing environments while excluding existing resources and non-environmental types', () => {
    const environments = [
      createEnvironment({ id: 'env-dev', name: 'Development', order: 1 }),
      createEnvironment({ id: 'env-prod', name: 'Production', shortName: 'prod', prefix: 'prod', order: 2 }),
    ];

    const missingForWebApp = getMissingEnvironmentNames(
      createResource({ resourceType: 'WebApp', configuredEnvironments: ['Development'] }),
      environments,
      RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS,
    );

    const missingForExistingResource = getMissingEnvironmentNames(
      createResource({ resourceType: 'WebApp', isExisting: true, configuredEnvironments: ['Development'] }),
      environments,
      RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS,
    );

    const missingForIdentity = getMissingEnvironmentNames(
      createResource({ resourceType: 'UserAssignedIdentity' }),
      environments,
      RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS,
    );

    const missingForFrontDoor = getMissingEnvironmentNames(
      createResource({ resourceType: 'FrontDoor' }),
      environments,
      RESOURCE_TYPES_WITHOUT_ENVIRONMENT_SETTINGS,
    );

    expect(missingForWebApp).toEqual(['Production']);
    expect(missingForExistingResource).toEqual([]);
    expect(missingForIdentity).toEqual([]);
    expect(missingForFrontDoor).toEqual([]);
  });
});

function createEnvironment(overrides: Partial<EnvironmentDefinitionResponse> = {}): EnvironmentDefinitionResponse {
  return {
    id: 'env-dev',
    name: 'Development',
    shortName: 'dev',
    prefix: 'dev',
    suffix: 'svc',
    location: 'westeurope',
    subscriptionId: 'sub-001',
    order: 1,
    requiresApproval: false,
    azureResourceManagerConnection: null,
    tags: [],
    ...overrides,
  };
}

function createConfig(overrides: Partial<InfrastructureConfigResponse> = {}): InfrastructureConfigResponse {
  return {
    id: 'config-1',
    name: 'Payments',
    defaultNamingTemplate: '{prefix}-{resourceAbbr}-{name}',
    projectId: 'project-1',
    useProjectNamingConventions: false,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 0,
    resourceCount: 0,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Standard',
    tags: [],
    layoutMode: 'SplitInfraCode',
    repositories: [],
    ...overrides,
  };
}

function createProject(overrides: Partial<ProjectResponse> = {}): ProjectResponse {
  return {
    id: 'project-1',
    name: 'Platform',
    description: 'Shared platform project',
    members: [],
    environmentDefinitions: [createEnvironment()],
    defaultNamingTemplate: '{prefix}-{resourceAbbr}-{name}',
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    usedResourceTypes: ['StorageAccount', 'WebApp'],
    repositories: [],
    layoutPreset: 'MultiRepo',
    ...overrides,
  };
}

function createResource(overrides: Partial<AzureResourceResponse> = {}): AzureResourceResponse {
  return {
    id: 'resource-1',
    resourceType: 'WebApp',
    name: 'orders-api',
    location: 'westeurope',
    parentResourceId: undefined,
    configuredEnvironments: [],
    isExisting: false,
    storageSubResources: undefined,
    ...overrides,
  };
}