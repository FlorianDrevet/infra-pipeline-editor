<#
.SYNOPSIS
  Sixth pass — fixes remaining light backgrounds
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  '#f8fbff' = 'var(--ifs-surface-2)'
}

$files = Get-ChildItem -Path $root -Recurse -Filter '*.scss' |
  Where-Object { $_.FullName -notmatch 'node_modules|dist|\.angular|_tokens\.scss|_mixins\.scss|login\.component\.scss' }

$totalReplacements = 0

foreach ($file in $files) {
  $content = Get-Content -Path $file.FullName -Raw
  $fileReplacements = 0

  foreach ($key in $replacements.Keys) {
    $escaped = [regex]::Escape($key)
    $matches = [regex]::Matches($content, $escaped)
    if ($matches.Count -gt 0) {
      $content = $content -replace $escaped, $replacements[$key]
      $fileReplacements += $matches.Count
    }
  }

  if ($fileReplacements -gt 0) {
    Set-Content -Path $file.FullName -Value $content -NoNewline
    $rel = $file.FullName -replace [regex]::Escape($root + '\'), ''
    Write-Host "$rel : $fileReplacements replacements"
    $totalReplacements += $fileReplacements
  }
}

Write-Host "`nTotal: $totalReplacements replacements"
