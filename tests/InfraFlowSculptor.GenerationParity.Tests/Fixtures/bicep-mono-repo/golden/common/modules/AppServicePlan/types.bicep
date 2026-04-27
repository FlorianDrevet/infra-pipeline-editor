@export()
@description('SKU name for the App Service Plan')
type SkuName = 'F1' | 'D1' | 'B1' | 'B2' | 'B3' | 'S1' | 'S2' | 'S3' | 'P1v2' | 'P2v2' | 'P3v2' | 'P1v3' | 'P2v3' | 'P3v3' | 'I1' | 'I2' | 'I3' | 'I1v2' | 'I2v2' | 'I3v2'

@export()
@description('Operating system type for the App Service Plan')
type OsType = 'Linux' | 'Windows'