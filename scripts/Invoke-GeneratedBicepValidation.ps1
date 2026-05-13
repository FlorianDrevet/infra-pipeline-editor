[CmdletBinding()]
param(
    [string]$OutputDirectory = ".\artifacts\generated-bicep-ci",
    [switch]$SkipBicepBuild
)

$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $repoRoot

$outputPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDirectory))
$env:IFS_BICEP_REPRESENTATIVE_OUTPUT_DIR = $outputPath

try {
    dotnet test .\tests\InfraFlowSculptor.BicepGeneration.Tests\InfraFlowSculptor.BicepGeneration.Tests.csproj --filter "FullyQualifiedName~RepresentativeBicepHarnessTests"
}
finally {
    Remove-Item Env:\IFS_BICEP_REPRESENTATIVE_OUTPUT_DIR -ErrorAction SilentlyContinue
}

$generatedBicepFiles = @(Get-ChildItem -Path $outputPath -Recurse -Filter *.bicep | Sort-Object FullName)
if ($generatedBicepFiles.Count -eq 0) {
    throw "No generated .bicep files were found under '$outputPath'."
}

if ($SkipBicepBuild) {
    Write-Host "Skipping 'bicep build'; generated $($generatedBicepFiles.Count) Bicep files under '$outputPath'."
    return
}

$bicepCommand = Get-Command bicep -ErrorAction SilentlyContinue
if ($null -eq $bicepCommand) {
    throw "The Bicep CLI is not available on PATH. Install it or rerun with -SkipBicepBuild."
}

foreach ($generatedBicepFile in $generatedBicepFiles) {
    Write-Host "Validating $($generatedBicepFile.FullName)"
    & $bicepCommand.Source build --file $generatedBicepFile.FullName | Out-Null
}

Write-Host "Validated $($generatedBicepFiles.Count) generated .bicep files under '$outputPath'."