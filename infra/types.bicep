@export()
type EnvironmentName = 'production' | 'development'

@export()
type EnvironmentVariables = {
  envName: string
  envShort: string
  envSuffix: string
  envPrefix: string
  location: string
}

@export()
var environments = {
  production: {
    envName: 'Production'
    envShort: ''
    envSuffix: ''
    envPrefix: ''
    location: 'francecentral'
  }
  development: {
    envName: 'Development'
    envShort: 'dev'
    envSuffix: '--dev'
    envPrefix: 'dev--'
    location: 'francecentral'
  }
}

@description('Rbac Role Type')
@export()
type RbacRoleType = {
  @description('Identifier of the role')
  id: string

  @description('Name of the role')
  description: string
}
