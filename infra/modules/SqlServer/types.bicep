@export()
@description('SQL Server version')
type SqlServerVersion = '12.0'

@export()
@description('Minimum TLS version for SQL Server connections')
type TlsVersion = '1.0' | '1.1' | '1.2'