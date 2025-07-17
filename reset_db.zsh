#!/usr/bin/env zsh

set -euo pipefail

# 0) clean slate
rm -rf ./Migrations ./obj ./bin

# 1) regenerate migration
dotnet restore
dotnet build
dotnet ef migrations add initDb

# 2) restart Docker stack
docker compose down
docker compose up -d db

# 3) wait for Postgres to be ready
echo "⏳ Waiting for database to initialize…"
sleep 10

# 4) drop & re-create database
docker compose exec db psql -U postgres -c 'DROP DATABASE IF EXISTS "ShopBotDb";'
docker compose exec db psql -U postgres -c 'CREATE DATABASE "ShopBotDb" OWNER postgres;'
docker compose exec db psql -U postgres -c 'GRANT ALL PRIVILEGES ON DATABASE "ShopBotDb" TO postgres;'

# 5) pre-create EF history table to avoid the initial SELECT error
docker compose exec db psql -U postgres -d ShopBotDb -c '
  CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId"   TEXT    NOT NULL PRIMARY KEY,
    "ProductVersion" TEXT   NOT NULL
  );
'

# 6) pre-create serilog table
docker compose exec db psql -U postgres -d ShopBotDb -c '
  CREATE TABLE "logs" (
    id            SERIAL PRIMARY KEY,
    timestamp     TIMESTAMPTZ NOT NULL,
    level         TEXT         NOT NULL,
    message       TEXT         NOT NULL,
    exception     TEXT,
    properties    JSONB
  );
'

# 7) apply EF migration
docker compose run --rm cli dotnet-ef database update

# 8) run pgadmin
docker compose up -d pgadmin

# 9) run vapebotapi
docker compose up -d --build vapebotapi

# 10) wait a moment
sleep 10

# 11) ngrok
docker compose up -d ngrok
sleep 3

# 12) list of category names
typeset -a categories=(
  'Alibarbar Ingot 9K'
  'iGet Bar Pro 10K'
  'iGet Bar 3500'
  'Kuz Lux 9000'
  'Cigarettes'
)

for name in "${categories[@]}"; do
  # build JSON with variable expansion
  json=$(printf '{"categoryId":0,"name":"%s"}' "$name")

  curl -s -X POST \
    'https://secure-endlessly-puma.ngrok-free.app/api/admin/category' \
    -H 'Accept: */*' \
    -H 'Content-Type: application/json' \
    -d "$json"

  echo  # newline for readability
  sleep 0.5
done

# 13) import alibarbar ingot 9K csv
pwsh -File ./_data/process_alibarbar_ingot_9k.ps1

# 14) import shipping rates
curl -X 'POST' \
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

curl -X 'POST' \
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

# 14) rebuild locally
dotnet restore
dotnet build

echo "✅ Database reset, history table created, and migrations applied."

