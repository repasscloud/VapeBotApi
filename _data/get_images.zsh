#!/usr/bin/env zsh

# list of URLs (no trailing commas)
urls=(
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074451-112947.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074740-581123.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074743-052908.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074745-508067.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074747-960466.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074750-392491.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074752-880496.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074755-962609.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074758-343814.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074800-717351.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074803-176314.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074805-524378.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074808-201760.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074810-597205.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074813-073875.png"
  "https://kuzvape.com/wp-content/uploads/2025/01/kuz-0114-074743-052908.png"
)

for url in $urls; do
  echo "Downloading ${url##*/}..."
  curl -L -O "$url"
done

echo "All done."
