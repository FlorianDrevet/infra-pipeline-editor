import { inject, Injectable } from '@angular/core';

import { PendingCustomDomainIssue } from '../interfaces/pending-custom-domain-issue.interface';
import { AzureResourceResponse } from '../interfaces/resource-group.interface';
import { CustomDomainService } from './custom-domain.service';

const SUPPORTED_CUSTOM_DOMAIN_RESOURCE_TYPES = new Set<string>(['ContainerApp', 'WebApp', 'FunctionApp']);
const VALIDATED_DNS_STATUS = 'Validated';

@Injectable({
  providedIn: 'root',
})
export class CustomDomainDiagnosticsService {
  private readonly customDomainService = inject(CustomDomainService);

  async collectPendingIssues(resources: AzureResourceResponse[]): Promise<PendingCustomDomainIssue[]> {
    const customDomainResources = resources.filter((resource) =>
      SUPPORTED_CUSTOM_DOMAIN_RESOURCE_TYPES.has(resource.resourceType),
    );

    const pendingIssuesByResource = await Promise.all(
      customDomainResources.map((resource) => this.loadPendingIssuesForResource(resource)),
    );

    return pendingIssuesByResource.flat();
  }

  private async loadPendingIssuesForResource(
    resource: AzureResourceResponse,
  ): Promise<PendingCustomDomainIssue[]> {
    try {
      const domains = await this.customDomainService.getByResourceId(resource.id);
      return domains
        .filter((domain) => domain.dnsValidationStatus !== VALIDATED_DNS_STATUS)
        .map((domain) => ({
          resourceId: resource.id,
          resourceName: resource.name,
          resourceType: resource.resourceType,
          domainName: domain.domainName,
          environmentName: domain.environmentName,
        }));
    } catch {
      return [];
    }
  }
}