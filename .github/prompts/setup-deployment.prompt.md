---
mode: agent
description: Provision the shared MCP environment and store credentials as GitHub Actions secrets. Triggered by "/setup-deployment".
---

# Setup Deployment

You are provisioning the shared MCP infrastructure for this repository and wiring it to GitHub Actions. Follow EVERY step exactly. Do NOT skip steps or reorder them.

---

## Step 1 — Collect inputs

Call the `vscode_askQuestions` tool with exactly these three questions:

```json
{
  "questions": [
    {
      "header": "EnvironmentName",
      "question": "Short name for this deployment ring (e.g. mymcpenv). Rules: 5–22 characters, lowercase letters and digits only, no hyphens or underscores. This name is used directly as the ACR name, Key Vault name, Container Apps environment name, and Log Analytics name. A 'st' prefix is added for the Storage Account name, so the effective Storage Account name will be 'st{EnvironmentName}' (max 24 chars).",
      "allowFreeformInput": true
    },
    {
      "header": "Location",
      "question": "Azure region to deploy into.",
      "allowFreeformInput": true,
      "options": [
        { "label": "westeurope", "recommended": true },
        { "label": "northeurope" },
        { "label": "eastus" },
        { "label": "eastus2" },
        { "label": "swedencentral" }
      ]
    },
    {
      "header": "SubscriptionId",
      "question": "Azure subscription ID (GUID) to deploy resources into.",
      "allowFreeformInput": true
    }
  ]
}
```

Validate the inputs:
- **EnvironmentName** must match `^[a-z0-9]{5,22}$`. If it does not, stop and ask the user to correct it.
- **SubscriptionId** must be a valid GUID (`xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`). If it does not look like a GUID, stop and ask the user to correct it.

Derive the remaining resource names:

| Parameter | Value |
|---|---|
| `acrName` | `{EnvironmentName}` |
| `containerAppsEnvName` | `{EnvironmentName}` |
| `keyVaultName` | `{EnvironmentName}` |
| `logAnalyticsName` | `{EnvironmentName}` |
| `storageAccountName` | `st{EnvironmentName}` |
| `resourceGroupName` | `rg-{EnvironmentName}` |

Echo the resolved values before proceeding:
> Provisioning **{EnvironmentName}** in **{Location}** — subscription `{SubscriptionId}`

---

## Step 2 — Verify prerequisites

Run `az version` and `gh --version`. If either command is not found, stop and tell the user which tool to install with a link:
- Azure CLI: https://learn.microsoft.com/cli/azure/install-azure-cli
- GitHub CLI: https://cli.github.com/

Verify an active Azure login by running:

```
az account show --query "name" -o tsv
```

If the command fails or returns nothing, stop and tell the user to run `az login` first.

Verify the GitHub CLI is authenticated by running:

```
gh auth status
```

If not authenticated, stop and tell the user to run `gh auth login` first.

---

## Step 3 — Set active subscription

```
az account set --subscription {SubscriptionId}
```

---

## Step 4 — Update Infrastructure/dev.bicepparam

Read `Infrastructure/dev.bicepparam`. Replace the entire file contents with the following, substituting the resolved values:

```bicep
using 'main.bicep'

param acrName             = '{EnvironmentName}'
param containerAppsEnvName = '{EnvironmentName}'
param keyVaultName        = '{EnvironmentName}'
param logAnalyticsName    = '{EnvironmentName}'
param location            = '{Location}'
param resourceGroupName   = 'rg-{EnvironmentName}'
param storageAccountName  = 'st{EnvironmentName}'
```

---

## Step 5 — Create service principal

Run the following command and **capture the JSON output into a variable**. Do NOT print the JSON to the terminal or display it in the chat — it contains the client secret.

```
az ad sp create-for-rbac --name "sp-mcp-{EnvironmentName}" --json-auth --output json
```

Store the complete JSON output in a shell variable named `SP_JSON`. Do not write it to disk.

> **Note:** If your Azure CLI version does not support `--json-auth`, use `--sdk-auth` instead — it produces the same output.

---

## Step 6 — Assign Owner role to service principal

Extract the `clientId` field from `SP_JSON` and assign the `Owner` role to the service principal at subscription scope. This allows the GitHub Actions workflow to create role assignments for the MCP servers:

```
az role assignment create --assignee {clientId from SP_JSON} --role Owner --scope /subscriptions/{SubscriptionId}
```

The Bicep template will reference the deployment identity inline to assign the `AcrPush` role during the GitHub Actions deployment.

---

## Step 7 — Store credentials in GitHub

Pipe `SP_JSON` directly to the GitHub CLI without writing to disk or displaying in the terminal. This keeps the client secret entirely out of terminal history and the file system:

```
echo {SP_JSON} | gh secret set AZURE_CREDENTIALS --app actions
```

Set the ACR name as GitHub Actions repository variables for both environments (both pointing to the same registry — split them into separate environments later if needed):

```
gh variable set ACR_NAME_DEV --body "{EnvironmentName}"
gh variable set ACR_NAME_PROD --body "{EnvironmentName}"
```

Clear `SP_JSON` from the shell variable after use.

---

## Step 8 — Copy workflow templates to workflows folder

Copy the three GitHub Actions workflow templates from `.github/templates/` to `.github/workflows/`:

```
cp .github/templates/deploy-bicep.yml .github/workflows/
cp .github/templates/docker-deploy-containerapp-template.yml .github/workflows/
cp .github/templates/docker-publish-template.yml .github/workflows/
```

---

## Step 9 — Print completion checklist

Print a checklist of every action completed. Mark each item ✅:

```
✅ Infrastructure/dev.bicepparam updated
✅ Service principal sp-mcp-{EnvironmentName} created
✅ Owner role assigned to SP on subscription {SubscriptionId}
✅ AZURE_CREDENTIALS secret set in GitHub Actions
✅ ACR_NAME_DEV variable set to {EnvironmentName}
✅ ACR_NAME_PROD variable set to {EnvironmentName}
✅ Workflow templates copied to .github/workflows/
```

---

## Step 10 — Commit and push to trigger deployment

Review your changes:

```
git status
```

You should see `Infrastructure/dev.bicepparam` modified and the three workflow files in `.github/workflows/`. Commit and push the changes:

```
git add Infrastructure/dev.bicepparam .github/workflows/
git commit -m "Configure MCP environment: {EnvironmentName}"
git push
```

This will trigger the **Deploy Bicep Template** workflow in GitHub Actions. The workflow will:
1. Deploy the shared infrastructure to Azure (ACR, Container Apps Environment, Key Vault, Log Analytics, etc.)

Monitor the workflow in the **Actions** tab of your repository.

Then print:
> **Next step:** Once the GitHub Actions workflow completes successfully, run **/new-mcp-server** to scaffold your first MCP server.
