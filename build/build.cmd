@echo off
echo Building EFRepositoryCore

dotnet build --configuration Release ..\src\EFRepositoryCore
REM dotnet pack ..\src\EFRepositoryCore --output ..\..\releases\ --configuration Release

echo EFRepositoryCore Packed


echo Building EFRepository for .NET Framework

msbuild ..\src\EFRepository\EFRepository.csproj /t:Build /p:Configuration=Release
nuget pack ..\src\EFRepository\Mindfire.EFRepository.nuspec -OutputDirectory ..\releases\



