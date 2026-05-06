using 'main.bicep'

// Shared registry — keep acrName and acrResourceGroupName identical in prod.bicepparam
param acrName             = 'mymcpacrthbm'
param acrResourceGroupName = 'rg-mymcpacrthbm'

// Dev-environment resources
param containerAppsEnvName = 'mymcpdevthbm'
param keyVaultName        = 'mymcpdevthbm'
param logAnalyticsName    = 'mymcpdevthbm'
param location            = 'swedencentral'
param resourceGroupName   = 'rg-mymcpdevthbm'
param storageAccountName  = 'stmymcpdevthbm'
