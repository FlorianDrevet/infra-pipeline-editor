targetScope = 'subscription'

import { EnvironmentName, environments } from 'types.bicep'
import { BuildStorageAccountName, BuildResourceGroupName, BuildResourceName } from 'functions.bicep'

@description('The target deployment environment')
param environmentName EnvironmentName

param sqlDatabaseIfsSku string
param sqlDatabaseIfsMaxSizeBytes int
param sqlDatabaseIfsCollation string
param sqlDatabaseIfsZoneRedundant bool
param storageAccountIfsSku string
param storageAccountIfsKind string
param storageAccountIfsAccessTier string
param storageAccountIfsAllowBlobPublicAccess bool
param storageAccountIfsSupportsHttpsTrafficOnly bool
param storageAccountIfsMinimumTlsVersion string
param sqlServerIfsVersion string
param sqlServerIfsAdministratorLogin string
param sqlServerIfsMinimalTlsVersion string

var env = environments[environmentName]

resource ifs 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: BuildResourceGroupName('ifs', 'rg', env)
  location: env.location
}

module sqlDatabaseIfsModule './modules/SqlDatabase/sqlDatabase.bicep' = {
  name: 'sqlDatabaseIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'sqldb', env)
    sku: sqlDatabaseIfsSku
    maxSizeBytes: sqlDatabaseIfsMaxSizeBytes
    collation: sqlDatabaseIfsCollation
    zoneRedundant: sqlDatabaseIfsZoneRedundant
  }
}

module storageAccountIfsModule './modules/StorageAccount/storageAccount.bicep' = {
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

module containerAppFrontIfsModule './modules/ContainerApp/containerApp.bicep' = {
  name: 'containerAppFrontIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('front-ifs', 'ca', env)
  }
}

module containerAppBackendIfsModule './modules/ContainerApp/containerApp.bicep' = {
  name: 'containerAppBackendIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('backend-ifs', 'ca', env)
  }
}

module applicationInsightsIfsModule './modules/ApplicationInsights/applicationInsights.bicep' = {
  name: 'applicationInsightsIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'appi', env)
  }
}

module logAnalyticsWorkspaceIfsModule './modules/LogAnalyticsWorkspace/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'law', env)
  }
}

module sqlServerIfsModule './modules/SqlServer/sqlServer.bicep' = {
  name: 'sqlServerIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'sql', env)
    version: sqlServerIfsVersion
    administratorLogin: sqlServerIfsAdministratorLogin
    minimalTlsVersion: sqlServerIfsMinimalTlsVersion
  }
}

module containerAppEnvironmentIfsModule './modules/ContainerAppEnvironment/containerAppEnvironment.bicep' = {
  name: 'containerAppEnvironmentIfs'
  scope: ifs
  params: {
    location: env.location
    name: BuildResourceName('ifs', 'cae', env)
  }
}

