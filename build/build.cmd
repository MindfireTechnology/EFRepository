@echo off

echo Building EFRepositoryCore

dotnet build --configuration Release ..\src\EFRepositoryCore
dotnet build --configuration Release ..\src\EFRepository-Standard-2_0
dotnet build --configuration Release ..\src\EFRepository-Core-3_0
REM dotnet pack ..\src\EFRepositoryCore --output ..\..\releases\ --configuration Release

echo Building EFRepository for .NET Framework

msbuild ..\src\EFRepository\EFRepository.csproj /t:Build /p:Configuration=Release
nuget pack ..\src\EFRepository\Mindfire.EFRepository.nuspec -OutputDirectory ..\releases\



