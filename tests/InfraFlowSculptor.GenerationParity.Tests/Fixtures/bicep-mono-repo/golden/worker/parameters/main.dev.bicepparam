using '../main.bicep'

param environmentName = 'dev'

param storageAccountStworkerSku = 'Standard_LRS'
param storageAccountStworkerKind = 'StorageV2'
param storageAccountStworkerAccessTier = 'Hot'
param storageAccountStworkerAllowBlobPublicAccess = false
param storageAccountStworkerSupportsHttpsTrafficOnly = true
param storageAccountStworkerMinimumTlsVersion = 'TLS1_2'

