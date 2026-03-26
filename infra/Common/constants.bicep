// Role definitions id
@export()
@description('RBAC roles grouped by Azure service')
var RbacRoles = {
  storage: {
    'Storage Blob Data Reader': {
      id: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
      description: 'Read and list Azure Storage containers and blobs.'
    }
  }
}
