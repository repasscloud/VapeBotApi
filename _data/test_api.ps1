# Tidy PowerShell test script

# Configuration
$baseUri = 'https://secure-endlessly-puma.ngrok-free.app'
$chatId  = '123123123'
$headers = @{ Accept = '*/*' }

try {
    Write-Host
    Write-Host "➡️  Fetching products..."
    $products = Invoke-RestMethod "$baseUri/api/admin/product" -Headers $headers
    if (-not $products) { throw "No products returned" }
    $productId = $products[0].productId
    Write-Host "   • First productId: $productId"

    Write-Host
    Write-Host "➡️  Checking current checkout for chatId $chatId..."
    $currentCheckout = Invoke-RestMethod "$baseUri/order/current/$chatId" -Headers $headers
    if (-not $currentCheckout) { Write-Host "   • No current checkout (OK)" }

    Write-Host
    Write-Host "➡️  Creating new checkout..."
    $checkoutId = Invoke-RestMethod "$baseUri/order/new/$chatId" -Headers $headers
    Write-Host "   • New checkout ID: $checkoutId"

    Write-Host
    Write-Host "➡️  Adding 10 of product $productId..."
    Invoke-RestMethod "$baseUri/cart/add/$chatId/$productId/10" -Headers $headers | Out-Null
    Write-Host "   • Added 10 items"

    Write-Host
    Write-Host "➡️  Removing 2 of product $productId..."
    Invoke-RestMethod "$baseUri/cart/sub/$chatId/$productId/2" -Headers $headers | Out-Null
    Write-Host "   • Removed 2 items"

    Write-Host
    Write-Host "➡️  Retrieving cart contents..."
    $cartItems = Invoke-RestMethod "$baseUri/cart/show/$chatId" -Headers $headers
    foreach ($item in $cartItems) {
        Write-Host "   • Product: $($item.name) | Qty: $($item.quantity) | Price: $($item.price)"
    }

    Write-Host
    Write-Host "➡️  Getting order subtotal..."
    $subtotal = Invoke-RestMethod "$baseUri/order/checkout/request/$chatId" -Headers $headers
    Write-Host "   • Subtotal: $subtotal"

    Write-Host
    Write-Host "➡️  Fetching shipping options..."
    $shippingOptions = Invoke-RestMethod "$baseUri/order/checkout/shipping/options/$chatId" -Headers $headers
    foreach ($opt in $shippingOptions) {
        Write-Host "   • Service: $($opt.service) | Rate: $($opt.price)"
    }

    Write-Host
    Write-Host "➡️  Setting shipping method to first option..."
    $service = $shippingOptions[0].service
    Invoke-RestMethod "$baseUri/order/checkout/shipping/set/$chatId/$service" -Headers $headers | Out-Null
    Write-Host "   • Shipping set to $service"

    Write-Host
    Write-Host "➡️  Setting payment method to Stripe..."
    $checkoutUri = Invoke-RestMethod "$baseUri/order/checkout/payment/set/$chatId/Stripe" -Headers $headers
    Write-Host "   • Checkout URI: $checkoutUri"
    Write-Host

} catch {
    Write-Error "❗  Error: $_"
    exit 1
}
