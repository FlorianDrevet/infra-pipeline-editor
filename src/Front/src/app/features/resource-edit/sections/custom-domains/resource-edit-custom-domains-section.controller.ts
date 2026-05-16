import { inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';

import { ConfirmDialogComponent, ConfirmDialogData } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddCustomDomainRequest, CustomDomainResponse } from '../../../../shared/interfaces/custom-domain.interface';
import { EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { CustomDomainService } from '../../../../shared/services/custom-domain.service';
import {
  AddCustomDomainDialogComponent,
  AddCustomDomainDialogData,
} from '../../add-custom-domain-dialog/add-custom-domain-dialog.component';
import { DnsInstructionsDialogComponent, DnsInstructionsDialogData } from '../../dns-instructions-dialog/dns-instructions-dialog.component';
import { ResourceEditCustomDomainsSection } from './resource-edit-custom-domains-section.interface';

interface ResourceEditCustomDomainsSectionControllerDependencies {
  getResourceId(): string;
  getEnvironments(): EnvironmentDefinitionResponse[];
}

export function createResourceEditCustomDomainsSectionController(
  dependencies: ResourceEditCustomDomainsSectionControllerDependencies,
): ResourceEditCustomDomainsSection {
  const dialog = inject(MatDialog);
  const customDomainService = inject(CustomDomainService);

  const customDomains = signal<CustomDomainResponse[]>([]);
  const isLoading = signal(false);
  const errorKey = signal('');

  const load = async (): Promise<void> => {
    const resourceId = dependencies.getResourceId();
    if (!resourceId) {
      customDomains.set([]);
      return;
    }

    isLoading.set(true);
    errorKey.set('');
    try {
      const domains = await customDomainService.getByResourceId(resourceId);
      customDomains.set(domains);
    } catch {
      errorKey.set('RESOURCE_EDIT.CUSTOM_DOMAINS.LOAD_ERROR');
    } finally {
      isLoading.set(false);
    }
  };

  const domainsForEnvironment = (environmentName: string): CustomDomainResponse[] =>
    customDomains().filter((domain) => domain.environmentName === environmentName);

  const addCustomDomain = async (request: AddCustomDomainRequest): Promise<void> => {
    isLoading.set(true);
    errorKey.set('');
    try {
      await customDomainService.add(dependencies.getResourceId(), request);
      await load();
    } catch {
      errorKey.set('RESOURCE_EDIT.CUSTOM_DOMAINS.ADD_ERROR');
    } finally {
      isLoading.set(false);
    }
  };

  const openAddDialog = (environmentName: string): void => {
    const dialogRef = dialog.open(AddCustomDomainDialogComponent, {
      width: '520px',
      data: {
        environments: dependencies.getEnvironments(),
        existingDomains: customDomains(),
        preselectedEnvironment: environmentName,
      } satisfies AddCustomDomainDialogData,
    });

    dialogRef.afterClosed().subscribe((result?: AddCustomDomainRequest) => {
      if (!result) {
        return;
      }

      addCustomDomain(result).catch(() => undefined);
    });
  };

  const removeCustomDomain = async (domain: CustomDomainResponse): Promise<void> => {
    isLoading.set(true);
    errorKey.set('');
    try {
      await customDomainService.remove(dependencies.getResourceId(), domain.id);
      customDomains.update((currentDomains) => currentDomains.filter((currentDomain) => currentDomain.id !== domain.id));
    } catch {
      errorKey.set('RESOURCE_EDIT.CUSTOM_DOMAINS.REMOVE_ERROR');
    } finally {
      isLoading.set(false);
    }
  };

  const removeDomain = (domain: CustomDomainResponse): void => {
    const dialogRef = dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'RESOURCE_EDIT.CUSTOM_DOMAINS.REMOVE_CONFIRM_TITLE',
        messageKey: 'RESOURCE_EDIT.CUSTOM_DOMAINS.REMOVE_CONFIRM_MESSAGE',
        messageParams: { domain: domain.domainName },
        confirmKey: 'COMMON.DELETE',
        cancelKey: 'COMMON.CANCEL',
      } satisfies ConfirmDialogData,
    });

    dialogRef.afterClosed().subscribe((confirmed?: boolean) => {
      if (!confirmed) {
        return;
      }

      removeCustomDomain(domain).catch(() => undefined);
    });
  };

  const validateDns = (domain: CustomDomainResponse): void => {
    isLoading.set(true);
    errorKey.set('');
    customDomainService
      .validateDns(dependencies.getResourceId(), domain.id)
      .then((updated) => {
        customDomains.update((current) =>
          current.map((d) => (d.id === updated.id ? updated : d)),
        );
      })
      .catch(() => {
        errorKey.set('RESOURCE_EDIT.CUSTOM_DOMAINS.VALIDATE_ERROR');
      })
      .finally(() => {
        isLoading.set(false);
      });
  };

  const showDnsInstructions = (domain: CustomDomainResponse): void => {
    dialog.open(DnsInstructionsDialogComponent, {
      width: '560px',
      data: {
        resourceId: dependencies.getResourceId(),
        domain,
      } satisfies DnsInstructionsDialogData,
    });
  };

  return {
    errorKey,
    isLoading,
    load,
    domainsForEnvironment,
    openAddDialog,
    removeDomain,
    validateDns,
    showDnsInstructions,
  };
}