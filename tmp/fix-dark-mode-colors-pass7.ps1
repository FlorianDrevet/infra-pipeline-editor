<#
.SYNOPSIS
  Seventh pass — catches ALL remaining light-mode hex text/bg colors
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  # Light-mode grays and blue-grays (text)
  '#3b4f66' = 'var(--ifs-text-primary)'
  '#2c3e50' = 'var(--ifs-text-primary)'
  '#5a6e82' = 'var(--ifs-text-secondary)'
  '#6b8299' = 'var(--ifs-text-muted)'
  '#8fa3b8' = 'var(--ifs-text-muted)'
  '#e0e6ed' = 'var(--ifs-border-subtle)'
  '#f0f3f6' = 'var(--ifs-border-subtle)'

  # Light borders
  '#e5eaf0' = 'var(--ifs-border-subtle)'
  '#dce4ec' = 'var(--ifs-border-subtle)'
  '#cdd7e3' = 'var(--ifs-border-subtle)'

  # Warning/orange
  '#ff9800' = 'var(--ifs-warning)'
  '#f44336' = 'var(--ifs-danger)'
}

$files = Get-ChildItem -Path $root -Recurse -Filter '*.scss' |
  Where-Object { $_.FullName -notmatch 'node_modules|dist|\.angular|_tokens\.scss|_mixins\.scss|login\.component\.scss|_main\.scss' }

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
