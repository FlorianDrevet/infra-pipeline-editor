<#
.SYNOPSIS
  Replaces ALL deprecated SCSS $ifs-* tokens with their V2 CSS var equivalents.
  Run from repo root. Targets only scss files under src/Front/src, excluding
  _tokens.scss and _mixins.scss (those will be cleaned separately).
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

# Ordered from longest to shortest to avoid partial matches
# e.g. $ifs-gradient-brand-soft must be replaced BEFORE $ifs-gradient-brand
$replacements = [ordered]@{
  # Gradients
  '$ifs-gradient-brand-soft'    = 'var(--ifs-gradient-cta)'
  '$ifs-gradient-glass-footer'  = 'var(--ifs-surface-1)'
  '$ifs-gradient-card-soft'     = 'var(--ifs-surface-1)'
  '$ifs-gradient-app-bg'        = 'var(--ifs-bg)'
  '$ifs-gradient-brand'         = 'var(--ifs-gradient-cta)'
  '$ifs-gradient-login'         = 'var(--ifs-gradient-login)'
  '$ifs-gradient-nav'           = 'var(--ifs-surface-1)'
  '$ifs-gradient-cta'           = 'var(--ifs-gradient-cta)'

  # Shadows
  '$ifs-shadow-cta-hover' = 'var(--ifs-shadow-md)'
  '$ifs-shadow-cta'       = 'var(--ifs-shadow-md)'
  '$ifs-shadow-hero'      = 'var(--ifs-shadow-lg)'
  '$ifs-shadow-nav'       = 'var(--ifs-shadow-sm)'
  '$ifs-shadow-xs'        = 'var(--ifs-shadow-sm)'
  '$ifs-shadow-xl'        = 'var(--ifs-shadow-lg)'

  # Radii
  '$ifs-radius-3xl' = 'var(--ifs-radius-lg)'
  '$ifs-radius-2xl' = 'var(--ifs-radius-lg)'
  '$ifs-radius-xl'  = 'var(--ifs-radius-lg)'

  # Spacing (longest first)
  '$ifs-space-16' = 'var(--ifs-space-64)'
  '$ifs-space-12' = 'var(--ifs-space-48)'
  '$ifs-space-10' = 'var(--ifs-space-40)'
  '$ifs-space-8'  = 'var(--ifs-space-32)'
  '$ifs-space-6'  = 'var(--ifs-space-24)'
  '$ifs-space-5'  = 'var(--ifs-space-20)'
  '$ifs-space-4'  = 'var(--ifs-space-16)'
  '$ifs-space-3'  = 'var(--ifs-space-12)'
  '$ifs-space-2'  = 'var(--ifs-space-8)'
  '$ifs-space-1'  = 'var(--ifs-space-4)'
  '$ifs-space-0'  = '0'

  # Brand colors (longest first)
  '$ifs-brand-cyan-light' = 'var(--ifs-accent-500)'
  '$ifs-brand-cyan-deep'  = 'var(--ifs-accent-600)'
  '$ifs-brand-blue-deep'  = 'var(--ifs-brand-700)'
  '$ifs-brand-dark-blue'  = 'var(--ifs-brand-700)'
  '$ifs-brand-cyan'       = 'var(--ifs-accent-500)'
  '$ifs-brand-teal'       = 'var(--ifs-accent-500)'
  '$ifs-brand-blue'       = 'var(--ifs-brand-500)'

  # Ink (longest first)
  '$ifs-ink-900' = 'var(--ifs-text-primary)'
  '$ifs-ink-800' = 'var(--ifs-text-primary)'
  '$ifs-ink-700' = 'var(--ifs-text-primary)'
  '$ifs-ink-500' = 'var(--ifs-text-secondary)'
  '$ifs-ink-400' = 'var(--ifs-text-muted)'
  '$ifs-ink-300' = 'var(--ifs-border-strong)'
  '$ifs-ink-200' = 'var(--ifs-border-subtle)'
  '$ifs-ink-100' = 'var(--ifs-border-subtle)'

  # Surfaces (longest first)
  '$ifs-surface-tinted' = 'var(--ifs-surface-2)'
  '$ifs-surface-300'    = 'var(--ifs-surface-3)'
  '$ifs-surface-200'    = 'var(--ifs-surface-2)'
  '$ifs-surface-100'    = 'var(--ifs-surface-2)'
  '$ifs-surface-50'     = 'var(--ifs-surface-1)'
  '$ifs-surface-0'      = 'var(--ifs-surface-1)'

  # Semantic (longest first)
  '$ifs-error-strong'   = 'var(--ifs-danger)'
  '$ifs-error-border'   = 'var(--ifs-danger)'
  '$ifs-error-soft'     = 'var(--ifs-danger-bg)'
  '$ifs-success-border' = 'var(--ifs-success)'
  '$ifs-success-soft'   = 'var(--ifs-success-bg)'
  '$ifs-warning-strong' = 'var(--ifs-warning)'
  '$ifs-warning-border' = 'var(--ifs-warning)'
  '$ifs-warning-soft'   = 'var(--ifs-warning-bg)'
  '$ifs-info-border'    = 'var(--ifs-info)'
  '$ifs-info-soft'      = 'var(--ifs-info-bg)'

  # Borders
  '$ifs-border-medium' = 'var(--ifs-border-subtle)'
  '$ifs-border-soft'   = 'var(--ifs-border-subtle)'

  # On-brand / on-dark
  '$ifs-on-brand-strong' = 'var(--ifs-text-on-brand)'
  '$ifs-on-brand-high'   = 'var(--ifs-text-on-brand)'
  '$ifs-on-brand-muted'  = 'rgba(255, 255, 255, 0.78)'
  '$ifs-on-brand-subtle' = 'rgba(255, 255, 255, 0.7)'
  '$ifs-on-dark-strong'  = 'var(--ifs-text-primary)'
  '$ifs-on-dark-muted'   = 'var(--ifs-text-secondary)'
  '$ifs-on-dark-subtle'  = 'var(--ifs-text-muted)'

  # Easing
  '$ifs-ease-in-out' = 'var(--ifs-ease-emphasized)'
  '$ifs-ease-out'    = 'var(--ifs-ease-standard)'

  # Misc
  '$ifs-accent-soft-contrast' = 'var(--ifs-accent-500)'
}

$files = Get-ChildItem -Path $root -Recurse -Filter '*.scss' |
  Where-Object { $_.FullName -notmatch 'node_modules|dist|\.angular|_tokens\.scss|_mixins\.scss' }

$totalReplacements = 0

foreach ($file in $files) {
  $content = Get-Content -Path $file.FullName -Raw
  $original = $content
  $fileReplacements = 0

  foreach ($key in $replacements.Keys) {
    # Use regex with word boundary after to avoid partial matches
    # Escape the $ at start for regex
    $escaped = [regex]::Escape($key)
    # Match the token only when NOT followed by another alphanumeric or hyphen
    $pattern = $escaped + '(?![a-zA-Z0-9_-])'
    $matches = [regex]::Matches($content, $pattern)
    if ($matches.Count -gt 0) {
      $content = [regex]::Replace($content, $pattern, $replacements[$key])
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
