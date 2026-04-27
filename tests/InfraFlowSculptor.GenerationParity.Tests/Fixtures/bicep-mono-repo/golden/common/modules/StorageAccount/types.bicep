@export()
@description('SKU name for the Storage Account')
type SkuName = 'Standard_LRS' | 'Standard_GRS' | 'Standard_RAGRS' | 'Standard_ZRS' | 'Premium_LRS' | 'Premium_ZRS'

@export()
@description('Kind of Storage Account')
type StorageKind = 'BlobStorage' | 'BlockBlobStorage' | 'FileStorage' | 'Storage' | 'StorageV2'

@export()
@description('Access tier for the Storage Account')
type AccessTier = 'Hot' | 'Cool' | 'Premium'

@export()
@description('Minimum TLS version for Storage Account connections')
type TlsVersion = 'TLS1_0' | 'TLS1_1' | 'TLS1_2'