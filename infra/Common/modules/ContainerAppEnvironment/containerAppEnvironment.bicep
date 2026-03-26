// =======================================================================
// Container App Environment Module
// -----------------------------------------------------------------------
// Module: containerAppEnvironmentIfs.module.bicep
// Description: Deploys an Azure Container App Environment resource
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments
// =======================================================================

import { SkuName, WorkloadProfileType } from './types.bicep'

@description('Azure region for the Container App Environment')
param location string

@description('Name of the Container App Environment')
param name string

@description('SKU of the Container App Environment')
param sku SkuName = 'Consumption'

@description('Workload profile type')
param workloadProfileType WorkloadProfileType = 'Consumption'

@description('Whether the internal load balancer is enabled')
param internalLoadBalancerEnabled bool = false

@description('Whether zone redundancy is enabled')
param zoneRedundancyEnabled bool = false

@description('Resource ID of the Log Analytics workspace (empty to skip)')
param logAnalyticsWorkspaceId string = ''

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: name
  location: location
  properties: {
    zoneRedundant: zoneRedundancyEnabled
    vnetConfiguration: {
      internal: internalLoadBalancerEnabled
    }
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: logAnalyticsWorkspaceId != '' ? {
        customerId: logAnalyticsWorkspaceId
      } : null
    }
    workloadProfiles: [
      {
        name: workloadProfileType
        workloadProfileType: workloadProfileType
      }
    ]
  }
}