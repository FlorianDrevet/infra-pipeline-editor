// =======================================================================
// App Service Plan Module
// -----------------------------------------------------------------------
// Module: appServicePlan.module.bicep
// Description: Deploys an Azure App Service Plan resource
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.web/serverfarms
// =======================================================================

import { SkuName, OsType } from './types.bicep'

@description('Azure region for the App Service Plan')
param location string

@description('Name of the App Service Plan')
param name string

@description('SKU name of the App Service Plan')
param sku SkuName = 'F1'

@description('Number of instances allocated to the plan')
param capacity int

@description('Operating system type')
param osType OsType = 'Linux'
@description('Resource tags')
param tags object = {}


var isLinux = osType == 'Linux'
var kind = isLinux ? 'linux' : 'app'

resource asp 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: kind
  sku: {
    name: sku
    capacity: capacity
  }
  properties: {
    reserved: isLinux
  }
}
