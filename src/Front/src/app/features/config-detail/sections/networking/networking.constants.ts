import { DsSelectOption } from '../../../../shared/components/ds';

export const NETWORKING_MODE_OPTIONS: DsSelectOption[] = [
  { value: 'Simplified', label: 'CONFIG_DETAIL_NETWORKING.MODE.SIMPLIFIED' },
  { value: 'Standard', label: 'CONFIG_DETAIL_NETWORKING.MODE.STANDARD' },
  { value: 'Advanced', label: 'CONFIG_DETAIL_NETWORKING.MODE.ADVANCED' },
];

export const VNET_SOURCE_TYPE_OPTIONS: DsSelectOption[] = [
  { value: 'CreateNew', label: 'CONFIG_DETAIL_NETWORKING.VNET.CREATE_NEW' },
  { value: 'UseExisting', label: 'CONFIG_DETAIL_NETWORKING.VNET.USE_EXISTING' },
  { value: 'UseHubSpoke', label: 'CONFIG_DETAIL_NETWORKING.VNET.USE_HUB_SPOKE' },
];

export const DNS_MODE_OPTIONS: DsSelectOption[] = [
  { value: 'AutoManaged', label: 'CONFIG_DETAIL_NETWORKING.DNS.AUTO_MANAGED' },
  { value: 'CentralizedHub', label: 'CONFIG_DETAIL_NETWORKING.DNS.CENTRALIZED_HUB' },
  { value: 'Custom', label: 'CONFIG_DETAIL_NETWORKING.DNS.CUSTOM' },
];
