// =======================================================================
// App Configuration Role Assignment Module
// -----------------------------------------------------------------------
// Module: appconfiguration.roleassignments.module.bicep
// Description: Creates role assignments for App Configuration resources
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.authorization/roleassignments
// =======================================================================

import { RbacRoleType } from '../../types.bicep'

@description('The name of the App Configuration instance')
param name string

@description('The principal ID to assign the role to')
param principalId string

@description('The roles to assign to the principal')
param roles RbacRoleType[]

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: name
}

resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in roles: {
  scope: appConfig
  name: guid(appConfig.id, principalId, role.id)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', role.id)
    principalId: principalId
    description: role.description
  }
}]