$files = Get-ChildItem -Path "src\Front\src\app\shared\interfaces" -Filter "*.interface.ts"
foreach ($f in $files) {
    $c = Get-Content $f.FullName -Raw
    if ($c -match "isExisting\?\: boolean;\`n\}") {
        $c = $c -replace "isExisting\?\: boolean;\`n\}", "isExisting?: boolean;`n}"
        Set-Content $f.FullName $c -NoNewline
        Write-Host "Fixed: $($f.Name)"
    }
}
