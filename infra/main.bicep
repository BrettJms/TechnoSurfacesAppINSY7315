@allowed([
  'staging'
  'production'
])
param environment string

param location string = resourceGroup().location

param sqlAdminLogin string = 'tsadmin'

@secure()
param sqlAdminPassword string

@description('Linux App Service runtime. Verified against az webapp list-runtimes --os-type linux --runtime dotnet.')
param linuxRuntime string = 'DOTNETCORE|10.0'

var isProd = environment == 'production'
var unique = uniqueString(resourceGroup().id)

var planName = 'tsqa-plan'
var appName = 'tsqa-app-${environment}'
var sqlServerName = 'tsqa-sql-${unique}'
var databaseName = 'tsqa-db-${environment}'
var kvName = 'tsqa-kv-${unique}'
var lawName = 'tsqa-log'
var aiName = 'tsqa-ai-${environment}'

var sqlHostname = '${sqlServerName}${az.environment().suffixes.sqlServerHostname}'
var appHostname = '${appName}.azurewebsites.net'
var healthUrl = 'https://${appHostname}/health'

// -----------------------------------------------------------------------------
// Network
// -----------------------------------------------------------------------------

resource vnet 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: 'tsqa-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.20.0.0/23'
      ]
    }
    subnets: [
      {
        name: 'sql'
        properties: {
          addressPrefix: '10.20.0.0/24'
        }
      }
      {
        name: 'web'
        properties: {
          addressPrefix: '10.20.1.0/24'
          delegations: [
            {
              name: 'web'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
    ]
  }
}

var sqlSubnetId = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  'sql'
)

var webSubnetId = resourceId(
  'Microsoft.Network/virtualNetworks/subnets',
  vnet.name,
  'web'
)

// -----------------------------------------------------------------------------
// Private DNS for Azure SQL
// -----------------------------------------------------------------------------

resource dnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink${az.environment().suffixes.sqlServerHostname}'
  location: 'global'
}

resource dnsLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: dnsZone
  name: 'tsqa-link'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnet.id
    }
    registrationEnabled: false
  }
}

// -----------------------------------------------------------------------------
// Azure SQL
// -----------------------------------------------------------------------------

resource sql 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    version: '12.0'
    publicNetworkAccess: 'Disabled'
    minimalTlsVersion: '1.2'
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
  }
}

resource db 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sql
  name: databaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
}

resource pe 'Microsoft.Network/privateEndpoints@2023-11-01' = {
  name: 'tsqa-sql-pe'
  location: location
  properties: {
    subnet: {
      id: sqlSubnetId
    }
    privateLinkServiceConnections: [
      {
        name: 'tsqa-sql-conn'
        properties: {
          privateLinkServiceId: sql.id
          groupIds: [
            'sqlServer'
          ]
        }
      }
    ]
  }
}

resource peDns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-11-01' = {
  parent: pe
  name: 'default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'sql'
        properties: {
          privateDnsZoneId: dnsZone.id
        }
      }
    ]
  }
}

// -----------------------------------------------------------------------------
// Key Vault
// -----------------------------------------------------------------------------

resource kv 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: kvName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    tenantId: subscription().tenantId
    accessPolicies: []
  }
}

// -----------------------------------------------------------------------------
// Monitoring
// -----------------------------------------------------------------------------

resource law 'Microsoft.OperationalInsights/workspaces@2025-02-01' = {
  name: lawName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource ai 'Microsoft.Insights/components@2020-02-02' = {
  name: aiName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: law.id
  }
}

// -----------------------------------------------------------------------------
// App Service
// -----------------------------------------------------------------------------

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

resource web 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    virtualNetworkSubnetId: webSubnetId
    siteConfig: {
      linuxFxVersion: linuxRuntime
      minTlsVersion: '1.2'
      alwaysOn: true
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: isProd ? 'Production' : 'Staging'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: ai.properties.ConnectionString
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '@Microsoft.KeyVault(SecretUri=${kv.properties.vaultUri}secrets/app-connection-${environment})'
        }
      ]
    }
  }
}

// Key Vault Secrets User
resource kvReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(kv.id, web.id, 'kv-secrets-user')
  scope: kv
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalId: web.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// -----------------------------------------------------------------------------
// Application Insights Standard availability test
// -----------------------------------------------------------------------------

resource webtest 'Microsoft.Insights/webtests@2022-06-15' = {
  name: 'tsqa-avail-${environment}'
  location: location
  kind: 'standard'
  tags: {
    'hidden-link:${ai.id}': 'Resource'
  }
  properties: {
    Description: 'Availability test for ${appName}'
    Enabled: true
    Frequency: 900
    Kind: 'standard'
    Locations: [
      {
        Id: 'emea-nl-ams-azr'
      }
    ]
    Name: 'tsqa-avail-${environment}'
    Request: {
      FollowRedirects: true
      HttpVerb: 'GET'
      ParseDependentRequests: false
      RequestUrl: healthUrl
    }
    RetryEnabled: true
    SyntheticMonitorId: 'tsqa-avail-${environment}'
    Timeout: 60
    ValidationRules: {
      ExpectedHttpStatusCode: 200
      SSLCheck: true
      SSLCertRemainingLifetimeCheck: 7
    }
  }
}

// -----------------------------------------------------------------------------
// Availability metric alert
// -----------------------------------------------------------------------------

resource availAlert 'Microsoft.Insights/metricalerts@2018-03-01' = {
  name: 'tsqa-avail-alert-${environment}'
  location: 'global'
  properties: {
    description: 'Availability of /health dropped below 90 percent'
    severity: 1
    enabled: true
    scopes: [
      ai.id
    ]
    evaluationFrequency: 'PT30M'
    windowSize: 'PT30M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'Availability'
          criterionType: 'StaticThresholdCriterion'
          metricName: 'availabilityResults/availabilityPercentage'
          operator: 'LessThan'
          threshold: 90
          timeAggregation: 'Average'
        }
      ]
    }
  }
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

output appName string = appName
output appHostname string = appHostname
output healthUrl string = healthUrl
output keyVaultName string = kvName
output sqlServerName string = sqlServerName
output databaseName string = databaseName
output sqlHostname string = sqlHostname
output applicationInsightsName string = aiName