pushd src
msbuild EFRepository.sln -p:Configuration=Release --verbosity:quiet

dotnet pack EFRepository-Core-3_0/EFRepository-Core-3_0.csproj --include-symbols -p:NuspecFile=../EFRepository/Mindfire.EFRepository.nuspec --output ../Releases -c Release
popd
