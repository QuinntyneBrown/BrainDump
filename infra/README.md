# Deployment

Every push to `main` deploys to Azure on the cheapest tiers (target: $0/month).

| Layer    | Service               | SKU            |
| -------- | --------------------- | -------------- |
| Frontend | Static Web Apps       | Free           |
| Backend  | App Service (Linux)   | F1 (Free)      |
| Database | Azure SQL Database    | Free offer     |

The App Service uses a system-assigned managed identity to authenticate to Azure SQL — no passwords are stored anywhere.

## One-time setup

You only run this once per Azure subscription.

### 1. Sign in

```powershell
az login
az account set --subscription <your-subscription-id>
```

### 2. Run the setup script

PowerShell (Windows):

```powershell
./infra/azure-setup.ps1
```

Or bash (macOS / Linux / WSL):

```bash
./infra/azure-setup.sh
```

Override defaults if any names are taken:

```powershell
./infra/azure-setup.ps1 -WebAppName braindump-api-quinn -SqlServerName braindump-sql-quinn -StaticWebAppName braindump-web-quinn
```

The script writes `infra/outputs.json` with everything you need next. **Don't commit it** — it contains the publish profile and SWA token. It is already covered by `infra/.gitignore`.

### 3. Set GitHub repository secrets

In **Settings → Secrets and variables → Actions → Secrets**:

| Secret                            | Source (in `outputs.json`)            |
| --------------------------------- | ------------------------------------- |
| `AZURE_WEBAPP_PUBLISH_PROFILE`    | `AZURE_WEBAPP_PUBLISH_PROFILE`        |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | `AZURE_STATIC_WEB_APPS_API_TOKEN`     |

### 4. Set the GitHub repository variable

In **Settings → Secrets and variables → Actions → Variables**:

| Variable        | Value (from `outputs.json`) |
| --------------- | --------------------------- |
| `API_BASE_URL`  | `apiBaseUrl`                |

### 5. Push to main

```powershell
git push origin main
```

The two workflows (`deploy-backend.yml`, `deploy-frontend.yml`) run on push and `workflow_dispatch`. They are path-filtered so a backend-only change won't redeploy the frontend and vice versa.

## What the script provisions

In resource group `braindump-rg` (default), region `canadacentral`:

- **App Service Plan** `braindump-plan` — F1 Linux.
- **Web App** `braindump-api` — `DOTNETCORE:9.0`, system-assigned managed identity.
- **Static Web App** `braindump-web` — Free SKU.
- **Logical SQL Server** `braindump-sql` — Azure AD-only auth, the Web App's managed identity set as AAD admin.
- **Azure SQL Database** `BrainDump` — serverless General Purpose, 2 vCore, on the Free offer (`--use-free-limit AutoPause`).
- **Firewall rule** `AllowAllAzureServices` (start/end 0.0.0.0).
- **App Service connection string** `DefaultConnection` pointed at the SQL DB with `Authentication=Active Directory Default;`.
- **App Service settings**: `ASPNETCORE_ENVIRONMENT=Production`, `Jwt__UseLocalAuth=true`, and `Cors__AllowedOrigins__0=<frontend URL>`.

## Tradeoffs you accepted

- **F1 has no Always-On**: first hit after idle takes ~10–30s. Mitigation: scale to B1 (~$13/mo).
- **F1 has no SLA** and 60 CPU-min/day per app.
- **No staging slot on F1** — every commit lands in prod.
- **No custom-domain SSL** on F1. The SWA frontend gets free SSL on custom domains.
- **Free SQL offer** — 100,000 vCore-seconds/month per subscription; auto-pauses afterwards. App will get a 30-second cold-start on the first request after a pause.
- **`Database:EnsureCreatedOnStartup=true`** in `appsettings.json` bootstraps the schema on first deploy. Once you add EF migrations, replace the `EnsureCreatedAsync` call in `Program.cs` with `MigrateAsync` and flip the setting off.
- **`Jwt:UseLocalAuth=true`** is set in App Service config so prod can boot without a real Azure AD app registration. This is the dev-mode local auth path; replace with a real IdP before exposing the app to anyone but you.

## Re-running

The script is idempotent for resource creation but it overwrites:

- The Web App's CORS allow-list (`Cors__AllowedOrigins__0`)
- The connection string

Safe to re-run at any time. SQL data is preserved.

## Tearing it all down

```powershell
az group delete --name braindump-rg --yes --no-wait
```
