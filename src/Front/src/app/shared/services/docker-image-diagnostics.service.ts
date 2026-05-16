import { Injectable } from '@angular/core';

import { AzureResourceResponse } from '../interfaces/resource-group.interface';
import { PendingDockerImageIssue } from '../components/generation-diagnostics-dialog/generation-diagnostics-dialog.component';

const CONTAINER_RESOURCE_TYPES = new Set<string>(['ContainerApp']);
const CONDITIONAL_CONTAINER_TYPES = new Set<string>(['WebApp', 'FunctionApp']);

@Injectable({
  providedIn: 'root',
})
export class DockerImageDiagnosticsService {
  collectPendingIssues(resources: AzureResourceResponse[]): PendingDockerImageIssue[] {
    const issues: PendingDockerImageIssue[] = [];

    for (const resource of resources) {
      if (resource.isExisting) continue;
      if (!this.isContainerResource(resource)) continue;

      const dockerImageName = resource.properties?.['dockerImageName'];
      const dockerImageValidated = resource.properties?.['dockerImageValidated'];

      if (!dockerImageName?.trim()) continue;
      if (dockerImageValidated === 'true') continue;

      issues.push({
        resourceId: resource.id,
        resourceName: resource.name,
        resourceType: resource.resourceType,
        dockerImageName,
      });
    }

    return issues;
  }

  private isContainerResource(resource: AzureResourceResponse): boolean {
    if (CONTAINER_RESOURCE_TYPES.has(resource.resourceType)) return true;
    if (!CONDITIONAL_CONTAINER_TYPES.has(resource.resourceType)) return false;
    return resource.properties?.['deploymentMode'] === 'Container';
  }
}
