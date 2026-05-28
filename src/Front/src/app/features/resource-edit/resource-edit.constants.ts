/** SKU options per resource type */
export const KEY_VAULT_SKU_OPTIONS = [
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

export const REDIS_SKU_OPTIONS = [
  { label: 'Basic', value: 'Basic' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

export const REDIS_TLS_OPTIONS = [
  { label: 'TLS 1.0', value: 'Tls10' },
  { label: 'TLS 1.1', value: 'Tls11' },
  { label: 'TLS 1.2', value: 'Tls12' },
];

export const REDIS_EVICTION_OPTIONS = [
  { label: 'NoEviction', value: 'NoEviction' },
  { label: 'AllKeysLru', value: 'AllKeysLru' },
  { label: 'VolatileLru', value: 'VolatileLru' },
  { label: 'AllKeysRandom', value: 'AllKeysRandom' },
  { label: 'VolatileRandom', value: 'VolatileRandom' },
  { label: 'VolatileTtl', value: 'VolatileTtl' },
  { label: 'AllKeysLfu', value: 'AllKeysLfu' },
  { label: 'VolatileLfu', value: 'VolatileLfu' },
];

export const REDIS_VERSION_OPTIONS = [
  { label: 'Redis 4', value: 4 },
  { label: 'Redis 6', value: 6 },
];

export const STORAGE_SKU_OPTIONS = [
  { label: 'Standard_LRS', value: 'Standard_LRS' },
  { label: 'Standard_GRS', value: 'Standard_GRS' },
  { label: 'Standard_RAGRS', value: 'Standard_RAGRS' },
  { label: 'Standard_ZRS', value: 'Standard_ZRS' },
  { label: 'Premium_LRS', value: 'Premium_LRS' },
  { label: 'Premium_ZRS', value: 'Premium_ZRS' },
];

export const STORAGE_KIND_OPTIONS = [
  { label: 'StorageV2', value: 'StorageV2' },
  { label: 'BlobStorage', value: 'BlobStorage' },
  { label: 'BlockBlobStorage', value: 'BlockBlobStorage' },
];

export const STORAGE_ACCESS_TIER_OPTIONS = [
  { label: 'Hot', value: 'Hot' },
  { label: 'Cool', value: 'Cool' },
];

export const STORAGE_TLS_OPTIONS = [
  { label: 'TLS 1.0', value: 'Tls10' },
  { label: 'TLS 1.1', value: 'Tls11' },
  { label: 'TLS 1.2', value: 'Tls12' },
];

export const STORAGE_CORS_METHOD_OPTIONS = ['DELETE', 'GET', 'HEAD', 'MERGE', 'OPTIONS', 'PATCH', 'POST', 'PUT'];
export const STORAGE_CORS_ALLOWED_HEADER_SUGGESTIONS = ['authorization', 'content-type', 'x-ms-*', 'x-ms-meta*', 'x-ms-client-request-id'];
export const STORAGE_CORS_EXPOSED_HEADER_SUGGESTIONS = ['content-type', 'etag', 'x-ms-*', 'x-ms-meta*', 'x-ms-request-id'];
export const STORAGE_CORS_MAX_AGE_PRESETS = [300, 3600, 86400];

export const APP_CONFIGURATION_SKU_OPTIONS = [
  { label: 'Free', value: 'Free' },
  { label: 'Standard', value: 'Standard' },
];

export const APP_CONFIGURATION_PUBLIC_NETWORK_OPTIONS = [
  { label: 'Enabled', value: 'Enabled' },
  { label: 'Disabled', value: 'Disabled' },
];

export const CAE_SKU_OPTIONS = [
  { label: 'Consumption', value: 'Consumption' },
  { label: 'Premium', value: 'Premium' },
];

export const CAE_WORKLOAD_PROFILE_OPTIONS = [
  { label: 'Consumption', value: 'Consumption' },
  { label: 'D4', value: 'D4' },
  { label: 'D8', value: 'D8' },
  { label: 'D16', value: 'D16' },
  { label: 'D32', value: 'D32' },
  { label: 'E4', value: 'E4' },
  { label: 'E8', value: 'E8' },
  { label: 'E16', value: 'E16' },
  { label: 'E32', value: 'E32' },
];

export const CA_CPU_OPTIONS = [
  { label: '0.25', value: '0.25' },
  { label: '0.5', value: '0.5' },
  { label: '1.0', value: '1.0' },
  { label: '2.0', value: '2.0' },
  { label: '4.0', value: '4.0' },
];

export const CA_MEMORY_OPTIONS = [
  { label: '0.5 Gi', value: '0.5Gi' },
  { label: '1.0 Gi', value: '1.0Gi' },
  { label: '2.0 Gi', value: '2.0Gi' },
  { label: '4.0 Gi', value: '4.0Gi' },
  { label: '8.0 Gi', value: '8.0Gi' },
];

export const CA_TRANSPORT_OPTIONS = [
  { label: 'Auto', value: 'auto' },
  { label: 'HTTP', value: 'http' },
  { label: 'HTTP/2', value: 'http2' },
  { label: 'TCP', value: 'tcp' },
];

export const LAW_SKU_OPTIONS = [
  { label: 'Free', value: 'Free' },
  { label: 'PerGB2018', value: 'PerGB2018' },
  { label: 'PerNode', value: 'PerNode' },
  { label: 'Premium', value: 'Premium' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Standalone', value: 'Standalone' },
  { label: 'Capacity Reservation', value: 'CapacityReservation' },
];

export const AI_RETENTION_OPTIONS = [
  { label: '30 days', value: 30 },
  { label: '60 days', value: 60 },
  { label: '90 days', value: 90 },
  { label: '120 days', value: 120 },
  { label: '180 days', value: 180 },
  { label: '270 days', value: 270 },
  { label: '365 days', value: 365 },
  { label: '550 days', value: 550 },
  { label: '730 days', value: 730 },
];

export const AI_INGESTION_MODE_OPTIONS = [
  { label: 'Application Insights', value: 'ApplicationInsights' },
  { label: 'Log Analytics', value: 'LogAnalytics' },
  { label: 'App Insights + Diagnostic Settings', value: 'ApplicationInsightsWithDiagnosticSettings' },
];

export const COSMOS_API_TYPE_OPTIONS = [
  { label: 'SQL (NoSQL)', value: 'SQL' },
  { label: 'MongoDB', value: 'MongoDB' },
  { label: 'Cassandra', value: 'Cassandra' },
  { label: 'Table', value: 'Table' },
  { label: 'Gremlin', value: 'Gremlin' },
];

export const COSMOS_CONSISTENCY_LEVEL_OPTIONS = [
  { label: 'Eventual', value: 'Eventual' },
  { label: 'Consistent Prefix', value: 'ConsistentPrefix' },
  { label: 'Session', value: 'Session' },
  { label: 'Bounded Staleness', value: 'BoundedStaleness' },
  { label: 'Strong', value: 'Strong' },
];

export const COSMOS_BACKUP_POLICY_OPTIONS = [
  { label: 'Periodic', value: 'Periodic' },
  { label: 'Continuous', value: 'Continuous' },
];

export const ACR_SKU_OPTIONS = [
  { label: 'Basic', value: 'Basic' },
  { label: 'Standard', value: 'Standard' },
  { label: 'Premium', value: 'Premium' },
];

export const ACR_PUBLIC_NETWORK_OPTIONS = [
  { label: 'Enabled', value: 'Enabled' },
  { label: 'Disabled', value: 'Disabled' },
];

export const SQL_SERVER_VERSION_OPTIONS = [
  { label: '12.0', value: 'V12' },
];

export const SQL_MIN_TLS_OPTIONS = [
  { label: 'TLS 1.0', value: '1.0' },
  { label: 'TLS 1.1', value: '1.1' },
  { label: 'TLS 1.2', value: '1.2' },
];

export const WEBAPP_RUNTIME_VERSION_MAP: Record<string, string[]> = {
  DotNet: ['10', '9', '8'],
  Node: ['22-lts', '20-lts'],
  Python: ['3.13', '3.12', '3.11', '3.10'],
  Java: ['21', '17', '11'],
  Php: ['8.4', '8.3', '8.2'],
};

export const FUNCTIONAPP_RUNTIME_VERSION_MAP: Record<string, string[]> = {
  DotNet: ['10-isolated', '9-isolated', '8-isolated', '8-in-process'],
  Node: ['22', '20'],
  Python: ['3.12', '3.11', '3.10'],
  Java: ['21', '17', '11'],
  PowerShell: ['7.4', '7.2'],
};

export const DOCUMENT_INTELLIGENCE_SKU_OPTIONS = [
  { label: 'Free (F0)', value: 'F0' },
  { label: 'Standard (S0)', value: 'S0' },
];

export const DOCUMENT_INTELLIGENCE_PUBLIC_NETWORK_OPTIONS = [
  { label: 'Enabled', value: 'Enabled' },
  { label: 'Disabled', value: 'Disabled' },
];