#!/usr/bin/env zsh
set -euo pipefail

# dir vars
kuz_path="images/kuz"
cig_path="images/cigarettes"
alb9k_path="images/alibarbar_9k"

# remove output dir paths
rm -rf $kuz_path
rm -rf $cig_path
rm -rf $alb9k_path

# ensure output dirs exist
mkdir -p $kuz_path $cig_path $alb9k_path

# first batch
kuz_urls=(
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

for url in $kuz_urls; do
  fname=${url##*/}
  echo "Downloading $fname → $kuz_path/$fname"
  curl -L -o "$kuz_path/$fname" "$url"
done

# second batch
cig_urls=(
  "https://handrollingtobacco.co.uk/wp-content/uploads/2024/08/manchester-royal-red-indonesia-duty-free-002.jpg"
  "https://www.ciggiesworld.ch/wp-content/uploads/2024/12/manchester-sapphire-blue-cigarettes.png"
  "https://www.ciggiesworld.ch/wp-content/uploads/2023/12/Marlboro-Red-Premium-Class-Cigarette.jpg"
  "https://www.ciggiesworld.ch/wp-content/uploads/2023/12/Marlboro_Gold_Original.png"
  "https://www.ciggiesworld.ch/wp-content/uploads/2023/12/Marlboro-Ice-Blast-Cigarette.jpg"
  "https://www.ciggiesworld.ch/wp-content/uploads/2023/12/Marlboro-Ice-Burst-Opened-Pack-1.jpg"
)

for url in $cig_urls; do
  fname=${url##*/}
  echo "Downloading $fname → $cig_path/$fname"
  curl -L -o "$cig_path/$fname" "$url"
done

# third batch
alb9k_urls=(
  https://just-vapes.com/wp-content/uploads/2025/03/strawberry-lychee.jpg
  https://just-vapes.com/wp-content/uploads/2025/04/ALIBARBAR-STRAWBERRY-WATERMELON.jpg
  https://just-vapes.com/wp-content/uploads/2024/06/alibarbar-vape-9000-banana-buzz-ingot.png                             
  https://just-vapes.com/wp-content/uploads/2024/06/alibarbar-vape-9000-blackberry-ive-ingot.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-bluebrry-blast-ingot.png                          
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-BLUEBERRY-MINT.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-california-sunset-ingot.png                       
  https://just-vapes.com/wp-content/uploads/2024/07/sss-min.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-cool-mint-ingot.png                               
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-double-apple-ingot.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-FTP.png                                           
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-grape-ice-ingot.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-kiwi-pineapple-ingot.png                          
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-mango-magic-ingot.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-passionfruit-mango-lime-ingot.png                 
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-pink-lemon-ingot.png                              
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-SKITTLES.png                                      
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-strawberry-coconut-watermelon-ingot.png           
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-watermelon-ice-ingot.png                          
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-WTF.png
  https://just-vapes.com/wp-content/uploads/2024/11/alibarbar-vape-9000-tobacco-ingot.png
  https://just-vapes.com/wp-content/uploads/2025/03/ribena.jpg
  https://just-vapes.com/wp-content/uploads/2025/04/ALIBARBAR-PEACH-ICE.jpg
)

for url in $alb9k_urls; do
  fname=${url##*/}
  echo "Downloading $fname → $alb9k_path/$fname"
  curl -L -o "$alb9k_path/$fname" "$url"
done

echo "All done."
