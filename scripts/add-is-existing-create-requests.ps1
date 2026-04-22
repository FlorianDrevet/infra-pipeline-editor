$interfaceFiles = Get-ChildItem -Path "src\Front\src\app\shared\interfaces" -Filter "*.interface.ts"

foreach ($f in $interfaceFiles) {
    $c = Get-Content $f.FullName -Raw
    # Find all Create*Request interfaces that don't have isExisting
    $matches = [regex]::Matches($c, '(export interface Create\w+Request \{[^}]*)(})')
    foreach ($m in $matches) {
        if ($m.Value -notmatch 'isExisting') {
            $old = $m.Value
            $new = $old -replace '(export interface Create\w+Request \{[^}]*)(})', '$1  isExisting?: boolean;`n$2'
            $c = $c.Replace($old, $new)
            Write-Host "Updated Create interface in $($f.Name)"
        }
    }
    Set-Content $f.FullName $c -NoNewline
}
