targetScope = 'subscription'

import { EnvironmentName, environments } from '../Common/types.bicep'
import { BuildResourceName } from '../Common/functions.bicep'

@description('The target deployment environment')
param environmentName EnvironmentName

param keyVaultKvapiSku string
param appServicePlanAspapiSku string
param appServicePlanAspapiCapacity int
param appServicePlanAspapiOsType string

var env = environments[environmentName]

var projectTags = {
  project: 'parity'
}

var configTags = {
  component: 'api'
}

var tags = union(projectTags, configTags, env.tags)

resource rgApi 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: BuildResourceName('rg-api', 'rg', env)
  location: env.location
  tags: tags
}

module keyVaultKvapiModule '../Common/modules/KeyVault/keyVault.module.bicep' = {
  name: 'keyVaultKvapi'
  scope: rgApi
  params: {
    location: env.location
    name: BuildResourceName('kvapi', 'kv', env)
    tags: tags
    sku: keyVaultKvapiSku
  }
}

module appServicePlanAspapiModule '../Common/modules/AppServicePlan/appServicePlan.module.bicep' = {
  name: 'appServicePlanAspapi'
  scope: rgApi
  params: {
    location: env.location
    name: BuildResourceName('aspapi', 'asp', env)
    tags: tags
    sku: appServicePlanAspapiSku
    capacity: appServicePlanAspapiCapacity
    osType: appServicePlanAspapiOsType
  }
}


