# Define your list of URLs
$urls = @(
    'https://just-vapes.com/product/banana-buzz-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/blackberry-ice-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/blueberry-blast-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/blueberry-mint-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/fanta-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/strawberry-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/cool-mint-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/banana-buzz-alibarbar-ingot-9000-puffs-copy/',
    'https://just-vapes.com/product/ftp-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/grape-ice-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/kiwi-pineapple-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/mango-magic-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/passionfruit-mango-lime-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/peach-ice-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/pink-lemon-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/ribena-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/skittles-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/strawberry-coconut-watermelon-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/strawberry-lychee-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/strawberry-watermelon-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/watermelon-ice-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/wtf-alibarbar-ingot-9000-puffs/',
    'https://just-vapes.com/product/tobacco-alibarbar-ingot-9000-puffs/'
)


foreach ($url in $urls) {
    try {
        $resp = Invoke-WebRequest -Uri $url -UseBasicParsing
        $resp.Links |
          Where-Object { $_.href -match '\.png' } |
          ForEach-Object { 
              # just print the link, nothing else
              $_.href 
          }
    }
    catch {
        Write-Warning "Failed to fetch $url : $_"
    }
}

