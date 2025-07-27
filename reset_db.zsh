#!/usr/bin/env zsh

set -euo pipefail

# API endpoint
API_URL="https://secure-endlessly-puma.ngrok-free.app/api/Payments/nowpayments/external/currencies"

# Target directory
ICON_DIR="./wwwroot/img/nowpaymentsio"

# Create directory if it doesn't exist
mkdir -p "$ICON_DIR"

echo
echo "🧹 0) Cleaning slate: removing Migrations, obj, and bin directories"
rm -rf ./Migrations ./obj ./bin

echo
echo "⚙️  1) Regenerating EF migration"
dotnet restore
dotnet build
dotnet ef migrations add initDb

echo
echo "🐳 2) Restarting Docker stack (db only)"
docker compose down
docker compose up -d db

echo
echo "⏳ 3) Waiting for database to initialize…"
sleep 10

echo
echo "❌ 4) Dropping existing ShopBotDb and recreating it"
docker compose exec db psql -U postgres -c 'DROP DATABASE IF EXISTS "ShopBotDb";'
docker compose exec db psql -U postgres -c 'CREATE DATABASE "ShopBotDb" OWNER postgres;'
docker compose exec db psql -U postgres -c 'GRANT ALL PRIVILEGES ON DATABASE "ShopBotDb" TO postgres;'

echo
echo "📜 5) Pre-creating EF Migrations History table"
docker compose exec db psql -U postgres -d ShopBotDb -c '
  CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId"   TEXT    NOT NULL PRIMARY KEY,
    "ProductVersion" TEXT   NOT NULL
  );
'

echo
echo "📜 6) Pre-creating Serilog logs table"
docker compose exec db psql -U postgres -d ShopBotDb -c '
  CREATE TABLE public.logs (
    id               SERIAL        PRIMARY KEY,
    message          TEXT          NOT NULL,
    message_template TEXT,
    level            VARCHAR(128)  NOT NULL,
    timestamp        TIMESTAMPTZ   NOT NULL,
    exception        TEXT,
    log_event        JSONB
  );
'

echo
echo "🔄 7) Applying EF migrations to the database"
docker compose run --rm cli dotnet-ef database update

echo
echo "📊 8) Starting pgAdmin service"
docker compose up -d pgadmin

echo
echo "🚀 9) Building and starting VapeBotApi service"
docker compose up -d --build vapebotapi

echo
echo "⏲️  10) Waiting a moment for services to settle…"
sleep 10

echo
echo "🌐  11) Starting ngrok tunnel"
docker compose up -d ngrok
sleep 3

echo
echo "🗂️  12) Seeding category names via API"
typeset -a categories=(
  'Alibarbar Ingot 9K'
  'iGet Bar Pro 10K'
  'iGet Bar 3500'
  'Kuz Lux 9000'
  'Cigarettes'
)
for name in "${categories[@]}"; do
  echo "   • $name"
  json=$(printf '{"categoryId":0,"name":"%s"}' "$name")
  curl -s -X POST \
    'https://secure-endlessly-puma.ngrok-free.app/api/admin/category' \
    -H 'Accept: */*' \
    -H 'Content-Type: application/json' \
    -d "$json"
  echo
  sleep 0.5
done

echo
echo "📥  13) Importing Alibarbar Ingot 9K CSV data"
pwsh -File ./_data/process_alibarbar_ingot_9k.ps1

echo
echo "🚚  14) Importing shipping rates via API"
curl -X POST \
  'https://secure-endlessly-puma.ngrok-free.app/api/admin/shippingquote' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "id": 0,
  "serviceName": "ParcelPost",
  "maxItems": 9999,
  "capacity": 23,
  "rate": 21
}'
echo
curl -X POST \
  'https://secure-endlessly-puma.ngrok-free.app/api/admin/shippingquote' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "id": 0,
  "serviceName": "ExpressPost",
  "maxItems": 9999,
  "capacity": 23,
  "rate": 26
}'

echo
echo "🔨  15) Rebuilding the .NET project"
rm -rf ./bin
rm -rf ./obj
dotnet clean
dotnet restore
dotnet build

echo
echo "🔬 16) Running API tests via PowerShell script"
pwsh -File ./_data/test_api.ps1
echo "✅  API tests complete"

echo
echo "🐙 17) Running Git automation"
git add .
git commit -m "Automated commit"
git push origin dev
echo "✅  Git automation complete"

echo
echo "📡 18) Fetching NowPayments currency list..."
RESPONSE=$(curl -s -X 'GET' "$API_URL" -H 'accept: */*')
echo "✅  Currency list fetched"

echo
echo "🖼️  19) Downloading coin icons from NowPayments..."
echo "$RESPONSE" | jq -r '.currencies[].currency' | while read -r CODE; do
    LOWER=$(echo "$CODE" | tr '[:upper:]' '[:lower:]')
    FILE_NAME="${LOWER}.svg"
    FILE_PATH="${ICON_DIR}/${FILE_NAME}"
    IMAGE_URL="https://nowpayments.io/images/coins/${FILE_NAME}"

    echo "🌐  Downloading: $IMAGE_URL"
    curl -s -f "$IMAGE_URL" -o "$FILE_PATH"

    if [[ $? -eq 0 ]]; then
        echo "✅  Saved → $FILE_PATH"
    else
        echo "❌  Failed: $IMAGE_URL"
        rm -f "$FILE_PATH"
    fi
done
echo "🏁  All icons processed"

echo
echo "✅ All done: database reset, migrations applied, tables created, and services started!"
