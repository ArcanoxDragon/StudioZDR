Push-Location $PSScriptRoot

$ErrorActionPreference = "SilentlyContinue"

# Kill dotnet build server (and any cached MSBuild instances)
dotnet build-server shutdown
taskkill /f /im dotnet.exe /im msbuild.exe

# Remove all cache and output files
Remove-Item -Force -Recurse ./.vs/ProjectEvaluation
Remove-Item -Force -Recurse ./.vs/StudioZDR/DesignTimeBuild
Remove-Item -Force -Recurse ./.vs/StudioZDR/FileContentIndex
Remove-Item -Force -Recurse ./.vs/StudioZDR/v17/.futdcache.v2
Remove-Item -Force -Recurse ./_ReSharper.Caches
Get-ChildItem -Include bin,obj -Force -Recurse | Remove-Item -Force -Recurse

# Rebuild the entire project
dotnet build -c Debug

Pop-Location