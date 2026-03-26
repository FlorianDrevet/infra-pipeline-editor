@export()
@description('SKU for the Container App Environment')
type SkuName = 'Consumption' | 'Premium'

@export()
@description('Workload profile type for the Container App Environment')
type WorkloadProfileType = 'Consumption' | 'D4' | 'D8' | 'D16' | 'D32' | 'E4' | 'E8' | 'E16' | 'E32'