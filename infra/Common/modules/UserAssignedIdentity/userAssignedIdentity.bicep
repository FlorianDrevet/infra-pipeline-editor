// =======================================================================
// User Assigned Identity Module
// -----------------------------------------------------------------------
// Module: userAssignedIdentityIfsBackend.module.bicep
// Description: Deploys an Azure User Assigned Identity resource
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.managedidentity/userassignedidentities
// =======================================================================

@description('Azure region for the User Assigned Identity')
param location string

@description('Name of the User Assigned Identity')
param name string

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: name
  location: location
}

output principalId string = identity.properties.principalId
output clientId string = identity.properties.clientId