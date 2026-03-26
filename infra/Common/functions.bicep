import { EnvironmentVariables } from 'types.bicep'

@description('Builds the default resource name from template: {name}-{resourceAbbr}{suffix}')
@export()
func BuildResourceName(name string, resourceAbbr string, env EnvironmentVariables) string =>
  '${name}-${resourceAbbr}${env.envSuffix}'

@description('Builds a ResourceGroup name from template: {resourceAbbr}-{name}{suffix}')
@export()
func BuildResourceGroupName(name string, resourceAbbr string, env EnvironmentVariables) string =>
  '${resourceAbbr}-${name}${env.envSuffix}'

@description('Builds a StorageAccount name from template: {name}{resourceAbbr}{envShort}')
@export()
func BuildStorageAccountName(name string, resourceAbbr string, env EnvironmentVariables) string =>
  '${name}${resourceAbbr}${env.envShort}'

