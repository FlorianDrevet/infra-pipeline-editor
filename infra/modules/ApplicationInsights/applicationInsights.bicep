import { IngestionMode } from './types.bicep'

@description('Azure region for the Application Insights resource')
param location string

@description('Name of the Application Insights resource')
param name string

@description('Resource ID of the Log Analytics workspace')
param logAnalyticsWorkspaceId string

@description('Sampling percentage (0-100)')
param samplingPercentage int = 100

@description('Number of days to retain data')
param retentionInDays int = 90

@description('Whether IP masking is disabled')
param disableIpMasking bool = false

@description('Whether local authentication is disabled')
param disableLocalAuth bool = false

@description('Ingestion mode for telemetry data')
param ingestionMode IngestionMode = 'LogAnalytics'

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
    SamplingPercentage: samplingPercentage
    RetentionInDays: retentionInDays
    DisableIpMasking: disableIpMasking
    DisableLocalAuth: disableLocalAuth
    IngestionMode: ingestionMode
  }
}

output vaultUri string = kv.properties.vaultUri
