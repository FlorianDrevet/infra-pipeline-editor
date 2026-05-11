<#
.SYNOPSIS
  Third pass — fixes remaining hardcoded colors in specific files.
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  # Brand/link blue on light bg
  '#0d65c0'                     = 'var(--ifs-accent-500)'
  '#0a4f99'                     = 'var(--ifs-accent-600)'
  'rgba(13, 47, 102, 0.06)'    = 'var(--ifs-surface-1)'
  'rgba(13, 101, 192, 0.28)'   = 'var(--ifs-accent-500)'
  'rgba(13, 101, 192, 0.35)'   = 'var(--ifs-accent-500)'
  'rgba(13, 101, 192, 0.04)'   = 'rgba(58, 180, 217, 0.06)'
  'rgba(13, 101, 192, 0.05)'   = 'rgba(58, 180, 217, 0.08)'
  'rgba(13, 101, 192, 0.1)'    = 'rgba(58, 180, 217, 0.15)'
  'rgba(2, 136, 209, 0.1)'     = 'rgba(58, 180, 217, 0.12)'
  'rgba(13, 47, 102, 0.12)'    = 'var(--ifs-shadow-sm)'

  # White bg elements (option cards, ACR)
  'background: #fff;'          = 'background: var(--ifs-surface-2);'
  'background: #ffffff;'       = 'background: var(--ifs-surface-3);'

  # Light text on light bg
  '#5a7da0'                     = 'var(--ifs-text-secondary)'
  '#3a6a8f'                     = 'var(--ifs-text-primary)'
  '#8ba4be'                     = 'var(--ifs-text-muted)'

  # Warn card (yellow bg on dark = too bright)
  'rgba(254, 243, 199, 0.7)'   = 'rgba(245, 166, 35, 0.08)'
  'rgba(245, 166, 35, 0.65)'   = 'rgba(245, 166, 35, 0.25)'
  '#e6a117'                    = 'var(--ifs-warning)'

  # Success
  'rgba(46, 125, 50, 0.05)'    = 'rgba(46, 125, 50, 0.08)'
  'rgba(46, 125, 50, 0.12)'    = 'rgba(46, 125, 50, 0.2)'
  '#2e7d32'                    = 'var(--ifs-success)'

  # Error
  '#a12d1f'                    = 'var(--ifs-danger)'
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
