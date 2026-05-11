<#
.SYNOPSIS
  Replaces all hardcoded light-mode colors across SCSS files with V2 dark-mode token references.
  Run from repo root.
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  # Light-mode backgrounds → dark surfaces
  'rgba(255, 255, 255, 0.88)' = 'var(--ifs-surface-2)'
  'rgba(255, 255, 255, 0.86)' = 'var(--ifs-surface-2)'
  'rgba(255, 255, 255, 0.78)' = 'var(--ifs-surface-2)'
  'rgba(255, 255, 255, 0.7)'  = 'var(--ifs-surface-2)'
  'rgba(255, 255, 255, 0.96)' = 'var(--ifs-surface-3)'
  'rgba(241, 247, 255, 0.7)'  = 'var(--ifs-surface-2)'
  'rgba(252, 254, 255, 0.98)' = 'var(--ifs-surface-2)'
  'rgba(239, 246, 255, 0.94)' = 'var(--ifs-surface-2)'
  'rgba(245, 247, 251, 0.92)' = 'var(--ifs-surface-2)'
  'rgba(236, 247, 255, 0.98)' = 'var(--ifs-surface-2)'
  'rgba(229, 241, 255, 0.98)' = 'var(--ifs-surface-2)'
  'rgba(255, 245, 245, 0.98)' = 'var(--ifs-danger-bg)'
  'rgba(255, 240, 240, 1)'    = 'var(--ifs-danger-bg)'
  'rgba(255, 228, 228, 0.98)' = 'var(--ifs-danger-bg)'

  # Light-mode borders → dark border tokens
  'rgba(146, 191, 235, 0.22)' = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.2)'  = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.18)' = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.14)' = 'var(--ifs-border-subtle)'
  'rgba(146, 191, 235, 0.28)' = 'var(--ifs-border-strong)'
  'rgba(113, 163, 212, 0.34)' = 'var(--ifs-border-strong)'

  # Light-mode shadows → dark shadow tokens
  'box-shadow: 0 10px 28px rgba(16, 52, 86, 0.06)' = 'box-shadow: var(--ifs-shadow-sm)'
  'box-shadow: 0 16px 40px rgba(16, 52, 86, 0.06)' = 'box-shadow: var(--ifs-shadow-md)'
  'box-shadow: 0 6px 18px rgba(16, 52, 86, 0.05)'  = 'box-shadow: var(--ifs-shadow-sm)'

  # Light-mode text colors → dark text tokens  
  '#0d2b4f' = 'var(--ifs-text-primary)'
  '#103454' = 'var(--ifs-text-primary)'
  '#0e1a2d' = 'var(--ifs-text-primary)'
  '#1f4f81' = 'var(--ifs-text-primary)'
  '#1a3355' = 'var(--ifs-text-primary)'
  '#60758e' = 'var(--ifs-text-secondary)'
  '#5b718b' = 'var(--ifs-text-secondary)'
  '#8da4ba' = 'var(--ifs-text-muted)'
  '#3b6fa0' = 'var(--ifs-accent-500)'

  # Light-mode brand blue → V2 accent/brand
  '#1565c0' = 'var(--ifs-accent-500)'
  '#0d47a1' = 'var(--ifs-accent-600)'
  '#155ea5' = 'var(--ifs-accent-600)'
  '#0d63c7' = 'var(--ifs-accent-600)'

  # Light-mode brand bg tints → dark token equivalents
  'rgba(21, 101, 192, 0.08)' = 'rgba(58, 180, 217, 0.12)'
  'rgba(21, 101, 192, 0.06)' = 'rgba(58, 180, 217, 0.08)'
  'rgba(13, 101, 192, 0.06)' = 'rgba(58, 180, 217, 0.08)'
  'rgba(13, 101, 192, 0.08)' = 'rgba(58, 180, 217, 0.12)'
  'rgba(13, 101, 192, 0.12)' = 'rgba(58, 180, 217, 0.15)'
  'rgba(13, 101, 192, 0.22)' = 'rgba(58, 180, 217, 0.25)'
  'rgba(21, 101, 192, 0.12)' = 'rgba(58, 180, 217, 0.15)'
  'rgba(21, 101, 192, 0.15)' = 'rgba(58, 180, 217, 0.18)'
  'rgba(21, 101, 192, 0.22)' = 'rgba(58, 180, 217, 0.25)'
  'rgba(21, 101, 192, 0.24)' = 'rgba(58, 180, 217, 0.2)'
  'rgba(21, 101, 192, 0.26)' = 'rgba(58, 180, 217, 0.25)'

  # Old pulse dot blue → accent
  '#0288d1' = 'var(--ifs-accent-500)'
  'rgba(2, 136, 209, 0.36)' = 'rgba(58, 180, 217, 0.36)'
  'rgba(2, 136, 209, 0)' = 'rgba(58, 180, 217, 0)'

  # Focus ring fallbacks (light) → dark-appropriate
  'rgba(21, 101, 192, 0.16)' = 'rgba(58, 180, 217, 0.2)'
  'rgba(21, 101, 192, 0.28)' = 'rgba(58, 180, 217, 0.3)'

  # Old red from light-mode semantic
  'rgba(244, 67, 54, 0.12)' = 'var(--ifs-danger-bg)'
  'rgba(244, 67, 54, 0.16)' = 'rgba(210, 74, 74, 0.2)'
  'rgba(244, 67, 54, 0.28)' = 'rgba(210, 74, 74, 0.3)'
  'rgba(217, 45, 32, 0.08)' = 'var(--ifs-danger-bg)'
  '#be3b35' = 'var(--ifs-danger)'
  '#d32f2f' = 'var(--ifs-danger)'
  '#b3261e' = 'var(--ifs-danger)'

  # Old success green from light mode
  'rgba(14, 159, 110, 0.08)' = 'var(--ifs-success-bg)'
  '#0e9f6e' = 'var(--ifs-success)'

  # Old cyan tint
  'rgba(0, 188, 212, 0.52)' = 'rgba(58, 180, 217, 0.5)'
  'rgba(0, 188, 212, 0.28)' = 'rgba(58, 180, 217, 0.3)'
  'rgba(0, 188, 212, 0.4)'  = 'rgba(58, 180, 217, 0.4)'

  # White overlay on dark → transparent/neutral
  'rgba(255, 255, 255, 0.12)' = 'rgba(255, 255, 255, 0.06)'
  'rgba(255, 255, 255, 0.16)' = 'rgba(255, 255, 255, 0.08)'
  'rgba(255, 255, 255, 0.25)' = 'rgba(255, 255, 255, 0.12)'
  'rgba(255, 255, 255, 0.18)' = 'rgba(255, 255, 255, 0.08)'
  'rgba(255, 255, 255, 0.22)' = 'rgba(255, 255, 255, 0.1)'
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

Write-Host "`nTotal: $totalReplacements replacements across $($files.Count) scanned files"
