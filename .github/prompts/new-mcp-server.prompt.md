---
mode: agent
description: Scaffold a new MCP Server in this repository. Triggered by "/new-mcp-server".
---

# New MCP Server Scaffold

You are scaffolding a new MCP server. Follow EVERY step exactly. Do NOT generate file content from memory — always read the template file first, then write the output file with substitutions applied.

---

## Step 1 — Collect inputs

Call the `vscode_askQuestions` tool with exactly these four questions:

```json
{
  "questions": [
    {
      "header": "ServerName",
      "question": "PascalCase name of the new MCP server (e.g. WeatherForecast, PartnerCenter). Used for the folder, namespace, class names, and .csproj.",
      "allowFreeformInput": true
    },
    {
      "header": "AuthType",
      "question": "How does this server authenticate against its upstream API?",
      "allowFreeformInput": false,
      "options": [
        {
          "label": "apikey",
          "description": "Upstream API uses a static API key (sent as header or query param)",
          "recommended": true
        },
        {
          "label": "entraid",
          "description": "Upstream API uses Entra ID — OBO token exchange on behalf of the signed-in user"
        }
      ]
    },
    {
      "header": "ApiConfigSection",
      "question": "Configuration section prefix for the upstream API settings (only used when AuthType = apikey). Leave blank to auto-derive as {ServerName}Api, e.g. WeatherForecastApi.",
      "allowFreeformInput": true
    },
    {
      "header": "Port",
      "question": "HTTP port the container listens on inside Azure Container Apps. Leave blank to use the default 4547.",
      "allowFreeformInput": true
    }
  ]
}
```

Once the user answers:
- If **Port** is blank or `4547`, use `4547`.
- If **AuthType** is `entraid`, ignore **ApiConfigSection**.
- If **ApiConfigSection** is `{ServerName}Api`, replace `{ServerName}` with the actual server name (e.g. `WeatherForecastApi`).

Derive `{{servername}}` = ServerName in all-lowercase (e.g. `WeatherForecast` → `weatherforecast`).

Echo the resolved values in one line before proceeding:
> Scaffolding **{ServerName}** | auth: **{AuthType}** | port: **{Port}**{optional: | config section: {ApiConfigSection}}

---

## Step 2 — Verify prerequisites

Check that `MCPServers/Shared/Shared.csproj` exists in the workspace.

If it does NOT exist, stop and say:
> "The Shared project was not found at `MCPServers/Shared/Shared.csproj`. Make sure you have cloned the full repository before scaffolding a new server."

---

## Step 3 — Create files from templates

The template files live in `.github/templates/mcp-server/`. For each output file below:
1. **Read** the specified template file.
2. **Replace** all occurrences of each placeholder with the resolved value.
3. **Write** the file to the target path.

**Placeholder substitution table:**

| Placeholder | Replace with |
|---|---|
| `{{ServerName}}` | The PascalCase server name provided by the user |
| `{{servername}}` | The lowercase server name |
| `{{Port}}` | The port number |
| `{{ApiConfigSection}}` | The config section name (API Key mode only) |

### Files to create

#### C# project

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/Server.csproj` | `MCPServers/{{ServerName}}/{{ServerName}}.csproj` |

If AuthType = `apikey`:

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/Program.ApiKey.cs` | `MCPServers/{{ServerName}}/Program.cs` |
| `.github/templates/mcp-server/Tool.ApiKey.cs` | `MCPServers/{{ServerName}}/Tools/{{ServerName}}Tool.cs` |
| `.github/templates/mcp-server/Service.ApiKey.cs` | `MCPServers/{{ServerName}}/Services/{{ServerName}}Service.cs` |
| `.github/templates/mcp-server/appsettings.ApiKey.json` | `MCPServers/{{ServerName}}/appsettings.json` |

If AuthType = `entraid`:

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/Program.EntraId.cs` | `MCPServers/{{ServerName}}/Program.cs` |
| `.github/templates/mcp-server/Tool.EntraId.cs` | `MCPServers/{{ServerName}}/Tools/{{ServerName}}Tool.cs` |
| `.github/templates/mcp-server/Service.EntraId.cs` | `MCPServers/{{ServerName}}/Services/{{ServerName}}Service.cs` |
| `.github/templates/mcp-server/appsettings.EntraId.json` | `MCPServers/{{ServerName}}/appsettings.json` |

Always create for both auth types:

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/appsettings.Development.json` | `MCPServers/{{ServerName}}/appsettings.Development.json` |
| `.github/templates/mcp-server/Dockerfile` | `MCPServers/{{ServerName}}/Dockerfile` |

#### Infrastructure

If AuthType = `apikey`:

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/bicepparam.ApiKey.bicepparam` | `Infrastructure/containerApp-{{ServerName}}.bicepparam` |

If AuthType = `entraid`:

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/bicepparam.EntraId.bicepparam` | `Infrastructure/containerApp-{{ServerName}}.bicepparam` |

#### CI/CD

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/workflow.yml` | `.github/workflows/docker-publish-{{servername}}.yml` |

#### Copilot Custom Connector

| Template | Target path |
|---|---|
| `.github/templates/mcp-server/swagger.json` | `Copilot/CustomConnectors/{{ServerName}}.swagger.json` |

---

## Step 4 — Add to solution

Run this command to register the new project in the solution:

```
dotnet sln MCPConcept.sln add MCPServers/{{ServerName}}/{{ServerName}}.csproj
```

---

## Step 5 — Update .vscode/tasks.json

Read `.vscode/tasks.json`. Add the following task object to the `tasks` array, then write the file back:

```json
{
  "label": "build {{ServerName}}",
  "command": "dotnet",
  "type": "process",
  "args": [
    "build",
    "${workspaceFolder}/MCPServers/{{ServerName}}/{{ServerName}}.csproj"
  ]
}
```

---

## Step 6 — Update .vscode/launch.json

Read `.vscode/launch.json`. Add the following configuration to the `configurations` array, then write the file back:

```json
{
  "name": "{{ServerName}}",
  "type": "dotnet",
  "request": "launch",
  "projectPath": "${workspaceFolder}/MCPServers/{{ServerName}}/{{ServerName}}.csproj",
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

---

## Step 7 — Print completion checklist

Print a checklist of every file created and every action taken. Mark each item ✅. Example:

```
✅ MCPServers/{{ServerName}}/{{ServerName}}.csproj
✅ MCPServers/{{ServerName}}/Program.cs
✅ MCPServers/{{ServerName}}/Tools/{{ServerName}}Tool.cs
✅ MCPServers/{{ServerName}}/Services/{{ServerName}}Service.cs
✅ MCPServers/{{ServerName}}/appsettings.json
✅ MCPServers/{{ServerName}}/appsettings.Development.json
✅ MCPServers/{{ServerName}}/Dockerfile
✅ Infrastructure/containerApp-{{ServerName}}.bicepparam
✅ .github/workflows/docker-publish-{{servername}}.yml
✅ Copilot/CustomConnectors/{{ServerName}}.swagger.json
✅ Added to MCPConcept.sln
✅ .vscode/tasks.json updated
✅ .vscode/launch.json updated
```

Then print the next steps:

> **Next steps:**
> 1. Open `MCPServers/{{ServerName}}/appsettings.json` and fill in all `TODO` values.
> 2. Open `MCPServers/{{ServerName}}/appsettings.Development.json` and fill in dev Entra ID credentials.
> 3. Implement `{{ServerName}}Service.GetDataAsync()` and add additional methods as needed.
> 4. Add real tool logic in `{{ServerName}}Tool.cs` — replace the `ExampleTool` stub.
> 5. Create Key Vault secrets listed in `Infrastructure/containerApp-{{ServerName}}.bicepparam`.
> 6. Update `EntraIdAuth__PublicUrl` and `TODO-public-url-after-first-deploy` after the first deploy.
> 7. Update the `servers` section in `Copilot/CustomConnectors/{{ServerName}}.swagger.json` with the real URL.
