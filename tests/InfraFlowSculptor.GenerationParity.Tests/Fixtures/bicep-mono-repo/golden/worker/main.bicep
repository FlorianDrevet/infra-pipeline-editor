targetScope = 'subscription'

import { EnvironmentName, environments } from '../Common/types.bicep'
import { BuildResourceName } from '../Common/functions.bicep'

@description('The target deployment environment')
param environmentName EnvironmentName

param storageAccountStworkerSku string
param storageAccountStworkerKind string
param storageAccountStworkerAccessTier string
param storageAccountStworkerAllowBlobPublicAccess bool
param storageAccountStworkerSupportsHttpsTrafficOnly bool
param storageAccountStworkerMinimumTlsVersion string

var env = environments[environmentName]

var projectTags = {
  project: 'parity'
}

var configTags = {
  component: 'worker'
}

var tags = union(projectTags, configTags, env.tags)

resource rgWorker 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: BuildResourceName('rg-worker', 'rg', env)
  location: env.location
  tags: tags
}

module storageAccountStworkerModule '../Common/modules/StorageAccount/storageAccount.module.bicep' = {
  name: 'storageAccountStworker'
  scope: rgWorker
  params: {
    location: env.location
    name: BuildResourceName('stworker', 'st', env)
    tags: tags
    sku: storageAccountStworkerSku
    kind: storageAccountStworkerKind
    accessTier: storageAccountStworkerAccessTier
    allowBlobPublicAccess: storageAccountStworkerAllowBlobPublicAccess
    supportsHttpsTrafficOnly: storageAccountStworkerSupportsHttpsTrafficOnly
    minimumTlsVersion: storageAccountStworkerMinimumTlsVersion
  }
}


