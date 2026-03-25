import { SkuName, StorageKind, AccessTier, TlsVersion } from './types.bicep'

@description('Azure region for the Storage Account')
param location string

@description('Name of the Storage Account')
param name string

@description('SKU of the Storage Account')
param sku SkuName = 'Standard_LRS'

@description('Kind of Storage Account')
param kind StorageKind = 'StorageV2'

@description('Access tier for blob storage')
param accessTier AccessTier = 'Hot'

@description('Whether public access to blobs is allowed')
param allowBlobPublicAccess bool

@description('Whether HTTPS traffic only is enforced')
param supportsHttpsTrafficOnly bool

@description('Minimum TLS version for client connections')
param minimumTlsVersion TlsVersion = 'TLS1_2'

var storagePropertiesBase = {
  allowBlobPublicAccess: allowBlobPublicAccess
  supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
  minimumTlsVersion: minimumTlsVersion
}

var storageAccessTierProperties = contains([
  'BlobStorage'
  'StorageV2'
], kind) ? {
  accessTier: accessTier
} : {}

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  kind: kind
  sku: {
    name: sku
  }
  properties: union(storagePropertiesBase, storageAccessTierProperties)
}

output vaultUri string = kv.properties.vaultUri
