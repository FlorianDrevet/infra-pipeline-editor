targetScope = 'subscription'

import { EnvironmentName, environments } from '../Common/types.bicep'
import { BuildResourceGroupName, BuildResourceName } from '../Common/functions.bicep'

@description('The target deployment environment')
param environmentName EnvironmentName


var env = environments[environmentName]

resource coreIfs 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: BuildResourceGroupName('core-ifs', 'rg', env)
  location: env.location
}

module logAnalyticsWorkspaceIfsModule '../Common/modules/LogAnalyticsWorkspace/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceIfs'
  scope: coreIfs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'law', env)
  }
}


