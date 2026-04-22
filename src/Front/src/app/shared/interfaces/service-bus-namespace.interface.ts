// ─── Environment Settings ────────────────────────────────────────────────────

export interface ServiceBusNamespaceEnvironmentConfigEntry {
  environmentName: string;
  sku?: string | null;
  capacity?: number | null;
  zoneRedundant?: boolean | null;
  disableLocalAuth?: boolean | null;
  minimumTlsVersion?: string | null;
}

export interface ServiceBusNamespaceEnvironmentConfigResponse {
  environmentName: string;
  sku: string | null;
  capacity: number | null;
  zoneRedundant: boolean | null;
  disableLocalAuth: boolean | null;
  minimumTlsVersion: string | null;
  isExisting?: boolean;
}

// ─── Sub-resource Responses ──────────────────────────────────────────────────

export interface ServiceBusQueueResponse {
  id: string;
  name: string;
  isExisting?: boolean;
}

export interface ServiceBusTopicSubscriptionResponse {
  id: string;
  topicName: string;
  subscriptionName: string;
  isExisting?: boolean;
}

// ─── Response ────────────────────────────────────────────────────────────────

export interface ServiceBusNamespaceResponse {
  id: string;
  resourceGroupId: string;
  name: string;
  location: string;
  queues: ServiceBusQueueResponse[];
  topicSubscriptions: ServiceBusTopicSubscriptionResponse[];
  environmentSettings: ServiceBusNamespaceEnvironmentConfigResponse[];
  isExisting?: boolean;
}

// ─── Requests ────────────────────────────────────────────────────────────────

export interface CreateServiceBusNamespaceRequest {
  resourceGroupId: string;
  name: string;
  location: string;
  environmentSettings?: ServiceBusNamespaceEnvironmentConfigEntry[];
  isExisting?: boolean;
}

export interface UpdateServiceBusNamespaceRequest {
  name: string;
  location: string;
  environmentSettings?: ServiceBusNamespaceEnvironmentConfigEntry[];
}

export interface AddServiceBusQueueRequest {
  name: string;
}

export interface AddServiceBusTopicSubscriptionRequest {
  topicName: string;
  subscriptionName: string;
}
