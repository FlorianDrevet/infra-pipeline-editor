// =======================================================================
// Storage Account Role Assignment Module
// -----------------------------------------------------------------------
// Module: storage.roleassignments.module.bicep
// Description: Creates role assignments for Storage Account resources
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.authorization/roleassignments
// =======================================================================

import { RbacRoleType } from '../../types.bicep'

@description('The name of the Storage Account instance')
param name string

@description('The principal ID to assign the role to')
param principalId string

@description('The roles to assign to the principal')
param roles RbacRoleType[]

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: name
}

resource roleAssignments 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for role in roles: {
  scope: storageAccount
  name: guid(storageAccount.id, principalId, role.id)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', role.id)
    principalId: principalId
    description: role.description
  }
}]