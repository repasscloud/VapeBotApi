#!/usr/bin/env zsh

set -euo pipefail

echo
echo "🐳 2) Restarting Docker stack (db only)"
docker compose down
docker compose up -d db

echo
echo "⏳ 3) Waiting for database to initialize…"
sleep 10

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
echo "🔨  15) Rebuilding the .NET project"
rm -rf ./bin
rm -rf ./obj
dotnet clean
dotnet restore
dotnet build

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
API_URL="https://secure-endlessly-puma.ngrok-free.app/api/Payments/nowpayments/external/currencies"
ICON_DIR="./wwwroot/img/nowpaymentsio"
LOGFILE="./nowpayments-download-errors.log"
# Ensure icon directory exists
mkdir -p "$ICON_DIR"
# Fetch currency list
echo "🌐  Fetching currency list from API..."
RESPONSE=$(curl -s -X GET "$API_URL" -H "accept: */*")
# Validate response
if [[ -z "$RESPONSE" ]]; then
    echo "❌  Failed to fetch currency list. RESPONSE is empty."
    exit 1
fi
# Iterate through currencies and download SVGs
echo "$RESPONSE" | jq -r '.currencies[].currency' | while IFS= read -r CODE; do
    LOWER="${CODE:l}"
    FILE_NAME="${LOWER}.svg"
    FILE_PATH="${ICON_DIR}/${FILE_NAME}"
    IMAGE_URL="https://nowpayments.io/images/coins/${FILE_NAME}"

    echo "🌐  Downloading: $IMAGE_URL"
    curl -s -f "$IMAGE_URL" -o "$FILE_PATH"

    if [[ $? -eq 0 ]]; then
        echo "✅  Saved → $FILE_PATH"
    else
        echo "❌  Failed: $IMAGE_URL" | tee -a "$LOGFILE"
        rm -f "$FILE_PATH"
    fi
done
echo "🏁  All icons processed"

echo
echo "✅ All done: database and services started, images downloaded!"
