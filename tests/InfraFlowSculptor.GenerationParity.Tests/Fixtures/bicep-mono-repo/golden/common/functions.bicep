import { EnvironmentVariables } from 'types.bicep'

@description('Builds the default resource name from template: {name}-{resourceAbbr}-{suffix}')
@export()
func BuildResourceName(name string, resourceAbbr string, env EnvironmentVariables) string =>
  '${name}-${resourceAbbr}-${env.envSuffix}'

