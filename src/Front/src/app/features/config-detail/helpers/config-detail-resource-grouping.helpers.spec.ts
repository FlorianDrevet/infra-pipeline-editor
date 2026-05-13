import {
  buildGroupedResourcesForResourceGroup,
  getUnparentedCrossConfigReferences,
} from './config-detail-resource-grouping.helpers';
import { CrossConfigReferenceResponse, IncomingCrossConfigReferenceResponse } from '../../../shared/interfaces/cross-config-reference.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';

describe('config detail resource grouping helpers', () => {
  it('groups local parents with children, cross-config parents, and standalone resources', () => {
    const groupedResources = buildGroupedResourcesForResourceGroup({
      resources: [
        createResource({ id: 'storage-1', name: 'storage', resourceType: 'StorageAccount' }),
        createResource({ id: 'blob-1', name: 'assets', resourceType: 'BlobContainer', parentResourceId: 'storage-1' }),
        createResource({ id: 'queue-1', name: 'jobs', resourceType: 'StorageQueue' }),
        createResource({ id: 'table-1', name: 'shared', resourceType: 'StorageTable', parentResourceId: 'external-storage' }),
      ],
      crossConfigReferences: [
        createCrossConfigReference({
          referenceId: 'xref-1',
          targetConfigId: 'config-remote',
          targetConfigName: 'Remote',
          targetResourceId: 'external-storage',
          targetResourceName: 'shared-storage',
          targetResourceType: 'StorageAccount',
          targetResourceGroupName: 'rg-remote',
        }),
      ],
      incomingCrossConfigReferences: [
        createIncomingReference({ targetResourceId: 'storage-1', sourceResourceId: 'consumer-1', sourceResourceName: 'consumer' }),
      ],
      parentChildResourceTypes: {
        StorageAccount: ['BlobContainer', 'StorageQueue', 'StorageTable'],
      },
      childResourceTypes: new Set(['BlobContainer', 'StorageQueue', 'StorageTable']),
    });

    const localParent = groupedResources.find((item) => item.resource.id === 'storage-1');
    expect(localParent?.isParent).toBeTrue();
    expect(localParent?.children?.map((child) => child.id)).toEqual(['blob-1']);
    expect(localParent?.incomingChildren?.map((reference) => reference.sourceResourceId)).toEqual(['consumer-1']);

    const crossConfigParent = groupedResources.find((item) => item.crossConfigRef?.referenceId === 'xref-1');
    expect(crossConfigParent?.resource.name).toBe('shared-storage');
    expect(crossConfigParent?.children?.map((child) => child.id)).toEqual(['table-1']);

    const standaloneResource = groupedResources.find((item) => item.resource.id === 'queue-1');
    expect(standaloneResource).toEqual(jasmine.objectContaining({ isParent: false }));
  });

  it('filters out cross-config references that are already represented as parent groupings', () => {
    const groupedResources = buildGroupedResourcesForResourceGroup({
      resources: [
        createResource({ id: 'blob-1', name: 'assets', resourceType: 'BlobContainer', parentResourceId: 'external-storage' }),
      ],
      crossConfigReferences: [
        createCrossConfigReference({
          referenceId: 'xref-parented',
          targetConfigId: 'config-remote',
          targetConfigName: 'Remote',
          targetResourceId: 'external-storage',
          targetResourceName: 'shared-storage',
          targetResourceType: 'StorageAccount',
          targetResourceGroupName: 'rg-remote',
        }),
        createCrossConfigReference({
          referenceId: 'xref-standalone',
          targetConfigId: 'config-other',
          targetConfigName: 'Other',
          targetResourceId: 'sql-1',
          targetResourceName: 'shared-sql',
          targetResourceType: 'SqlServer',
          targetResourceGroupName: 'rg-sql',
        }),
      ],
      incomingCrossConfigReferences: [],
      parentChildResourceTypes: {
        StorageAccount: ['BlobContainer', 'StorageQueue', 'StorageTable'],
      },
      childResourceTypes: new Set(['BlobContainer', 'StorageQueue', 'StorageTable']),
    });

    const remainingReferences = getUnparentedCrossConfigReferences(groupedResources, [
      createCrossConfigReference({
        referenceId: 'xref-parented',
        targetConfigId: 'config-remote',
        targetConfigName: 'Remote',
        targetResourceId: 'external-storage',
        targetResourceName: 'shared-storage',
        targetResourceType: 'StorageAccount',
        targetResourceGroupName: 'rg-remote',
      }),
      createCrossConfigReference({
        referenceId: 'xref-standalone',
        targetConfigId: 'config-other',
        targetConfigName: 'Other',
        targetResourceId: 'sql-1',
        targetResourceName: 'shared-sql',
        targetResourceType: 'SqlServer',
        targetResourceGroupName: 'rg-sql',
      }),
    ]);

    expect(remainingReferences.map((reference) => reference.referenceId)).toEqual(['xref-standalone']);
  });
});

function createResource(overrides: Partial<AzureResourceResponse>): AzureResourceResponse {
  return {
    id: 'resource-1',
    name: 'resource',
    resourceType: 'StorageAccount',
    location: 'westeurope',
    configuredEnvironments: [],
    isExisting: false,
    storageSubResources: undefined,
    ...overrides,
  };
}

function createCrossConfigReference(overrides: Partial<CrossConfigReferenceResponse>): CrossConfigReferenceResponse {
  return {
    referenceId: 'xref-1',
    targetResourceId: 'target-1',
    targetResourceName: 'target',
    targetResourceType: 'StorageAccount',
    targetConfigId: 'config-remote',
    targetConfigName: 'Remote',
    targetResourceGroupName: 'rg-remote',
    ...overrides,
  };
}

function createIncomingReference(overrides: Partial<IncomingCrossConfigReferenceResponse>): IncomingCrossConfigReferenceResponse {
  return {
    referenceId: 'incoming-1',
    targetResourceId: 'target-1',
    targetResourceName: 'target',
    targetResourceType: 'StorageAccount',
    sourceResourceId: 'source-1',
    sourceResourceName: 'source',
    sourceResourceType: 'WebApp',
    sourceResourceGroupName: 'rg-source',
    sourceConfigId: 'config-source',
    sourceConfigName: 'Source',
    ...overrides,
  };
}