import { SkuName, PublicNetworkAccess } from './types.bicep'

@description('Azure region for the App Configuration store')
param location string

@description('Name of the App Configuration store')
param name string

@description('SKU of the App Configuration store')
param sku SkuName = 'standard'

@description('Number of days to retain soft-deleted items')
param softDeleteRetentionInDays int = 7

@description('Whether purge protection is enabled')
param enablePurgeProtection bool = false

@description('Whether local authentication is disabled')
param disableLocalAuth bool = false

@description('Public network access setting')
param publicNetworkAccess PublicNetworkAccess = 'Enabled'

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  properties: {
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection
    disableLocalAuth: disableLocalAuth
    publicNetworkAccess: publicNetworkAccess
  }
}

output vaultUri string = kv.properties.vaultUri
