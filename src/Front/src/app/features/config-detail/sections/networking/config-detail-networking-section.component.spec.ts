import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsSelectComponent } from '../../../../shared/components/ds/ds-select/ds-select.component';
import { ConfigDetailNetworkingSectionComponent } from './config-detail-networking-section.component';
import {
  ConfigDetailNetworkingSectionViewModel,
  NetworkingResourceItem,
} from './config-detail-networking-section.view-model';

describe('ConfigDetailNetworkingSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailNetworkingSectionComponent>;
  let translateService: TranslateService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailNetworkingSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    translateService = TestBed.inject(TranslateService);
    translateService.setTranslation('fr', {
      COMMON: {
        SAVE: 'Enregistrer',
      },
      CONFIG_DETAIL_NETWORKING: {
        MODE: {
          LABEL: 'Mode reseau',
          SIMPLIFIED: 'Simplifie',
          STANDARD: 'Standard',
          ADVANCED: 'Avance',
        },
        VNET: {
          TITLE: 'Configuration VNet',
          SOURCE_LABEL: 'Source',
          CREATE_NEW: 'Creer un nouveau VNet',
          USE_EXISTING: 'Utiliser un VNet existant',
          USE_HUB_SPOKE: 'Hub & Spoke',
          ADDRESS_SPACE: 'Espace',
          ADDRESS_SPACE_PLACEHOLDER: 'address-space-cidr',
          SUBNET_PREFIX: 'Prefixe',
          SUBNET_PREFIX_PLACEHOLDER: 'subnet-prefix-cidr',
          EXISTING_VNET_ID: 'VNet existant',
          EXISTING_VNET_ID_PLACEHOLDER: '/subscriptions/...',
          PE_SUBNET_NAME: 'Subnet PE',
          PE_SUBNET_NAME_PLACEHOLDER: 'snet-pe',
        },
        DNS: {
          TITLE: 'DNS',
          MODE_LABEL: 'Mode DNS',
          AUTO_MANAGED: 'Automatique',
          CENTRALIZED_HUB: 'Hub centralise',
          CUSTOM: 'Personnalise',
          HUB_RG_ID: 'RG Hub',
          HUB_RG_ID_PLACEHOLDER: 'rg-dns-hub',
          HUB_SUBSCRIPTION_ID: 'Subscription Hub',
          HUB_SUBSCRIPTION_ID_PLACEHOLDER: '00000000-0000-0000-0000-000000000000',
        },
        PRIVATIZATION: {
          TITLE: 'Privatisation',
          DESCRIPTION: 'Description',
          EMPTY: 'Aucune ressource',
        },
      },
    });
    translateService.use('fr');

    fixture = TestBed.createComponent(ConfigDetailNetworkingSectionComponent);
  });

  it('renders translated save label and translated select options', () => {
    fixture.componentRef.setInput('viewModel', createViewModel());
    fixture.detectChanges();

    const selectComponent = fixture.debugElement.query(By.directive(DsSelectComponent)).componentInstance as DsSelectComponent;
    const buttonLabel = fixture.nativeElement.querySelector('.ds-btn__label') as HTMLSpanElement;

    expect(selectComponent.options().map((option) => option.label)).toEqual(['Simplifie', 'Standard', 'Avance']);
    expect(buttonLabel.textContent?.trim()).toBe('Enregistrer');
  });

  it('renders orchestration and distinct networking cards when networking details are available', () => {
    fixture.componentRef.setInput('viewModel', createViewModel({
      showVnetPanel: true,
      showDnsPanel: true,
      resources: [
        createResourceItem('storage-1', 'Storage Account', 'StorageAccount', 'storage', true),
        createResourceItem('container-app-1', 'Container App', 'ContainerApp', 'cloud', false),
      ],
    }));
    fixture.detectChanges();

    const orchestrationCard = fixture.nativeElement.querySelector('.networking-card--orchestration');
    const sectionCards = fixture.nativeElement.querySelectorAll('.networking-card--section');
    const stateChips = fixture.nativeElement.querySelectorAll('.networking-card__state');
    const privatizationList = fixture.nativeElement.querySelector('.privatization-list');
    const privatizationItems = fixture.nativeElement.querySelectorAll('.privatization-item');

    expect(orchestrationCard).not.toBeNull();
    expect(fixture.nativeElement.querySelectorAll('app-ds-card-mat').length).toBe(4);
    expect(sectionCards.length).toBe(3);
    expect(stateChips.length).toBeGreaterThanOrEqual(3);
    expect(privatizationList).not.toBeNull();
    expect(privatizationItems.length).toBe(2);
  });
});

interface NetworkingSectionViewModelOverrides {
  readonly mode?: string;
  readonly vnetSourceType?: string;
  readonly dnsMode?: string;
  readonly resources?: NetworkingResourceItem[];
  readonly showVnetPanel?: boolean;
  readonly showDnsPanel?: boolean;
}

function createViewModel(
  overrides: NetworkingSectionViewModelOverrides = {},
): ConfigDetailNetworkingSectionViewModel {
  return {
    isLoading: signal(false),
    isSaving: signal(false),
    errorKey: signal(''),
    mode: signal(overrides.mode ?? 'Simplified'),
    vnetSourceType: signal(overrides.vnetSourceType ?? 'CreateNew'),
    existingVnetResourceId: signal(''),
    createNewAddressSpace: signal(''),
    createNewSubnetAddressPrefix: signal(''),
    privateEndpointsSubnetName: signal(''),
    dnsMode: signal(overrides.dnsMode ?? 'AutoManaged'),
    dnsHubResourceGroupId: signal(''),
    dnsHubSubscriptionId: signal(''),
    resources: signal(overrides.resources ?? []),
    showVnetPanel: signal(overrides.showVnetPanel ?? false),
    showDnsPanel: signal(overrides.showDnsPanel ?? false),
    modeOptions: signal([
      { value: 'Simplified', label: 'Simplifie' },
      { value: 'Standard', label: 'Standard' },
      { value: 'Advanced', label: 'Avance' },
    ]),
    vnetSourceTypeOptions: signal([
      { value: 'CreateNew', label: 'Creer un nouveau VNet' },
      { value: 'UseExisting', label: 'Utiliser un VNet existant' },
      { value: 'UseHubSpoke', label: 'Hub & Spoke' },
    ]),
    dnsModeOptions: signal([
      { value: 'AutoManaged', label: 'Automatique' },
      { value: 'CentralizedHub', label: 'Hub centralise' },
      { value: 'Custom', label: 'Personnalise' },
    ]),
    onModeChange: () => undefined,
    onVnetSourceTypeChange: () => undefined,
    onExistingVnetResourceIdChange: () => undefined,
    onCreateNewAddressSpaceChange: () => undefined,
    onCreateNewSubnetAddressPrefixChange: () => undefined,
    onPrivateEndpointsSubnetNameChange: () => undefined,
    onDnsModeChange: () => undefined,
    onDnsHubResourceGroupIdChange: () => undefined,
    onDnsHubSubscriptionIdChange: () => undefined,
    onTogglePrivatization: () => undefined,
    save: async () => undefined,
  };
}

function createResourceItem(
  id: string,
  name: string,
  resourceType: string,
  icon: string,
  isPrivatized: boolean,
): NetworkingResourceItem {
  return {
    id,
    name,
    resourceType,
    icon,
    isPrivatized,
    saving: false,
  };
}