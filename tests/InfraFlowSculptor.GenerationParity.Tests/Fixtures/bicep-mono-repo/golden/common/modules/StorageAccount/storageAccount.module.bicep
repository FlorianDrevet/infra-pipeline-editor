// =======================================================================
// Storage Account Module
// -----------------------------------------------------------------------
// Module: storageAccount.module.bicep
// Description: Deploys an Azure Storage Account resource
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts
// =======================================================================

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
@description('Resource tags')
param tags object = {}


resource storage 'Microsoft.Storage/storageAccounts@2025-06-01' = {
  name: name
  location: location
  tags: tags
  kind: kind
  sku: {
    name: sku
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    allowBlobPublicAccess: allowBlobPublicAccess
    supportsHttpsTrafficOnly: supportsHttpsTrafficOnly
    minimumTlsVersion: minimumTlsVersion
    accessTier: accessTier
  }
}





