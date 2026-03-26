targetScope = 'subscription'

import { EnvironmentName, environments } from '../Common/types.bicep'
import { BuildStorageAccountName, BuildResourceGroupName, BuildResourceName } from '../Common/functions.bicep'
import { RbacRoles } from '../Common/constants.bicep'

@description('The target deployment environment')
param environmentName EnvironmentName

param storageAccountIfsSku string
param storageAccountIfsKind string
param storageAccountIfsAccessTier string
param storageAccountIfsAllowBlobPublicAccess bool
param storageAccountIfsSupportsHttpsTrafficOnly bool
param storageAccountIfsMinimumTlsVersion string

var env = environments[environmentName]

resource ifs 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: BuildResourceGroupName('ifs', 'rg', env)
  location: env.location
}

// ── Cross-configuration existing resource groups ──────────────────
resource existing_coreIfs 'Microsoft.Resources/resourceGroups@2024-07-01' existing = {
  name: BuildResourceGroupName('core-ifs', 'rg', env)
}

// ── Cross-configuration existing resources ──────────────────────
resource existing_ifs 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: BuildResourceName('ifs', 'law', env)
  scope: existing_coreIfs
}

module containerAppIfsBackendModule '../Common/modules/ContainerApp/containerApp.bicep' = {
  name: 'containerAppIfsBackend'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs-backend', 'ca', env)
  }
}

module applicationInsightsIfsModule '../Common/modules/ApplicationInsights/applicationInsights.bicep' = {
  name: 'applicationInsightsIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'appi', env)
  }
}

module storageAccountIfsModule '../Common/modules/StorageAccount/storageAccount.bicep' = {
  name: 'storageAccountIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildStorageAccountName('ifs', 'stg', env)
    sku: storageAccountIfsSku
    kind: storageAccountIfsKind
    accessTier: storageAccountIfsAccessTier
    allowBlobPublicAccess: storageAccountIfsAllowBlobPublicAccess
    supportsHttpsTrafficOnly: storageAccountIfsSupportsHttpsTrafficOnly
    minimumTlsVersion: storageAccountIfsMinimumTlsVersion
  }
}

module storageAccountIfsBlobsModule '../Common/modules/StorageAccount/storage.blobs.module.bicep' = {
  name: 'storageAccountIfsBlobs'
  scope: ifs
  params: {
    storageAccountName: BuildStorageAccountName('ifs', 'stg', env)
    blobContainerNames: [ 'test' ]
    corsRules: 
[]
  }
}

module userAssignedIdentityIfsBackendModule '../Common/modules/UserAssignedIdentity/userAssignedIdentity.bicep' = {
  name: 'userAssignedIdentityIfsBackend'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs-backend', 'id', env)
  }
}

module containerAppEnvironmentIfsModule '../Common/modules/ContainerAppEnvironment/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'cae', env)
  }
}

module containerAppIfsBackendstorageAccountIfsRoles '../Common/modules/StorageAccount/storage.roleassignments.module.bicep' = {
  name: 'containerAppIfsBackendstorageAccountIfsRoles'
  scope: ifs
  params: {
    name: BuildStorageAccountName('ifs', 'stg', env)
    principalId: containerAppIfsBackendModule.outputs.principalId
    roles: [
      RbacRoles.storage['Storage Blob Data Reader']
    ]
  }
}


