import { Signal } from '@angular/core';

import { DsSelectOption } from '../../../../shared/components/ds';
import { PrivateEndpointConfigResponse } from '../../../../shared/interfaces/private-endpoint-config.interface';

export interface ResourceEditNetworkingSection {
  readonly isPrivatized: Signal<boolean>;
  readonly privateEndpointConfig: Signal<PrivateEndpointConfigResponse | null>;
  readonly vnetOptions: Signal<DsSelectOption[]>;
  readonly subnetOptions: Signal<DsSelectOption[]>;
  readonly isLoadingSubnets: Signal<boolean>;
  readonly isLoading: Signal<boolean>;
  readonly isSaving: Signal<boolean>;
  readonly errorKey: Signal<string>;

  load(): void;
  loadSubnetsForVnet(vnetId: string): Promise<void>;
  togglePrivatization(value: boolean): Promise<void>;
  saveConfig(config: { virtualNetworkId: string; subnetName: string; dnsMode: string; dnsHubResourceGroupId?: string; dnsHubSubscriptionId?: string }): Promise<void>;
  removeConfig(): Promise<void>;
}
