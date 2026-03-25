import { SqlServerVersion, TlsVersion } from './types.bicep'

@description('Azure region for the SQL Server')
param location string

@description('Name of the SQL Server')
param name string

@description('SQL Server version')
param version SqlServerVersion = '12.0'

@description('Administrator login name')
param administratorLogin string

@secure()
@description('Administrator login password')
param administratorLoginPassword string

@description('Minimum TLS version for client connections')
param minimalTlsVersion TlsVersion = '1.2'

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: name
  location: location
  properties: {
    version: version
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    minimalTlsVersion: minimalTlsVersion
    publicNetworkAccess: 'Enabled'
  }
}

output id string = sqlServer.id
output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName

output vaultUri string = kv.properties.vaultUri
