$files = Get-ChildItem -Path "src\Front\src\app\shared\interfaces" -Filter "*.interface.ts"
$fixed = 0
foreach ($f in $files) {
    $c = [System.IO.File]::ReadAllText($f.FullName)
    $needle = "isExisting?: boolean;" + '`' + "n}"
    if ($c.Contains($needle)) {
        $replacement = "isExisting?: boolean;`n}"
        $c = $c.Replace($needle, $replacement)
        [System.IO.File]::WriteAllText($f.FullName, $c)
        Write-Host "Fixed: $($f.Name)"
        $fixed++
    }
}
Write-Host "Total fixed: $fixed"
