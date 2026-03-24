import { SkuName } from './types.bicep'

@description('Azure region for the Log Analytics workspace')
param location string

@description('Name of the Log Analytics workspace')
param name string

@description('SKU of the Log Analytics workspace')
param sku SkuName = 'PerGB2018'

@description('Number of days to retain data')
param retentionInDays int = 30

@description('Daily ingestion quota in GB (-1 for unlimited)')
param dailyQuotaGb int = -1

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  properties: {
    sku: {
      name: sku
    }
    retentionInDays: retentionInDays
    workspaceCapping: {
      dailyQuotaGb: dailyQuotaGb
    }
  }
}

output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id