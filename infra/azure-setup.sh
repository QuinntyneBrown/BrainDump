#!/usr/bin/env bash
# Azure setup for brain-dump (cheapest tier).
# See infra/azure-setup.ps1 for full documentation; this is a bash port.
set -euo pipefail

LOCATION="${LOCATION:-canadacentral}"
# SWA control plane is not in canadacentral; centralus is the closest region it offers.
SWA_LOCATION="${SWA_LOCATION:-centralus}"
RG="${RG:-braindump-rg}"
PLAN="${PLAN:-braindump-plan}"
WEBAPP="${WEBAPP:-braindump-api}"
SWA="${SWA:-braindump-web}"
SQL_SERVER="${SQL_SERVER:-braindump-sql}"
SQL_DB="${SQL_DB:-BrainDump}"

step() { printf "\n==> %s\n" "$1"; }

step "Checking Azure CLI login"
az account show --output none

step "Resource group: $RG"
az group create --name "$RG" --location "$LOCATION" --output none

step "App Service plan: $PLAN (F1 Linux)"
az appservice plan create \
  --name "$PLAN" --resource-group "$RG" --location "$LOCATION" \
  --sku F1 --is-linux --output none

step "Web App: $WEBAPP (.NET 9 Linux)"
az webapp create \
  --name "$WEBAPP" --resource-group "$RG" --plan "$PLAN" \
  --runtime "DOTNETCORE:9.0" --output none

step "Enabling SCM basic auth (required by webapps-deploy@v3 publish-profile flow)"
az resource update \
  --resource-group "$RG" \
  --name scm \
  --namespace Microsoft.Web \
  --resource-type basicPublishingCredentialsPolicies \
  --parent "sites/$WEBAPP" \
  --set properties.allow=true \
  --output none

step "Enabling system-assigned managed identity"
WEBAPP_PRINCIPAL_ID="$(az webapp identity assign \
  --name "$WEBAPP" --resource-group "$RG" \
  --query principalId --output tsv)"
echo "Web App principalId: $WEBAPP_PRINCIPAL_ID"

step "App Service configuration"
az webapp config appsettings set \
  --name "$WEBAPP" --resource-group "$RG" \
  --settings ASPNETCORE_ENVIRONMENT=Production Jwt__UseLocalAuth=true \
  --output none

step "SQL Server: $SQL_SERVER"
SIGNED_IN_OBJECT_ID="$(az ad signed-in-user show --query id --output tsv)"
SIGNED_IN_UPN="$(az ad signed-in-user show --query userPrincipalName --output tsv)"

az sql server create \
  --name "$SQL_SERVER" --resource-group "$RG" --location "$LOCATION" \
  --enable-ad-only-auth \
  --external-admin-principal-type User \
  --external-admin-name "$SIGNED_IN_UPN" \
  --external-admin-sid "$SIGNED_IN_OBJECT_ID" \
  --output none

step "Allow Azure services to reach SQL Server"
az sql server firewall-rule create \
  --resource-group "$RG" --server "$SQL_SERVER" \
  --name AllowAllAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 \
  --output none

step "Setting Web App MI as SQL Server AAD admin"
az sql server ad-admin create \
  --resource-group "$RG" --server-name "$SQL_SERVER" \
  --display-name "$WEBAPP" --object-id "$WEBAPP_PRINCIPAL_ID" \
  --output none

step "Azure SQL DB: $SQL_DB (Free offer, serverless GP)"
az sql db create \
  --resource-group "$RG" --server "$SQL_SERVER" --name "$SQL_DB" \
  --use-free-limit true --free-limit-exhaustion-behavior AutoPause \
  --edition GeneralPurpose --family Gen5 --capacity 2 \
  --compute-model Serverless --backup-storage-redundancy Local \
  --output none

CONN="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Database=${SQL_DB};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
step "ConnectionStrings:DefaultConnection on Web App"
az webapp config connection-string set \
  --name "$WEBAPP" --resource-group "$RG" \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONN" \
  --output none

step "Static Web App: $SWA (Free, $SWA_LOCATION)"
az staticwebapp create \
  --name "$SWA" --resource-group "$RG" --location "$SWA_LOCATION" \
  --sku Free --output none

FRONTEND_URL="https://$(az staticwebapp show --name "$SWA" --resource-group "$RG" --query defaultHostname --output tsv)"
echo "Frontend URL: $FRONTEND_URL"

step "Adding frontend origin to Web App CORS allow-list"
az webapp config appsettings set \
  --name "$WEBAPP" --resource-group "$RG" \
  --settings Cors__AllowedOrigins__0="$FRONTEND_URL" \
  --output none

step "Capturing deploy secrets"
PUBLISH_PROFILE="$(az webapp deployment list-publishing-profiles \
  --name "$WEBAPP" --resource-group "$RG" --xml)"
SWA_TOKEN="$(az staticwebapp secrets list \
  --name "$SWA" --resource-group "$RG" \
  --query properties.apiKey --output tsv)"
API_BASE_URL="https://${WEBAPP}.azurewebsites.net"

OUTPUTS_PATH="$(dirname "$0")/outputs.json"
cat > "$OUTPUTS_PATH" <<EOF
{
  "resourceGroup": "$RG",
  "webApp": "$WEBAPP",
  "staticWebApp": "$SWA",
  "sqlServer": "$SQL_SERVER",
  "sqlDatabase": "$SQL_DB",
  "apiBaseUrl": "$API_BASE_URL",
  "frontendBaseUrl": "$FRONTEND_URL",
  "AZURE_WEBAPP_PUBLISH_PROFILE": $(jq -Rs . <<<"$PUBLISH_PROFILE"),
  "AZURE_STATIC_WEB_APPS_API_TOKEN": "$SWA_TOKEN"
}
EOF

echo ""
echo "Done."
echo "API:       $API_BASE_URL"
echo "Frontend:  $FRONTEND_URL"
echo ""
echo "Secrets written to: $OUTPUTS_PATH"
echo "Set these GitHub repository secrets:"
echo "  AZURE_WEBAPP_PUBLISH_PROFILE"
echo "  AZURE_STATIC_WEB_APPS_API_TOKEN"
echo "And this repository variable:"
echo "  API_BASE_URL = $API_BASE_URL"
