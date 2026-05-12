$ErrorActionPreference = 'Continue'
$issues = Get-Content tmp\open-issues-full.json -Raw | ConvertFrom-Json

# Build canonical map: normalized title -> issue without [AUDIT] prefix (canonical)
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
        # Both have [AUDIT] prefix - keep oldest
        $canon = $g.Group | Sort-Object Number | Select-Object -First 1
        $auditDups = $g.Group | Where-Object { $_.Number -ne $canon.Number }
    }
    foreach ($d in $auditDups) {
        $plan += [PSCustomObject]@{
            CloseNumber  = $d.Number
            CloseTitle   = $d.Title
            CanonicalNum = $canon.Number
            CanonicalTitle = $canon.Title
            Reason = "duplicate"
        }
    }
}

"=== CLOSE PLAN (duplicates) ==="
"Total to close: $($plan.Count)"
$plan | Format-Table -AutoSize | Out-String
$plan | Export-Csv -Path tmp\close-plan-dupes.csv -NoTypeInformation -Encoding utf8
"Plan written to tmp\close-plan-dupes.csv"
