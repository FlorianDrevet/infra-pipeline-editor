// =======================================================================
// Storage Account Module
// -----------------------------------------------------------------------
// Module: storage.blobs.module.module.bicep
// Description: Deploys an Azure Storage Account resource
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts
// =======================================================================

import { CorsRuleDescription } from './types.bicep'

@description('Storage account name')
param storageAccountName string

@description('Blob containers names')
param blobContainerNames string[]

@description('CORS rules')
param corsRules CorsRuleDescription[] = []

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-06-01' existing = {
  name: storageAccountName
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-06-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    cors: {
      corsRules: corsRules
    }
  }
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-06-01' = [
  for blobContainerName in blobContainerNames: {
    name: blobContainerName
    parent: blobService
  }
]