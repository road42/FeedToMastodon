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

mv ./FeedToMastodon.Cli/bin/Release/netcoreapp2.1/linux-x64/publish ./FeedToMastodon-linux-x64-0.2.1
mv ./FeedToMastodon.Cli/bin/Release/netcoreapp2.1/win-x64/publish ./FeedToMastodon-win-x64-0.2.1
mv ./FeedToMastodon.Cli/bin/Release/netcoreapp2.1/osx-x64/publish ./FeedToMastodon-osx-x64-0.2.1

tar czvf FeedToMastodon-linux-x64-0.2.1.tar.gz FeedToMastodon-linux-x64-0.2.1/
zip -r FeedToMastodon-win-x64-0.2.1.zip FeedToMastodon-win-x64-0.2.1/
zip -r FeedToMastodon-osx-x64-0.2.1.zip FeedToMastodon-osx-x64-0.2.1/


rm -rf ./FeedToMastodon-linux-x64-*
rm -rf ./FeedToMastodon-win-x64-*
rm -rf ./FeedToMastodon-osx-x64-*