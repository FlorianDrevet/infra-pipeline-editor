using 'main.bicep'

param environmentName = 'Development'



param storageAccountIfsSku = 'Standard_LRS'
param storageAccountIfsKind = 'StorageV2'
param storageAccountIfsAccessTier = 'Hot'
param storageAccountIfsAllowBlobPublicAccess = false
param storageAccountIfsSupportsHttpsTrafficOnly = true
param storageAccountIfsMinimumTlsVersion = 'TLS1_2'



