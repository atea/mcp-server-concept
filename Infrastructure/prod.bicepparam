using 'main.bicep'

// Shared registry — keep acrName and acrResourceGroupName identical to dev.bicepparam
param acrName             = 'mymcpacrthbm'
param acrResourceGroupName = 'rg-mymcpacrthbm'

// Prod-environment resources
param containerAppsEnvName = 'mymcpprodthbm'
param keyVaultName        = 'mymcpprodthbm'
param logAnalyticsName    = 'mymcpprodthbm'
param location            = 'swedencentral'
param resourceGroupName   = 'rg-mymcpprodthbm'
param storageAccountName  = 'stmymcpprodthbm'
