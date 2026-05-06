# Azure setup for brain-dump (cheapest tier)
#
# Creates:
#   - Resource group (canadacentral)
#   - App Service Plan F1 Linux (free)
#   - Web App, .NET 9, system-assigned managed identity (free, on F1)
#   - Static Web App, Free tier (free)
#   - Logical SQL server with the Web App's managed identity as Azure AD admin
#   - Azure SQL DB on the Free offer (free up to monthly quota, auto-pauses afterwards)
#   - Connection string + JWT settings on the Web App
#   - SQL firewall rule allowing Azure services
#
# Idempotent: re-running with the same names is a no-op for existing resources.
#
# Prerequisites:
#   - Azure CLI logged in (`az login`)
#   - Subscription set (`az account set --subscription <id>`)
#   - The signed-in user must be able to set Azure AD admin on a SQL server
#
# Outputs (saved to ./infra/outputs.json) needed for GitHub secrets:
#   - AZURE_WEBAPP_PUBLISH_PROFILE
#   - AZURE_STATIC_WEB_APPS_API_TOKEN
#   - API_BASE_URL  (App Service URL)
#   - FRONTEND_BASE_URL  (Static Web App URL)

[CmdletBinding()]
param(
  [string]$Location          = "canadacentral",
  # SWA control plane is not in canadacentral; centralus is the closest region it offers.
  [string]$StaticWebAppLocation = "centralus",
  [string]$ResourceGroup     = "braindump-rg",
  [string]$AppServicePlan    = "braindump-plan",
  [string]$WebAppName        = "braindump-api",     # must be globally unique; override if taken
  [string]$StaticWebAppName  = "braindump-web",
  [string]$SqlServerName     = "braindump-sql",     # must be globally unique
  [string]$SqlDatabaseName   = "BrainDump"
)

$ErrorActionPreference = "Stop"

function Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

# -----------------------------------------------------------------------------
# 0. Sanity checks
# -----------------------------------------------------------------------------
Step "Checking Azure CLI login"
$account = az account show --output json 2>$null | ConvertFrom-Json
if (-not $account) { throw "Run 'az login' first." }
Write-Host "Subscription: $($account.name) ($($account.id))"

# -----------------------------------------------------------------------------
# 1. Resource group
# -----------------------------------------------------------------------------
Step "Resource group: $ResourceGroup"
az group create --name $ResourceGroup --location $Location --output none

# -----------------------------------------------------------------------------
# 2. App Service plan (F1 Linux)
# -----------------------------------------------------------------------------
Step "App Service plan: $AppServicePlan (F1 Linux)"
az appservice plan create `
  --name $AppServicePlan `
  --resource-group $ResourceGroup `
  --location $Location `
  --sku F1 `
  --is-linux `
  --output none

# -----------------------------------------------------------------------------
# 3. Web App (.NET 9 Linux) with system-assigned managed identity
# -----------------------------------------------------------------------------
Step "Web App: $WebAppName (.NET 9)"
az webapp create `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --plan $AppServicePlan `
  --runtime "DOTNETCORE:9.0" `
  --output none

Step "Enabling SCM basic auth (required by webapps-deploy@v3 publish-profile flow)"
az resource update `
  --resource-group $ResourceGroup `
  --name scm `
  --namespace Microsoft.Web `
  --resource-type basicPublishingCredentialsPolicies `
  --parent "sites/$WebAppName" `
  --set properties.allow=true `
  --output none

Step "Enabling system-assigned managed identity on Web App"
$identity = az webapp identity assign `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --output json | ConvertFrom-Json
$webAppPrincipalId = $identity.principalId
Write-Host "Web App principalId: $webAppPrincipalId"

# Don't auto-redirect to HTTPS at the platform level — F1 already terminates TLS.
# Keep WEBSITES_PORT default; .NET reads ASPNETCORE_URLS from App Service.
Step "Setting App Service config (production environment)"
az webapp config appsettings set `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --settings `
    ASPNETCORE_ENVIRONMENT=Production `
    Jwt__UseLocalAuth=true `
  --output none

# -----------------------------------------------------------------------------
# 4. SQL Server with the Web App MI as the AAD admin
# -----------------------------------------------------------------------------
Step "SQL Server: $SqlServerName"
# The signed-in user is set as initial AAD admin so we can assign the MI afterwards.
$signedInUser   = az ad signed-in-user show --output json | ConvertFrom-Json
$signedInUserId = $signedInUser.id
$signedInUpn    = $signedInUser.userPrincipalName

az sql server create `
  --name $SqlServerName `
  --resource-group $ResourceGroup `
  --location $Location `
  --enable-ad-only-auth `
  --external-admin-principal-type User `
  --external-admin-name $signedInUpn `
  --external-admin-sid $signedInUserId `
  --output none

Step "Allow Azure services to reach SQL Server"
az sql server firewall-rule create `
  --resource-group $ResourceGroup `
  --server $SqlServerName `
  --name AllowAllAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0 `
  --output none

# Replace the human admin with the Web App's managed identity so the API can
# authenticate to SQL with no password.
Step "Setting Web App MI as SQL Server AAD admin"
az sql server ad-admin create `
  --resource-group $ResourceGroup `
  --server-name $SqlServerName `
  --display-name $WebAppName `
  --object-id $webAppPrincipalId `
  --output none

# -----------------------------------------------------------------------------
# 5. Azure SQL Database on the Free offer (cheapest)
# -----------------------------------------------------------------------------
Step "Azure SQL DB: $SqlDatabaseName (Free offer, serverless GP)"
# --use-free-limit grants 100,000 vCore-seconds + 32 GB free per subscription.
# AutoPause keeps it free even after the monthly quota is exhausted.
az sql db create `
  --resource-group $ResourceGroup `
  --server $SqlServerName `
  --name $SqlDatabaseName `
  --use-free-limit true `
  --free-limit-exhaustion-behavior AutoPause `
  --edition GeneralPurpose `
  --family Gen5 `
  --capacity 2 `
  --compute-model Serverless `
  --backup-storage-redundancy Local `
  --output none

# -----------------------------------------------------------------------------
# 6. Connection string on the Web App (passwordless, MI auth)
# -----------------------------------------------------------------------------
# No `Authentication=...` clause: the API's UseAzureSqlAuthentication extension
# attaches an AccessTokenCallback via DefaultAzureCredential, and SqlClient
# rejects setting the callback when Authentication is also specified.
$connString = "Server=tcp:$SqlServerName.database.windows.net,1433;Database=$SqlDatabaseName;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
Step "Setting ConnectionStrings:DefaultConnection on Web App"
az webapp config connection-string set `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --connection-string-type SQLAzure `
  --settings DefaultConnection="$connString" `
  --output none

# -----------------------------------------------------------------------------
# 7. Static Web App (Free tier)
# -----------------------------------------------------------------------------
Step "Static Web App: $StaticWebAppName (Free, $StaticWebAppLocation)"
az staticwebapp create `
  --name $StaticWebAppName `
  --resource-group $ResourceGroup `
  --location $StaticWebAppLocation `
  --sku Free `
  --output none

# -----------------------------------------------------------------------------
# 8. Wire the SWA origin into the Web App's CORS allow-list
# -----------------------------------------------------------------------------
$swa = az staticwebapp show --name $StaticWebAppName --resource-group $ResourceGroup --output json | ConvertFrom-Json
$frontendUrl = "https://$($swa.defaultHostname)"
Step "Frontend URL: $frontendUrl"

az webapp config appsettings set `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --settings Cors__AllowedOrigins__0=$frontendUrl `
  --output none

# -----------------------------------------------------------------------------
# 9. Capture deploy secrets
# -----------------------------------------------------------------------------
Step "Capturing publish profile and SWA deployment token"
$publishProfile = az webapp deployment list-publishing-profiles `
  --name $WebAppName `
  --resource-group $ResourceGroup `
  --xml

$swaToken = az staticwebapp secrets list `
  --name $StaticWebAppName `
  --resource-group $ResourceGroup `
  --query properties.apiKey `
  --output tsv

$apiBaseUrl = "https://$WebAppName.azurewebsites.net"

$outputs = [ordered]@{
  resourceGroup                     = $ResourceGroup
  webApp                            = $WebAppName
  staticWebApp                      = $StaticWebAppName
  sqlServer                         = $SqlServerName
  sqlDatabase                       = $SqlDatabaseName
  apiBaseUrl                        = $apiBaseUrl
  frontendBaseUrl                   = $frontendUrl
  AZURE_WEBAPP_PUBLISH_PROFILE      = $publishProfile
  AZURE_STATIC_WEB_APPS_API_TOKEN   = $swaToken
}

$outputsPath = Join-Path $PSScriptRoot "outputs.json"
$outputs | ConvertTo-Json -Depth 5 | Set-Content -Path $outputsPath -Encoding UTF8

Write-Host ""
Write-Host "Done." -ForegroundColor Green
Write-Host "API:       $apiBaseUrl"
Write-Host "Frontend:  $frontendUrl"
Write-Host ""
Write-Host "Secrets written to: $outputsPath"
Write-Host "Set these GitHub repository secrets (Settings -> Secrets and variables -> Actions):"
Write-Host "  AZURE_WEBAPP_PUBLISH_PROFILE     <- value of AZURE_WEBAPP_PUBLISH_PROFILE in outputs.json"
Write-Host "  AZURE_STATIC_WEB_APPS_API_TOKEN  <- value of AZURE_STATIC_WEB_APPS_API_TOKEN in outputs.json"
Write-Host ""
Write-Host "And set this GitHub repository variable:"
Write-Host "  API_BASE_URL = $apiBaseUrl"
