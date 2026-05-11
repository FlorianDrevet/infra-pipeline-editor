<#
.SYNOPSIS
  Fourth pass — fixes remaining rgba(13,...) and other last holdouts
#>

$root = 'c:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src'

$replacements = [ordered]@{
  # rgba(13, 101, 192, ...) — old brand blue
  'rgba(13, 101, 192, 0.55)' = 'var(--ifs-accent-500)'
  'rgba(13, 101, 192, 0.38)' = 'var(--ifs-accent-500)'
  'rgba(13, 101, 192, 0.3)'  = 'var(--ifs-border-strong)'
  'rgba(13, 101, 192, 0.25)' = 'var(--ifs-border-strong)'
  'rgba(13, 101, 192, 0.2)'  = 'var(--ifs-border-subtle)'
  'rgba(13, 101, 192, 0.18)' = 'var(--ifs-border-subtle)'
  'rgba(13, 101, 192, 0.16)' = 'rgba(58, 180, 217, 0.15)'
  'rgba(13, 101, 192, 0.15)' = 'rgba(58, 180, 217, 0.15)'
  'rgba(13, 101, 192, 0.07)' = 'rgba(58, 180, 217, 0.08)'
  'rgba(13, 101, 192, 0.03)' = 'rgba(58, 180, 217, 0.04)'

  # rgba(13, 47, 102, ...) — old dark navy
  'rgba(13, 47, 102, 0.55)' = 'var(--ifs-text-secondary)'
  'rgba(13, 47, 102, 0.6)'  = 'var(--ifs-text-secondary)'
  'rgba(13, 47, 102, 0.18)' = 'rgba(0, 0, 0, 0.25)'
  'rgba(13, 47, 102, 0.08)' = 'var(--ifs-border-subtle)'
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
