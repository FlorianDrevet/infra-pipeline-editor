using 'main.bicep'

param environmentName = 'Production'

param sqlDatabaseIfsSku = 'Basic'
param sqlDatabaseIfsMaxSizeBytes = 2147483648
param sqlDatabaseIfsCollation = 'SQL_Latin1_General_CP1_CI_AS'
param sqlDatabaseIfsZoneRedundant = 'false'

param storageAccountIfsSku = 'Standard_LRS'
param storageAccountIfsKind = 'StorageV2'
param storageAccountIfsAccessTier = 'Hot'
param storageAccountIfsAllowBlobPublicAccess = 'false'
param storageAccountIfsSupportsHttpsTrafficOnly = 'true'
param storageAccountIfsMinimumTlsVersion = 'TLS1_2'



param appConfigurationIfsSku = 'Standard'



param keyVaultIfsSku = 'standard'


param sqlServerIfsVersion = 'V12'
param sqlServerIfsAdministratorLogin = 'sqladmin'
param sqlServerIfsMinimalTlsVersion = '1.2'


