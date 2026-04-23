@export()
type EnvironmentName = 'dev'

@export()
type EnvironmentVariables = {
  envName: string
  envShort: string
  envSuffix: string
  envPrefix: string
  location: string
  tags: object
}

@export()
var environments = {
  dev: {
    envName: 'dev'
    envShort: 'dev'
    envSuffix: 'dev'
    envPrefix: 'dev'
    location: 'westeurope'
    tags: {
      env: 'dev'
    }
  }
}
