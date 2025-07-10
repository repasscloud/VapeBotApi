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

echo "✅ Database reset, history table created, and migrations applied."
