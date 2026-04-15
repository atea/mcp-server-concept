using 'containerApp.bicep'

param location = 'westeurope'
param imageName = 'fbimostwanted'
param appName = 'fbimostwanted'
param environmentName = 'axmcpatea'
param resourceGroupName = 'rg-axmcpatea'
param keyVaultSecrets = [
  {
    key: 'fbimostwantedclientid' // Must be lowercase - used in secretRef
    value: 'FBIMostWantedClientId' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'fbimostwantedclientsecret' // Must be lowercase - used in secretRef
    value: 'FBIMostWantedClientSecret' // PascalCase - actual Key Vault secret name
  }
  {
    key: 'fbimostwantedtenantid' // Must be lowercase - used in secretRef
    value: 'FBIMostWantedTenantId' // PascalCase - actual Key Vault secret name
  }
]
param environment = [
  {
    name: 'EntraIdAuth__TenantId'
    secretRef: 'fbimostwantedtenantid'
  }
  {
    name: 'EntraIdAuth__ClientId'
    secretRef: 'fbimostwantedclientid'
  }
  {
    name: 'EntraIdAuth__ClientSecret'
    secretRef: 'fbimostwantedclientsecret'
  }
  {
    name: 'EntraIdAuth__PublicUrl'
    value: 'fbimostwanted.wonderfulriver-e85e8efa.westeurope.azurecontainerapps.io'
  }
  {
    name: 'FBIMostWantedApi__BaseUrl'
    value: 'https://api.fbi.gov'
  }
  {
    name: 'IsTransportStateless'
    value: 'true'
  }
  // Application Insights connection string is automatically added by the template
]
