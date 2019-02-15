#!/bin/sh

# Set new version first in
#  - FeedToMastodon.Lib/Constants.cs
#  - FeedToMastodon.Cli/FeedToMastodon.Cli.csproj

rm -f ./FeedToMastodon-linux-x64-*.tar.gz
rm -f ./FeedToMastodon-win-x64-*.zip
rm -f ./FeedToMastodon-osx-x64-*.zip

dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r osx-x64

mv ./FeedToMastodon.Cli/bin/Release/netcoreapp2.2/linux-x64/publish ./FeedToMastodon-linux-x64-0.2.2
mv ./FeedToMastodon.Cli/bin/Release/netcoreapp2.2/win-x64/publish ./FeedToMastodon-win-x64-0.2.2
mv ./FeedToMastodon.Cli/bin/Release/netcoreapp2.2/osx-x64/publish ./FeedToMastodon-osx-x64-0.2.2

tar czvf FeedToMastodon-linux-x64-0.2.2.tar.gz FeedToMastodon-linux-x64-0.2.2/
zip -r FeedToMastodon-win-x64-0.2.2.zip FeedToMastodon-win-x64-0.2.2/
zip -r FeedToMastodon-osx-x64-0.2.2.zip FeedToMastodon-osx-x64-0.2.2/


rm -rf ./FeedToMastodon-linux-x64-0.2.2
rm -rf ./FeedToMastodon-win-x64-0.2.2
rm -rf ./FeedToMastodon-osx-x64-0.2.2