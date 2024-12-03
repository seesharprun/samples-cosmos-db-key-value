targetScope = 'resourceGroup'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Id of the principal that is deploying the template')
param deploymentUserPrincipalId string = ''

var resourceToken = toLower(uniqueString(resourceGroup().id, environmentName, location))
var tags = {
  'azd-env-name': environmentName
  repo: 'https://github.com/seesharprun/samples-cosmos-db-key-value'
}

module nosql 'br/public:avm/res/document-db/database-account:0.8.1' = {
  name: 'cosmos-db-nosql-account'
  params: {
    name: 'cosmos-db-nosql-${resourceToken}'
    location: location
    tags: tags
    disableKeyBasedMetadataWriteAccess: true
    disableLocalAuth: true
    networkRestrictions: {
      publicNetworkAccess: 'Enabled'
      ipRules: []
      virtualNetworkRules: []
    }
    capabilitiesToAdd: [
      'EnableServerless'
    ]
    sqlRoleDefinitions: [
      {
        name: 'nosql-data-plane-contributor'
        dataAction: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
        ]
      }
    ]
    sqlRoleAssignmentsPrincipalIds: !empty(deploymentUserPrincipalId) ? [
      deploymentUserPrincipalId
    ] : null
    sqlDatabases: [
      {
        name: 'example-database'
        containers: [
          {
            name: 'example-container'
            paths: [
              '/organization'
            ]
            // Tmp: I can't disable indexing yet because of a bug in AVM where you can't disable TTL which is required.
          }
        ]
      }
    ]
  }
}

module table 'br/public:avm/res/document-db/database-account:0.8.1' = {
  name: 'cosmos-db-table-account'
  params: {
    name: 'cosmos-db-table-${resourceToken}'
    location: location
    tags: tags
    disableKeyBasedMetadataWriteAccess: true
    disableLocalAuth: true
    networkRestrictions: {
      publicNetworkAccess: 'Enabled'
      ipRules: []
      virtualNetworkRules: []
    }
    capabilitiesToAdd: [
      'EnableServerless'
      'EnableTable'
    ]
    sqlRoleDefinitions: [
      {
        name: 'table-data-plane-contributor'
        dataAction: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/tables/*'
          'Microsoft.DocumentDB/databaseAccounts/tables/containers/entities/*'          
        ]
      }
    ]
    sqlRoleAssignmentsPrincipalIds: !empty(deploymentUserPrincipalId) ? [
      deploymentUserPrincipalId
    ] : null
    tables: [
      {
        name: 'example-table'
        defaultTtl: any(null)
        indexingPolicy: {
          indexingMode: 'none'
        }
      }
    ]
  }
}

output NOSQL_ENDPOINT string = nosql.outputs.endpoint
output NOSQL_DATABASENAME string = 'example-database'
output NOSQL_CONTAINERNAME string = 'example-container'

output TABLE_ENDPOINT string = 'https://${table.outputs.name}.table.cosmos.azure.com:443/'
output TABLE_TABLENAME string = 'example-table'
