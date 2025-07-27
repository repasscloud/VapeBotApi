# Step 1: Set paths
$iconDir = "./wwwroot/img/nowpaymentsio"
$csvPath = "./nowpayments_currency_upload.csv"
$uploadUrl = "https://secure-endlessly-puma.ngrok-free.app/api/Payments/nowpayments/currencies"

# Step 2: Init CSV header
if (Test-Path -Path $csvPath) { Remove-Item -Path $csvPath -Confirm:$false -Force }
$csvHeader = "CoinName,CurrencyCodeFull,CurrencyCode,Network,ImageUrl"
Set-Content -Path $csvPath -Value $csvHeader

# Step 3: Build rows
Get-ChildItem -Path $iconDir -Filter *.svg | ForEach-Object {
    $codeFull = $_.BaseName
    $imageUrl = "https://secure-endlessly-puma.ngrok-free.app/img/nowpaymentsio/$codeFull.svg"
    $csvLine = '"","","' + $codeFull + '","","' + $imageUrl + '"'
    Add-Content -Path $csvPath -Value $csvLine
}

# # Step 4: Read CSV & convert to JSON
# $csvData = Import-Csv -Path $csvPath
# $jsonData = $csvData | ConvertTo-Json -Depth 5

# # Step 5: Upload JSON to API
# $response = Invoke-WebRequest -Uri $uploadUrl -Method POST -Body $jsonData -ContentType "application/json"
# Write-Host "✅ Upload complete. Response status: $($response.StatusCode)"
