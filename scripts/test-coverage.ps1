param(
    [string]$Target = ".\InfraFlowSculptor.slnx",
    [string]$ResultsDirectory = ".\artifacts\TestResults",
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$AdditionalArguments
)

$repoRoot = Split-Path -Parent $PSScriptRoot
$exitCode = 0

Push-Location $repoRoot
try {
    if (-not (Test-Path $ResultsDirectory)) {
        New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null
    }

    $dotnetArguments = @(
        'test'
        $Target
        '--collect:XPlat Code Coverage'
        '--results-directory'
        $ResultsDirectory
    ) + $AdditionalArguments

    & dotnet @dotnetArguments
    $exitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

exit $exitCode