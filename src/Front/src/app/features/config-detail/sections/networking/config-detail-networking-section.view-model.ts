import { Signal } from '@angular/core';

import { DsSelectOption } from '../../../../shared/components/ds';
import { AzureResourceResponse } from '../../../../shared/interfaces/resource-group.interface';

export interface NetworkingResourceItem {
  id: string;
  name: string;
  resourceType: string;
  icon: string;
  isPrivatized: boolean;
  saving: boolean;
}

export interface ConfigDetailNetworkingSectionViewModel {
  readonly isLoading: Signal<boolean>;
  readonly isSaving: Signal<boolean>;
  readonly errorKey: Signal<string>;
  readonly mode: Signal<string>;
  readonly vnetSourceType: Signal<string>;
  readonly existingVnetResourceId: Signal<string>;
  readonly createNewAddressSpace: Signal<string>;
  readonly createNewSubnetAddressPrefix: Signal<string>;
  readonly privateEndpointsSubnetName: Signal<string>;
  readonly dnsMode: Signal<string>;
  readonly dnsHubResourceGroupId: Signal<string>;
  readonly dnsHubSubscriptionId: Signal<string>;
  readonly resources: Signal<NetworkingResourceItem[]>;
  readonly showVnetPanel: Signal<boolean>;
  readonly showDnsPanel: Signal<boolean>;
  readonly modeOptions: DsSelectOption[];
  readonly vnetSourceTypeOptions: DsSelectOption[];
  readonly dnsModeOptions: DsSelectOption[];

  onModeChange(value: string | number | null): void;
  onVnetSourceTypeChange(value: string | number | null): void;
  onExistingVnetResourceIdChange(value: string): void;
  onCreateNewAddressSpaceChange(value: string): void;
  onCreateNewSubnetAddressPrefixChange(value: string): void;
  onPrivateEndpointsSubnetNameChange(value: string): void;
  onDnsModeChange(value: string | number | null): void;
  onDnsHubResourceGroupIdChange(value: string): void;
  onDnsHubSubscriptionIdChange(value: string): void;
  onTogglePrivatization(resourceId: string, isPrivatized: boolean): void;
  save(): Promise<void>;
}
