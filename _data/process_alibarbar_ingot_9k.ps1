# process-csv.ps1

param(
  [string]$CsvPath = "$PSScriptRoot\import_Alibarbar_Ingot_9K.csv",
  [string]$Endpoint = "https://secure-endlessly-puma.ngrok-free.app/api/admin/product"
)

# Read the CSV
$rows = Import-Csv -Path $CsvPath

foreach ($row in $rows) {
    # Build JSON body
    $body = @{
        name = $row.name
        imageUrl = $row.imageUrl
        emoji = $row.emoji
        price = $row.price
        categoryId = [int]$row.categoryId
    } | ConvertTo-Json

    # POST it
    Invoke-RestMethod -Uri $Endpoint `
                      -Method Post `
                      -ContentType "application/json" `
                      -Body $body `
                      -AllowInsecureRedirect

    # Pause 0.5s
    Start-Sleep -Milliseconds 500
}
