# Manage App Registrations

This guide covers the three Copilot agent commands for managing Entra ID app registrations needed by MCP servers: creating an MCP account, creating an agent account, and setting reply URIs.

---

## Overview

Each MCP server deployment requires one app registration:

1. **MCP App Registration** (`mcp-{ServerName}`) — Protects the MCP server endpoint. Called by upstream clients (including Copilot agents, if directly integrated).

**Optional:** If you are integrating the MCP server with **Copilot or other clients that use APIM** and need an agent or client app to authenticate and call the MCP server with delegated permissions, you may also create:

2. **Agent App Registration** (`agent-{ServerName}`) — Used by AI agents or client applications to authenticate with delegated permissions to call the MCP server. Only required for Copilot Studio custom connectors or APIM-based scenarios.

Both registrations store their credentials in Azure Key Vault for secure retrieval at runtime.

---

## Prerequisites

| Requirement | Details |
|---|---|
| **Azure CLI** | Must be installed and authenticated (`az login`) |
| **Entra ID Role** | You must have one of: Application Developer, Application Administrator, or Global Administrator |
| **Key Vault** | Must exist (created by `/setup-deployment`); you must have Key Vault Secrets Officer or higher role |
| **Azure Subscription** | Where the Key Vault is deployed |

---

## Workflow

```
┌────────────────────────────────────┐
│  1. /setup-deployment              │  (Provision shared infrastructure)
│     Creates Key Vault              │
└────────────┬───────────────────────┘
             │
             ▼
┌────────────────────────────────────┐
│  2. /create-mcp-account            │  (Required)
│     - Creates app for MCP server   │
│     - Exposes API scope            │
│     - Stores credentials in KV     │
└────────────┬───────────────────────┘
             │
             ▼
┌────────────────────────────────────┐
│  3. /create-agent-account          │  (Optional: Copilot/APIM only)
│     - Creates app for agent/client │
│     - Adds delegated permissions   │
│     - Stores credentials in KV     │
└────────────┬───────────────────────┘
             │
             ▼
┌────────────────────────────────────┐
│  4. /set-reply-uri (optional)      │  (Optional: web clients with OAuth redirects)
│     - For web client apps          │
│     - Adds web reply URI           │
│     - Idempotent (no-op if exists) │
└────────────────────────────────────┘
             │
             ▼
┌────────────────────────────────────┐
│  5. /new-mcp-server                │  (Scaffold server & deploy)
│     - Creates server project       │
│     - Configures bicepparam        │
│     - References KV secrets        │
└────────────────────────────────────┘
```

---

## Step 1: Create MCP Account

Run:

```
/create-mcp-account
```

### What it does

- Creates an app registration named `mcp-{ServerName}`
- Exposes an API scope `api://{AppId}/mcp.tools` for delegated access
- Adds Microsoft Graph delegated permissions (`User.Read`, `offline_access`, `profile`) and grants admin consent
- Creates a 10-year client secret
- Stores four secrets in Key Vault:
  - `mcp-{ServerName}` — JSON payload with all credentials
  - `{ServerName}ClientId` — Application ID
  - `{ServerName}ClientSecret` — Client secret value
  - `{ServerName}TenantId` — Tenant ID

### Inputs

| Input | Description | Example |
|---|---|---|
| **ServerName** | Name of the MCP server | `WeatherForecast` |
| **KeyVaultName** | Name of the Azure Key Vault | `mymcpenv` |
| **SubscriptionId** | Azure subscription containing the Key Vault | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |

### Step-by-step walkthrough

**Step 0 — Run prerequisite and access checks**

Runs `scripts/Validate-ProvisioningEnvironment.ps1` (no flags — includes Key Vault check). Verifies that Azure CLI is authenticated, you have the required Entra ID role, and at least one Key Vault is accessible in the current subscription.

**Step 1 — Collect inputs**

The agent asks for `ServerName`, `KeyVaultName`, and prompts you to confirm or override the active Azure subscription.

**Step 2 — Create or update the app registration**

Runs:

```
scripts/New-EntraAppRegistration.ps1 -ServerName "{ServerName}" -KeyVaultName "{KeyVaultName}" -SubscriptionId "{SubscriptionId}" -AccountType mcp
```

**Step 3 — Print completion checklist**

Prints all created resources and Key Vault secret names. Does not print secret values.

### Example output

```
✅ App registration 'mcp-WeatherForecast' configured
✅ Enterprise Application for '12345678-1234-1234-1234-123456789012' verified
✅ Application ID URI: api://12345678-1234-1234-1234-123456789012
✅ API scope api://12345678-1234-1234-1234-123456789012/mcp.tools exposed
✅ Microsoft Graph delegated permissions added (User.Read, offline_access, profile)
✅ Admin consent granted
✅ Client secret created (10-year expiry)
✅ Key Vault secrets stored in mymcpenv

Key Vault secrets created:
- mcp-WeatherForecast
- WeatherForecastClientId
- WeatherForecastClientSecret
- WeatherForecastTenantId
```

### Idempotent behavior

- If an app registration with the name `mcp-{ServerName}` already exists, it will be reused
- Existing Key Vault secrets will be overwritten with the new credentials
- Running the command again is safe and will not create duplicate registrations

---

## Step 2: Create Agent Account (Optional)

Run:

```
/create-agent-account
```

**Note:** This step is **optional** and only required if you are integrating the MCP server with **Copilot Studio custom connectors or APIM-based clients** that need an agent or client application to authenticate with delegated permissions to call the MCP server. If your MCP server is called directly (e.g., by a backend service), skip this step.

### What it does

- Creates an app registration named `agent-{ServerName}`
- Adds three delegated permissions from Microsoft Graph:
  - `User.Read` — Read user profile
  - `offline_access` — Refresh token access
  - `profile` — Read user's email and profile
- Adds delegated permission to the MCP app's exposed scope `api://{McpAppId}/mcp.tools`
- Creates a 10-year client secret
- Stores one secret in Key Vault:
  - `agent-{ServerName}` — JSON payload with ApplicationId, ClientSecret, TenantId

### Inputs

| Input | Description | Example |
|---|---|---|
| **ServerName** | Same as used in `/create-mcp-account` | `WeatherForecast` |
| **McpAppId** | Application ID from the MCP app registration | `12345678-1234-1234-1234-123456789012` |
| **KeyVaultName** | Same as used in `/create-mcp-account` | `mymcpenv` |
| **SubscriptionId** | Azure subscription containing the Key Vault | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |

### Step-by-step walkthrough

**Step 0 — Run prerequisite and access checks**

Runs `scripts/Validate-ProvisioningEnvironment.ps1` (no flags — includes Key Vault check). Verifies that Azure CLI is authenticated, you have the required Entra ID role, and at least one Key Vault is accessible in the current subscription.

**Step 1 — Collect inputs**

The agent asks for `ServerName`, `McpAppId` (the Application ID from `/create-mcp-account`), `KeyVaultName`, and prompts you to confirm or override the active Azure subscription.

**Step 2 — Create or update the agent app registration**

Runs:

```
scripts/New-EntraAppRegistration.ps1 -ServerName "{ServerName}" -McpAppId "{McpAppId}" -KeyVaultName "{KeyVaultName}" -SubscriptionId "{SubscriptionId}" -AccountType agent
```

**Step 3 — Print completion checklist**

Prints the agent app registration details and the Key Vault secret name. Does not print secret values.

### Example output

```
✅ App registration 'agent-WeatherForecast' configured
✅ Enterprise Application for '87654321-4321-4321-4321-210987654321' verified
✅ Description set
✅ Microsoft Graph delegated permissions added (User.Read, offline_access, profile)
✅ Delegated permission granted to MCP app scope api://12345678-1234-1234-1234-123456789012/mcp.tools
✅ Client secret created (10-year expiry)
✅ Key Vault secret stored in mymcpenv

Key Vault secret created: agent-WeatherForecast
```

### Idempotent behavior

- If an app registration with the name `agent-{ServerName}` already exists, it will be reused
- Existing Key Vault secrets will be overwritten with the new credentials
- Running the command again is safe and will not create duplicate registrations

### Consent and permissions

When users first authenticate to the agent app, they will see a consent prompt requesting permission to:
- Read your profile (User.Read)
- Maintain access to data you have given it access to (offline_access)
- View your basic profile (profile)
- Access the MCP server on your behalf (api://{McpAppId}/mcp.tools)

A tenant administrator can pre-grant admin consent in the Azure Portal to skip the user consent prompt:
1. Go to **Entra ID > App registrations > agent-{ServerName} > API permissions**
2. Click **Grant admin consent for {TenantName}**

---

## Step 3: Set Reply URI (Optional — Web Clients Only)

Run (only if the agent is a **web client application** using OAuth 2.0 redirects):

```
/set-reply-uri
```

**Note:** This step is only required for web applications that use OAuth 2.0 authentication flows with client-side redirects (e.g., single-page applications, web portals). It is not needed for backend services, desktop applications, or server-to-server authentication scenarios.

### What it does

- Adds a web reply URI (OAuth 2.0 redirect URI) to an app registration
- Looks up the app by display name; if multiple registrations share the same name, prompts you to choose the correct one
- Idempotent: no-op if the URI already exists
- Does not write secrets to Key Vault

### Inputs

| Input | Description | Example |
|---|---|---|
| **AppDisplayName** | Display name of the app registration | `agent-WeatherForecast` or `mcp-PartnerCenter` |
| **ReplyUri** | HTTPS web redirect URI | `https://myapp.azurewebsites.net/auth/callback` |

### Step-by-step walkthrough

**Step 0 — Run prerequisite and access checks**

Runs `scripts/Validate-ProvisioningEnvironment.ps1` (no flags — includes Key Vault check). Verifies that Azure CLI is authenticated and you have the required Entra ID role.

**Step 1 — Collect inputs and look up the app**

The agent asks for `AppDisplayName` and `ReplyUri` (must start with `https://`). It then searches for matching app registrations by display name using `az ad app list`:

- **Exactly one match** — proceeds automatically
- **Multiple matches** — prompts you to select the correct one
- **No matches** — stops with an error

**Step 2 — Add the reply URI**

Runs:

```
scripts/Set-ReplyUri.ps1 -AppId "{AppId}" -ReplyUri "{ReplyUri}"
```

The script is idempotent — if the URI already exists it reports it without making any changes.

**Step 3 — Print completion**

Prints the full updated list of web reply URIs on the app registration.

### Example output

```
✅ Reply URI added to app registration 'agent-WeatherForecast' (87654321-4321-4321-4321-210987654321)

Web reply URIs:
  - https://myapp.azurewebsites.net/auth/callback
  - https://localhost:3000/auth/callback

Next step: Users will now be redirected to 'https://myapp.azurewebsites.net/auth/callback' when they complete OAuth 2.0 authentication.
```

### Idempotent behavior

- If the URI already exists, the command prints a message and stops (no changes)
- Safe to run multiple times

---

## Key Vault Secret Names and Formats

All secrets are stored in the Key Vault with exact casing as shown. Application code and Bicep parameter files must reference these names exactly.

### MCP Account Secrets

| Secret Name | Description | Format |
|---|---|---|
| `mcp-{ServerName}` | Complete credentials | `{"ApplicationId":"{appId}","ClientSecret":"{secret}","TenantId":"{tenantId}"}` |
| `{ServerName}ClientId` | Application ID only | `{appId}` (GUID) |
| `{ServerName}ClientSecret` | Client secret value only | `{secret}` (string) |
| `{ServerName}TenantId` | Tenant ID only | `{tenantId}` (GUID) |

**Example for `WeatherForecast` server:**
- `mcp-WeatherForecast`
- `WeatherForecastClientId`
- `WeatherForecastClientSecret`
- `WeatherForecastTenantId`

### Agent Account Secrets

| Secret Name | Description | Format |
|---|---|---|
| `agent-{ServerName}` | Complete credentials | `{"ApplicationId":"{appId}","ClientSecret":"{secret}","TenantId":"{tenantId}"}` |

**Example for `WeatherForecast` agent:**
- `agent-WeatherForecast`

---

## Troubleshooting

### "You do not have Application Developer, Application Administrator, or Global Administrator role"

**Problem:** The role check failed.

**Solution:**
1. Verify your Entra ID roles in the [Azure Portal](https://portal.azure.com) under **Entra ID > Users > {YourName} > Assigned roles**
2. Contact your Azure AD administrator to assign one of the required roles
3. If recently assigned, sign out and sign back in: `az logout && az login`

### "Key Vault '{KeyVaultName}' not found or you do not have access"

**Problem:** Key Vault is missing or inaccessible.

**Solution:**
1. Verify the Key Vault name matches the environment name from `/setup-deployment`
2. Verify you have Key Vault Secrets Officer or higher role on the Key Vault
3. If using a different subscription, verify you set the correct SubscriptionId

### "App registration '{AppDisplayName}' not found"

**Problem:** The app registration does not exist or the name is wrong.

**Solution (for `/set-reply-uri`):**
1. Verify the display name is correct — it must match exactly (e.g. `agent-WeatherForecast`)
2. Check the [Azure Portal](https://portal.azure.com) under **Entra ID > App registrations** to confirm the exact display name

### "Insufficient privileges to complete operation"

**Problem:** The role check passed but a subsequent operation failed due to insufficient permissions.

**Possible causes:**
- You do not have permission to grant admin consent to Microsoft Graph permissions
- You do not have permission to add API scopes to the app
- You do not have permission to update the app registration in this tenant

**Solution:**
1. Contact your Azure AD administrator to complete the step in the Azure Portal
2. Or ask them to grant you higher permissions (Application Administrator or Global Administrator)

### "PrincipalNotFound" when creating a secret

**Problem:** Role assignment or app registration creation propagated slowly in Entra ID.

**Solution:**
1. Wait 30–60 seconds
2. Run the command again

### Secrets are in the wrong Key Vault

**Problem:** You created secrets in the wrong Key Vault.

**Solution:**
1. Delete the incorrect secrets in the old Key Vault
2. Run the `/create-mcp-account` or `/create-agent-account` command again with the correct Key Vault name

### Users see a consent prompt every time

**Problem:** Admin consent was not granted, or the delegated permissions were updated.

**Solution:**
1. Have a tenant administrator grant admin consent in the Azure Portal:
   - Go to **Entra ID > App registrations > {AppName} > API permissions**
   - Click **Grant admin consent for {TenantName}**
2. If permissions were recently updated, cached consent may need to be cleared (this happens automatically over time)

---

## Next Steps

After creating the app registrations and setting reply URIs:

1. **For MCP servers:** Run `/new-mcp-server` to scaffold the server code and Bicep deployment, which will reference the Key Vault secrets automatically
2. **For agents or client apps:** Use the agent app credentials in your authentication code to obtain access tokens
3. **For Copilot Studio connectors:** Register the MCP server as a custom connector using the OpenAPI definition and the MCP app credentials
