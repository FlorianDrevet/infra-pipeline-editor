// =======================================================================
// Container App Module
// -----------------------------------------------------------------------
// Module: containerAppIfsBackend.module.bicep
// Description: Deploys an Azure Container App resource
// See: https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps
// =======================================================================

import { TransportMethod } from './types.bicep'

@description('Azure region for the Container App')
param location string

@description('Name of the Container App')
param name string

@description('Resource ID of the Container App Environment')
param containerAppEnvironmentId string

@description('Container image to deploy')
param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('CPU cores allocated to the container')
param cpuCores string = '0.25'

@description('Memory allocated to the container (e.g. 0.5Gi)')
param memoryGi string = '0.5Gi'

@description('Minimum number of replicas')
param minReplicas int = 0

@description('Maximum number of replicas')
param maxReplicas int = 1

@description('Whether ingress is enabled')
param ingressEnabled bool = true

@description('Target port for ingress traffic')
param ingressTargetPort int = 80

@description('Whether ingress is externally accessible')
param ingressExternal bool = true

@description('Transport method for ingress')
param transportMethod TransportMethod = 'auto'

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    configuration: {
      ingress: ingressEnabled ? {
        external: ingressExternal
        targetPort: ingressTargetPort
        transport: transportMethod
      } : null
    }
    template: {
      containers: [
        {
          name: name
          image: containerImage
          resources: {
            cpu: json(cpuCores)
            memory: memoryGi
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

output principalId string = containerApp.identity.principalId
