import { SkuName } from './types.bicep'

@description('Azure region for the Key Vault')
param location string

@description('Name of the Key Vault')
param name string

@description('SKU of the Key Vault')
param sku SkuName = 'standard'

resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  properties: {
    sku: {
      family: 'A'
      name: sku
    }
    tenantId: subscription().tenantId
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enableSoftDelete: true
  }
}