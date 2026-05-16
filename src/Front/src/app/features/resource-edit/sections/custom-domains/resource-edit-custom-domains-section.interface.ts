import { Signal } from '@angular/core';

import { CustomDomainResponse } from '../../../../shared/interfaces/custom-domain.interface';

export interface ResourceEditCustomDomainsSection {
  readonly errorKey: Signal<string>;
  readonly isLoading: Signal<boolean>;

  load(): Promise<void>;
  domainsForEnvironment(environmentName: string): CustomDomainResponse[];
  openAddDialog(environmentName: string): void;
  removeDomain(domain: CustomDomainResponse): void;
  validateDns(domain: CustomDomainResponse): void;
  showDnsInstructions(domain: CustomDomainResponse): void;
}