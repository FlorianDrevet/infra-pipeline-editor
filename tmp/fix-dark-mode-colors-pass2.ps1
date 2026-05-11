<#
.SYNOPSIS
  Second pass — fixes remaining hardcoded light-mode colors.
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  # Remaining rgba(146, 191, 235, ...) borders
  'rgba(146, 191, 235, 0.24)' = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.25)' = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.15)' = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.3)'  = 'var(--ifs-border-strong)'
  'rgba(146, 191, 235, 0.4)'  = 'var(--ifs-border-strong)'
  'rgba(146, 191, 235, 0.5)'  = 'var(--ifs-accent-500)'

  # rgba(16, 52, 84, ...) text / bg on light
  'rgba(16, 52, 84, 0.88)' = 'var(--ifs-text-primary)'
  'rgba(16, 52, 84, 0.85)' = 'var(--ifs-text-primary)'
  'rgba(16, 52, 84, 0.08)' = 'var(--ifs-surface-2)'
  'rgba(16, 52, 84, 0.05)' = 'var(--ifs-surface-2)'
  'rgba(16, 52, 84, 0.03)' = 'var(--ifs-surface-1)'
  'rgba(16, 52, 86, 0.06)' = 'rgba(0, 0, 0, 0.12)'
  'rgba(16, 52, 86, 0.12)' = 'rgba(0, 0, 0, 0.2)'

  # Old focus ring with old accent
  'rgba(2, 136, 209, 0.18)' = 'rgba(58, 180, 217, 0.25)'

  # White text on dark brand (OK but in bootstrap-setup)
  'rgba(255, 255, 255, 0.84)' = 'var(--ifs-text-secondary)'
}

$files = Get-ChildItem -Path $root -Recurse -Filter '*.scss' |
  Where-Object { $_.FullName -notmatch 'node_modules|dist|\.angular|_tokens\.scss|_mixins\.scss|login\.component\.scss' }

$totalReplacements = 0

foreach ($file in $files) {
  $content = Get-Content -Path $file.FullName -Raw
  $original = $content
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
