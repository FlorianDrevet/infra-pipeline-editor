<#
.SYNOPSIS
  Fifth pass — fixes remaining hardcoded semantic and gray-blue text colors.
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  # Gray-blue text (light-mode muted/secondary) → dark-mode tokens
  '#617892'  = 'var(--ifs-text-secondary)'
  '#546e7a'  = 'var(--ifs-text-secondary)'
  '#78909c'  = 'var(--ifs-text-muted)'
  '#90a4ae'  = 'var(--ifs-text-muted)'
  '#b0bec5'  = 'var(--ifs-text-muted)'
  '#8fa4b8'  = 'var(--ifs-text-muted)'
  '#37474f'  = 'var(--ifs-text-primary)'
  '#1a1a2e'  = 'var(--ifs-text-primary)'
  '#8ab3b6'  = 'var(--ifs-text-muted)'

  # Error reds (light-mode dark red → dark-mode bright red)
  '#c62828'  = 'var(--ifs-danger)'
  '#b42318'  = 'var(--ifs-danger)'
  '#bf360c'  = 'var(--ifs-danger)'

  # Warning/orange (light-mode dark orange → dark-mode bright warning)
  '#e65100'  = 'var(--ifs-warning)'
  '#ed6c02'  = 'var(--ifs-warning)'
  '#f57f17'  = 'var(--ifs-warning)'
  '#b36b00'  = 'var(--ifs-warning)'
  '#b45309'  = 'var(--ifs-warning)'
  '#92400e'  = 'var(--ifs-warning)'
  '#b8770a'  = 'var(--ifs-warning)'
  '#ef6c00'  = 'var(--ifs-warning)'
  '#795548'  = 'var(--ifs-text-muted)'
  '#6d4c41'  = 'var(--ifs-text-muted)'

  # Light-mode near-white → dark-mode text-primary
  '#f5f8fd'  = 'var(--ifs-text-primary)'
  '#f4fbfb'  = 'var(--ifs-text-primary)'
  '#fecaca'  = 'var(--ifs-danger)'

  # success green (light-mode dark green → bright green)
  '#2e7d32'  = 'var(--ifs-success)'
}

$files = Get-ChildItem -Path $root -Recurse -Filter '*.scss' |
  Where-Object { $_.FullName -notmatch 'node_modules|dist|\.angular|_tokens\.scss|_mixins\.scss|login\.component\.scss|_main\.scss' }

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
