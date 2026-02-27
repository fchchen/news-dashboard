@description('The location for all resources')
param location string = resourceGroup().location

@description('Base name for resources')
param baseName string = 'newsdash'

@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Name of the shared Cosmos DB account (COSMOS_DB_ACCOUNT_NAME output from azure-ai-code-agent deployment)')
param cosmosAccountName string

var uniqueSuffix = uniqueString(resourceGroup().id)
var appServicePlanName = '${baseName}-plan-${environment}'
var apiAppName = '${baseName}-api-${environment}'
var functionAppName = '${baseName}-func-${environment}'
var storageAccountName = '${baseName}st${uniqueSuffix}'
var staticWebAppName = '${baseName}-web-${environment}'

// Cosmos DB Account (shared; owned and created by azure-ai-code-agent)
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-02-15-preview' existing = {
  name: cosmosAccountName
}

// Shared database (owned by azure-ai-code-agent)
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-02-15-preview' existing = {
  parent: cosmosAccount
  name: 'DevDb'
}

// Cosmos DB Container - NewsItems
resource newsItemsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-02-15-preview' = {
  parent: cosmosDatabase
  name: 'NewsItems'
  properties: {
    resource: {
      id: 'NewsItems'
      partitionKey: {
        paths: ['/source']
        kind: 'Hash'
      }
      defaultTtl: 1209600 // 14 days
    }
  }
}

// Cosmos DB Container - Snapshots
resource snapshotsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-02-15-preview' = {
  parent: cosmosDatabase
  name: 'Snapshots'
  properties: {
    resource: {
      id: 'Snapshots'
      partitionKey: {
        paths: ['/id']
        kind: 'Hash'
      }
      defaultTtl: 2592000 // 30 days
    }
  }
}

// Storage Account (for Azure Functions)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

// App Service Plan (shared by API and Functions)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true // Linux
  }
}

// API App Service
resource apiApp 'Microsoft.Web/sites@2023-01-01' = {
  name: apiAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'ConnectionStrings__CosmosDb'
          value: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
        }
      ]
    }
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=core.windows.net;AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'CosmosDbConnection'
          value: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
        }
      ]
    }
  }
}

// Static Web App (Angular frontend)
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// Outputs
output cosmosDbEndpoint string = cosmosAccount.properties.documentEndpoint
output apiUrl string = 'https://${apiApp.properties.defaultHostName}'
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
