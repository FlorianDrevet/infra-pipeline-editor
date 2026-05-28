import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { VnetHelpDialogComponent, VnetHelpDialogData } from './vnet-help-dialog.component';

describe('VnetHelpDialogComponent', () => {
  let fixture: ComponentFixture<VnetHelpDialogComponent>;
  let translateService: TranslateService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VnetHelpDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: { context: 'resourceCreate' } satisfies VnetHelpDialogData,
        },
      ],
    }).compileComponents();

    translateService = TestBed.inject(TranslateService);
    translateService.setTranslation('en', {
      COMMON: {
        CLOSE: 'Close',
        VNET_HELP_DIALOG: {
          RESOURCE_CREATE: {
            TITLE: 'Virtual network values',
            SUBTITLE: 'Configure the addressing values Azure needs before this virtual network can be created.',
          },
          NETWORKING_PROFILE: {
            TITLE: 'Networking profile values',
            SUBTITLE: 'Choose the values used when InfraFlowSculptor creates the private-endpoint network footprint.',
          },
          ADDRESS_SPACES: {
            TITLE: 'Address spaces',
            BODY: 'An address space defines the CIDR ranges available inside the virtual network. Azure uses these ranges to allocate subnets and private IP addresses.',
            GUIDANCE: 'Use ranges approved by your platform team or taken from your landing zone networking plan.',
          },
          DNS_SERVERS: {
            TITLE: 'DNS servers',
            BODY: 'DNS servers define how resources in the VNet resolve host names. Leave this empty when Azure-provided DNS is enough.',
            GUIDANCE: 'Use your hub DNS forwarders, on-prem resolvers, or documented shared-services IPs when custom name resolution is required.',
          },
          DDOS: {
            TITLE: 'DDoS protection',
            BODY: 'Enabling DDoS changes the protection plan applied to this virtual network and can affect cost and operational ownership.',
          },
          SUBNET_PREFIX: {
            TITLE: 'Private-endpoint subnet prefix',
            BODY: 'The subnet prefix reserves a smaller CIDR block inside the selected VNet address space for private endpoints.',
            GUIDANCE: 'Choose a non-overlapping subnet that fits inside the VNet range and matches your network segmentation rules.',
          },
          WHY: {
            TITLE: 'Why the app asks for this',
            BODY: 'InfraFlowSculptor needs these inputs to generate valid Azure networking artifacts instead of guessing values that may clash with your platform design.',
          },
          IMPACT: {
            TITLE: 'What this impacts',
            BODY: 'These values affect IP allocation, private connectivity, DNS resolution, and downstream resources that attach to the virtual network.',
          },
        },
      },
    }, true);
    translateService.use('en');
  });

  it('renders the VNet concepts and implications for the resource creation context', () => {
    fixture = TestBed.createComponent(VnetHelpDialogComponent);
    fixture.detectChanges();

    const textContent = normalizeWhitespace(fixture.nativeElement.textContent ?? '');

    expect(textContent).toContain('Virtual network values');
    expect(textContent).toContain('Address spaces');
    expect(textContent).toContain('DNS servers');
    expect(textContent).toContain('DDoS protection');
    expect(textContent).toContain('Why the app asks for this');
    expect(textContent).toContain('What this impacts');
  });

  it('renders subnet-prefix guidance for the networking profile context', () => {
    TestBed.overrideProvider(MAT_DIALOG_DATA, {
      useValue: { context: 'networkingProfile' } satisfies VnetHelpDialogData,
    });

    fixture = TestBed.createComponent(VnetHelpDialogComponent);
    fixture.detectChanges();

    const textContent = normalizeWhitespace(fixture.nativeElement.textContent ?? '');

    expect(textContent).toContain('Networking profile values');
    expect(textContent).toContain('Private-endpoint subnet prefix');
    expect(textContent).not.toContain('DDoS protection');
  });
});

function normalizeWhitespace(value: string): string {
  return value.replace(/\s+/g, ' ').trim();
}