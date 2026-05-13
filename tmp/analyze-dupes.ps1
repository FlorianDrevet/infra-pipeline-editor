$issues = Get-Content tmp\open-issues-full.json -Raw | ConvertFrom-Json
$norm = $issues | ForEach-Object {
    $n = $_.title -replace '^\[AUDIT\]\s*',''
    [PSCustomObject]@{Number=$_.number; Title=$_.title; Norm=$n; Created=$_.createdAt; BodyLen=$_.body.Length; Labels=(($_.labels.name) -join ',')}
}
$grouped = $norm | Group-Object Norm | Where-Object { $_.Count -gt 1 } | Sort-Object Count -Descending
"Duplicate groups: $($grouped.Count)"
"Total issues in dup groups: $(($grouped | ForEach-Object { $_.Count } | Measure-Object -Sum).Sum)"
""
"=== ALL DUPLICATE GROUPS ==="
$grouped | ForEach-Object {
    "GROUP ($($_.Count)): $($_.Name)"
    $_.Group | Sort-Object Created | ForEach-Object {
        "  #$($_.Number) created=$(($_.Created -split 'T')[0]) bodyLen=$($_.BodyLen) title='$($_.Title)'"
    }
}
""
"=== UNIQUE (no duplicate) ==="
$norm | Group-Object Norm | Where-Object { $_.Count -eq 1 } | ForEach-Object {
    $g = $_.Group[0]
    "  #$($g.Number) $($g.Title)"
}
