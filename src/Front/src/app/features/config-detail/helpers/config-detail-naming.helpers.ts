import { EnvironmentDefinitionResponse, InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { RESOURCE_TYPE_ABBREVIATIONS } from '../../../shared/resource-metadata/resource-type.metadata';

export interface ConfigDetailNamingPreviewRequest {
  resourceName: string;
  resourceType: string;
  environment: EnvironmentDefinitionResponse | null;
  config: InfrastructureConfigResponse | null;
  project: ProjectResponse | null;
}

export function getMissingEnvironmentNames(
  resource: AzureResourceResponse,
  environments: ReadonlyArray<EnvironmentDefinitionResponse>,
  excludedResourceTypes: ReadonlySet<string>,
): string[] {
  if (resource.isExisting) {
    return [];
  }

  if (excludedResourceTypes.has(resource.resourceType)) {
    return [];
  }

  if (environments.length === 0) {
    return [];
  }

  const configuredEnvironments = new Set(resource.configuredEnvironments ?? []);
  return environments
    .map((environment) => environment.name)
    .filter((environmentName) => !configuredEnvironments.has(environmentName));
}

export function resolveNamingPreview(request: ConfigDetailNamingPreviewRequest): string | null {
  const { config, environment, project, resourceName, resourceType } = request;

  if (!environment || !config) {
    return null;
  }

  const useProjectNaming = config.useProjectNamingConventions && project;
  const namingTemplates = useProjectNaming ? project.resourceNamingTemplates : config.resourceNamingTemplates;
  const defaultTemplate = useProjectNaming ? project.defaultNamingTemplate : config.defaultNamingTemplate;
  const resourceOverride = namingTemplates.find((template) => template.resourceType === resourceType);
  const template = resourceOverride?.template ?? defaultTemplate;

  if (!template) {
    return null;
  }

  const configAbbreviationOverride = config.resourceAbbreviationOverrides.find(
    (override) => override.resourceType === resourceType,
  );
  const projectAbbreviationOverride = project?.resourceAbbreviations.find(
    (override) => override.resourceType === resourceType,
  );
  const effectiveAbbreviation = configAbbreviationOverride?.abbreviation
    ?? projectAbbreviationOverride?.abbreviation
    ?? RESOURCE_TYPE_ABBREVIATIONS[resourceType]
    ?? resourceType.toLowerCase();

  const replacements: Readonly<Record<string, string>> = {
    name: resourceName,
    prefix: environment.prefix ?? '',
    suffix: environment.suffix ?? '',
    env: environment.name,
    envShort: environment.shortName ?? '',
    resourceType,
    resourceAbbr: effectiveAbbreviation,
    location: environment.location,
  };

  return template.replaceAll(/\{(\w+)\}/g, (_match, key: string) => replacements[key] ?? `{${key}}`);
}