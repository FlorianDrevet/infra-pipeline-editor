import { SkuName } from './types.bicep'

@description('Azure region for the SQL Database')
param location string

@description('Name of the SQL Database')
param name string

@description('Name of the parent SQL Server')
param sqlServerName string

@description('SKU of the SQL Database')
param sku SkuName = 'Basic'

@description('Maximum size of the database in bytes')
param maxSizeBytes int

@description('Collation of the database')
param collation string

@description('Whether the database is zone redundant')
param zoneRedundant bool

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: name
  location: location
  sku: {
    name: sku
  }
  properties: {
    collation: collation
    maxSizeBytes: maxSizeBytes
    zoneRedundant: zoneRedundant
  }
}

output id string = sqlDatabase.id