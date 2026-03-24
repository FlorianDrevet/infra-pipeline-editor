@export()
type EnvironmentName = 'production' | 'development'

@export()
type EnvironmentVariables = {
  envName: string
  envSuffix: string
  envShortSuffix: string
  envPrefix: string
  envShortPrefix: string
  location: string
}

@export()
var environments = {
  production: {
    envName: 'Production'
    envSuffix: ''
    envShortSuffix: ''
    envPrefix: ''
    envShortPrefix: ''
    location: 'francecentral'
  }
  development: {
    envName: 'Development'
    envSuffix: '--dev'
    envShortSuffix: '-dev'
    envPrefix: 'dev--'
    envShortPrefix: 'dev-'
    location: 'francecentral'
  }
}
