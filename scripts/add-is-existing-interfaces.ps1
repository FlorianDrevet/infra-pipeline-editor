$interfaceFiles = Get-ChildItem -Path "src\Front\src\app\shared\interfaces" -Filter "*.interface.ts" | Where-Object { $_.Name -notmatch "^(auth|bicep|config-diag|cross-config|dependent|environment|infra-config|name-availability|pipeline|project|resource-group|role-assignment|secure-param)" }

foreach ($f in $interfaceFiles) {
    $c = Get-Content $f.FullName -Raw
    if ($c -match 'isExisting') { Write-Host "Skip (already has isExisting): $($f.Name)"; continue }
    
    # Find the Response interface block and add isExisting before the closing }
    # Look for the pattern: "  environmentSettings: ...Response[];\n}" or similar last property before }
    $patternResponse = '(export interface \w+Response \{[^}]+)(})'
    if ($c -match $patternResponse) {
        $nc = [regex]::Replace($c, '(export interface (\w+Response) \{([^}]*))(})', {
            param($m)
            $block = $m.Groups[1].Value
            $close = $m.Groups[4].Value
            "$block  isExisting?: boolean;`n$close"
        })
        if ($c -ne $nc) {
            Set-Content $f.FullName $nc -NoNewline
            Write-Host "Updated: $($f.Name)"
        } else {
            Write-Host "No match: $($f.Name)"
        }
    } else {
        Write-Host "No Response interface found: $($f.Name)"
    }
}
