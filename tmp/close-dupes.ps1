$ErrorActionPreference = 'Continue'
$DryRun = $false
$Repo = 'FlorianDrevet/infra-pipeline-editor'

$issues = Get-Content tmp\open-issues-full.json -Raw | ConvertFrom-Json
$norm = $issues | ForEach-Object {
    $n = $_.title -replace '^\[AUDIT\]\s*',''
    [PSCustomObject]@{Number=$_.number; Title=$_.title; Norm=$n; HasAuditPrefix=($_.title -like '`[AUDIT`]*')}
}
$dupGroups = $norm | Group-Object Norm | Where-Object { $_.Count -gt 1 }

$plan = @()
foreach ($g in $dupGroups) {
    $canon = $g.Group | Where-Object { -not $_.HasAuditPrefix } | Select-Object -First 1
    $auditDups = $g.Group | Where-Object { $_.HasAuditPrefix }
    if ($null -eq $canon) {
        $canon = $g.Group | Sort-Object Number | Select-Object -First 1
        $auditDups = $g.Group | Where-Object { $_.Number -ne $canon.Number }
    }
    foreach ($d in $auditDups) {
        $plan += [PSCustomObject]@{ CloseNum=$d.Number; CanonicalNum=$canon.Number; Title=$d.Title }
    }
}

"Plan size: $($plan.Count)"
$ok = 0; $fail = 0
foreach ($p in $plan) {
    $msg = "Closing as duplicate of #$($p.CanonicalNum) (audit-workflow re-sync artifact, same finding code, identical content). Canonical issue tracks the work."
    if ($DryRun) {
        "DRY: gh issue close $($p.CloseNum) -> dup of #$($p.CanonicalNum)  | $($p.Title)"
        $ok++
    } else {
        Write-Host "Closing #$($p.CloseNum) (dup of #$($p.CanonicalNum))"
        & gh issue close $p.CloseNum --repo $Repo --reason 'not planned' --comment $msg 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) { $ok++ } else { $fail++; Write-Host "FAIL #$($p.CloseNum)" -ForegroundColor Red }
    }
}
"Closed OK: $ok / Failed: $fail"
