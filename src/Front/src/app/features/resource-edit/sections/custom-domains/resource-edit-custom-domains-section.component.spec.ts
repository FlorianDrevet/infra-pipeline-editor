import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { CustomDomainResponse } from '../../../../shared/interfaces/custom-domain.interface';
import { ResourceEditCustomDomainsSectionComponent } from './resource-edit-custom-domains-section.component';

describe('ResourceEditCustomDomainsSectionComponent', () => {
  let fixture: ComponentFixture<ResourceEditCustomDomainsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditCustomDomainsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditCustomDomainsSectionComponent);
  });

  it('shows the empty state when the selected environment has no custom domains', () => {
    const section = createSection({
      domainsForEnvironment: () => [],
    });

    fixture.componentRef.setInput('environmentName', 'dev');
    fixture.componentRef.setInput('section', section);
    fixture.detectChanges();

    expect(getElement('.custom-domains-section__empty')).not.toBeNull();
  });

  it('delegates add and remove actions to the section controller', () => {
    const openAddDialog = jasmine.createSpy('openAddDialog');
    const removeDomain = jasmine.createSpy('removeDomain');
    const domain = createDomain('domain-1', 'dev', 'api.contoso.com');
    const section = createSection({
      domainsForEnvironment: () => [domain],
      openAddDialog,
      removeDomain,
    });

    fixture.componentRef.setInput('environmentName', 'dev');
    fixture.componentRef.setInput('section', section);
    fixture.detectChanges();

    getButton('.custom-domains-section__intro app-ds-button button').click();
    getButton('app-ds-icon-button[icon="delete_outline"] button').click();

    expect(openAddDialog).toHaveBeenCalledOnceWith('dev');
    expect(removeDomain).toHaveBeenCalledOnceWith(domain);
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }

  function getElement(selector: string): HTMLElement | null {
    return fixture.nativeElement.querySelector(selector) as HTMLElement | null;
  }
});

function createSection(overrides: Partial<ResourceEditCustomDomainsSectionStub> = {}): ResourceEditCustomDomainsSectionStub {
  return {
    errorKey: signal(''),
    isLoading: signal(false),
    domainsForEnvironment: () => [],
    openAddDialog: () => undefined,
    removeDomain: () => undefined,
    validateDns: () => undefined,
    showDnsInstructions: () => undefined,
    ...overrides,
  };
}

function createDomain(id: string, environmentName: string, domainName: string): CustomDomainResponse {
  return {
    id,
    resourceId: 'resource-1',
    environmentName,
    domainName,
    bindingType: 'SniEnabled',
    dnsValidationStatus: 'Pending',
  };
}

interface ResourceEditCustomDomainsSectionStub {
  errorKey: ReturnType<typeof signal<string>>;
  isLoading: ReturnType<typeof signal<boolean>>;
  domainsForEnvironment(environmentName: string): CustomDomainResponse[];
  openAddDialog(environmentName: string): void;
  removeDomain(domain: CustomDomainResponse): void;
  validateDns(domain: CustomDomainResponse): void;
  showDnsInstructions(domain: CustomDomainResponse): void;
}