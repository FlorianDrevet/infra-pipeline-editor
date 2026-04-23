targetScope = 'subscription'

import { EnvironmentName, environments } from 'types.bicep'
import { BuildResourceName } from 'functions.bicep'

@description('The target deployment environment')
param environmentName EnvironmentName

param keyVaultKvtestSku string

var env = environments[environmentName]

var projectTags = {
  project: 'parity'
}

var tags = union(projectTags, env.tags)

resource rgTest 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: BuildResourceName('rg-test', 'rg', env)
  location: env.location
  tags: tags
}

module keyVaultKvtestModule './modules/KeyVault/keyVault.module.bicep' = {
  name: 'keyVaultKvtest'
  scope: rgTest
  params: {
    location: env.location
    name: BuildResourceName('kvtest', 'kv', env)
    tags: tags
    sku: keyVaultKvtestSku
  }
}

