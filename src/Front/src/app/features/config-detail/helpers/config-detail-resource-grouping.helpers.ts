import {
  CrossConfigReferenceResponse,
  IncomingCrossConfigReferenceResponse,
} from '../../../shared/interfaces/cross-config-reference.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';

export interface ResourceDisplayItem {
  resource: AzureResourceResponse;
  children?: AzureResourceResponse[];
  incomingChildren?: IncomingCrossConfigReferenceResponse[];
  isParent: boolean;
  crossConfigRef?: CrossConfigReferenceResponse;
}

interface ResourceGroupingContext {
  parentMap: Map<string, AzureResourceResponse>;
  childrenByParent: Map<string, AzureResourceResponse[]>;
}

export interface ResourceGroupingRequest {
  resources: ReadonlyArray<AzureResourceResponse>;
  crossConfigReferences: ReadonlyArray<CrossConfigReferenceResponse>;
  incomingCrossConfigReferences: ReadonlyArray<IncomingCrossConfigReferenceResponse>;
  parentChildResourceTypes: Readonly<Record<string, ReadonlyArray<string> | undefined>>;
  childResourceTypes: ReadonlySet<string>;
}

export function buildGroupedResourcesForResourceGroup(request: ResourceGroupingRequest): ResourceDisplayItem[] {
  const { resources } = request;
  if (resources.length === 0) {
    return [];
  }

  const grouping = createResourceGrouping(resources, request.parentChildResourceTypes);
  const crossConfigParentRefs = createCrossConfigParentRefs(
    request.crossConfigReferences,
    request.parentChildResourceTypes,
    grouping.parentMap,
    grouping.childrenByParent,
  );
  const standaloneResources = collectStandaloneResources(
    resources,
    request.childResourceTypes,
    grouping.parentMap,
    crossConfigParentRefs,
    grouping.childrenByParent,
  );

  return buildResourceDisplayItems(
    grouping.parentMap,
    grouping.childrenByParent,
    crossConfigParentRefs,
    createIncomingChildrenMap(request.incomingCrossConfigReferences),
    standaloneResources,
  );
}

export function getUnparentedCrossConfigReferences(
  groupedResources: ReadonlyArray<ResourceDisplayItem>,
  crossConfigReferences: ReadonlyArray<CrossConfigReferenceResponse>,
): CrossConfigReferenceResponse[] {
  const parentedReferenceIds = new Set(
    groupedResources
      .filter((item) => item.crossConfigRef)
      .map((item) => item.crossConfigRef?.referenceId),
  );

  return crossConfigReferences.filter((reference) => !parentedReferenceIds.has(reference.referenceId));
}

function createResourceGrouping(
  resources: ReadonlyArray<AzureResourceResponse>,
  parentChildResourceTypes: Readonly<Record<string, ReadonlyArray<string> | undefined>>,
): ResourceGroupingContext {
  const parentMap = new Map<string, AzureResourceResponse>();
  const childrenByParent = new Map<string, AzureResourceResponse[]>();

  for (const resource of resources) {
    if (!parentChildResourceTypes[resource.resourceType]) {
      continue;
    }

    parentMap.set(resource.id, resource);
    childrenByParent.set(resource.id, []);
  }

  return { parentMap, childrenByParent };
}

function createCrossConfigParentRefs(
  crossConfigReferences: ReadonlyArray<CrossConfigReferenceResponse>,
  parentChildResourceTypes: Readonly<Record<string, ReadonlyArray<string> | undefined>>,
  parentMap: Map<string, AzureResourceResponse>,
  childrenByParent: Map<string, AzureResourceResponse[]>,
): Map<string, CrossConfigReferenceResponse> {
  const crossConfigParentRefs = new Map<string, CrossConfigReferenceResponse>();

  for (const reference of crossConfigReferences) {
    if (!parentChildResourceTypes[reference.targetResourceType] || parentMap.has(reference.targetResourceId)) {
      continue;
    }

    crossConfigParentRefs.set(reference.targetResourceId, reference);
    if (!childrenByParent.has(reference.targetResourceId)) {
      childrenByParent.set(reference.targetResourceId, []);
    }
  }

  return crossConfigParentRefs;
}

function collectStandaloneResources(
  resources: ReadonlyArray<AzureResourceResponse>,
  childResourceTypes: ReadonlySet<string>,
  parentMap: Map<string, AzureResourceResponse>,
  crossConfigParentRefs: Map<string, CrossConfigReferenceResponse>,
  childrenByParent: Map<string, AzureResourceResponse[]>,
): AzureResourceResponse[] {
  const standaloneResources: AzureResourceResponse[] = [];

  for (const resource of resources) {
    if (parentMap.has(resource.id)) {
      continue;
    }

    if (!tryAttachChildResource(resource, childResourceTypes, parentMap, crossConfigParentRefs, childrenByParent)) {
      standaloneResources.push(resource);
    }
  }

  return standaloneResources;
}

function tryAttachChildResource(
  resource: AzureResourceResponse,
  childResourceTypes: ReadonlySet<string>,
  parentMap: Map<string, AzureResourceResponse>,
  crossConfigParentRefs: Map<string, CrossConfigReferenceResponse>,
  childrenByParent: Map<string, AzureResourceResponse[]>,
): boolean {
  if (!childResourceTypes.has(resource.resourceType) || !resource.parentResourceId) {
    return false;
  }

  const parentId = resource.parentResourceId;
  if (!parentMap.has(parentId) && !crossConfigParentRefs.has(parentId)) {
    return false;
  }

  childrenByParent.get(parentId)?.push(resource);
  return true;
}

function createIncomingChildrenMap(
  incomingCrossConfigReferences: ReadonlyArray<IncomingCrossConfigReferenceResponse>,
): Map<string, IncomingCrossConfigReferenceResponse[]> {
  const incomingByTarget = new Map<string, IncomingCrossConfigReferenceResponse[]>();

  for (const incomingReference of incomingCrossConfigReferences) {
    const existing = incomingByTarget.get(incomingReference.targetResourceId);
    if (existing) {
      existing.push(incomingReference);
      continue;
    }

    incomingByTarget.set(incomingReference.targetResourceId, [incomingReference]);
  }

  return incomingByTarget;
}

function buildResourceDisplayItems(
  parentMap: Map<string, AzureResourceResponse>,
  childrenByParent: Map<string, AzureResourceResponse[]>,
  crossConfigParentRefs: Map<string, CrossConfigReferenceResponse>,
  incomingByTarget: Map<string, IncomingCrossConfigReferenceResponse[]>,
  standaloneResources: ReadonlyArray<AzureResourceResponse>,
): ResourceDisplayItem[] {
  const result: ResourceDisplayItem[] = [];

  for (const [parentId, parent] of parentMap) {
    result.push({
      resource: parent,
      children: childrenByParent.get(parentId) ?? [],
      incomingChildren: incomingByTarget.get(parentId),
      isParent: true,
    });
  }

  for (const [referenceResourceId, reference] of crossConfigParentRefs) {
    const children = childrenByParent.get(referenceResourceId) ?? [];
    if (children.length === 0) {
      continue;
    }

    result.push({
      resource: {
        id: reference.targetResourceId,
        name: reference.targetResourceName,
        resourceType: reference.targetResourceType,
        location: '',
      },
      children,
      isParent: true,
      crossConfigRef: reference,
    });
  }

  for (const resource of standaloneResources) {
    result.push({ resource, isParent: false });
  }

  return result;
}