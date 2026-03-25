// Role definitions id
@export()
@description('RBAC roles grouped by Azure service')
var RbacRoles = {
  appconfiguration: {
    'App Configuration Data Reader': {
      id: '516239f1-63e1-4d78-a4de-a74fb236a071'
      description: 'Allows read access to App Configuration data.'
    }
  }
  storage: {
    'Storage Blob Data Contributor': {
      id: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
      description: 'Read, write, and delete Azure Storage containers and blobs.'
    }
  }
}
