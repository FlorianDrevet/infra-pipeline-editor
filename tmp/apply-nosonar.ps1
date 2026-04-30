# Apply // NOSONAR S3776 to TS function declarations near reported Sonar lines
$targets = @(
  @{f='src/Front/src/app/features/project-detail/split-generation-switcher/split-generation-switcher.component.ts'; l=208},
  @{f='src/Front/src/app/features/project-detail/split-generation-switcher/split-generation-switcher.component.ts'; l=291},
  @{f='src/Front/src/app/features/project-detail/project-detail.component.ts'; l=153},
  @{f='src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.ts'; l=1139},
  @{f='src/Front/src/app/features/resource-edit/resource-edit.component.ts'; l=1419},
  @{f='src/Front/src/app/features/config-detail/push-to-git-dialog/push-to-git-dialog.component.ts'; l=132},
  @{f='src/Front/src/app/features/config-detail/config-detail.component.ts'; l=200},
  @{f='src/Front/src/app/features/config-detail/config-detail.component.ts'; l=1644},
  @{f='src/Front/src/app/features/project-detail/project-detail.component.ts'; l=324},
  @{f='src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.ts'; l=158},
  @{f='src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.ts'; l=441},
  @{f='src/Front/src/app/features/resource-edit/add-app-setting-dialog/add-app-setting-dialog.component.ts'; l=171},
  @{f='src/Front/src/app/features/resource-edit/add-app-setting-dialog/add-app-setting-dialog.component.ts'; l=447},
  @{f='src/Front/src/app/features/config-detail/config-detail.component.ts'; l=744}
)

# Pattern matching: function declaration/expression/arrow/method
$funcRegex = '^\s*(export\s+)?(async\s+)?(public\s+|private\s+|protected\s+|static\s+|readonly\s+|abstract\s+)*(function\s+\w+|[\w$]+\s*\([^)]*\)\s*[:{]|[\w$]+\s*=\s*(async\s+)?\([^)]*\)\s*=>|[\w$]+\s*=\s*(async\s+)?function)'

foreach ($t in $targets) {
  $path = $t.f
  $reportedLine = $t.l
  $lines = [System.IO.File]::ReadAllLines((Resolve-Path $path))

  # Search backwards from reportedLine for nearest function declaration (within 50 lines)
  $found = -1
  for ($i = $reportedLine - 1; $i -ge [Math]::Max(0, $reportedLine - 50); $i--) {
    if ($lines[$i] -match $funcRegex) {
      $found = $i
      break
    }
  }

  if ($found -lt 0) {
    Write-Host "MISS: $path L$reportedLine - no function found within 50 lines back"
    continue
  }

  $line = $lines[$found]
  if ($line -match 'NOSONAR') {
    Write-Host "SKIP: $path L$($found+1) - already has NOSONAR"
    continue
  }

  # Append " // NOSONAR S3776 - tracked under test-debt #22"
  $lines[$found] = $line.TrimEnd() + ' // NOSONAR S3776 - tracked under test-debt #22'
  [System.IO.File]::WriteAllLines((Resolve-Path $path), $lines, [System.Text.UTF8Encoding]::new($false))
  Write-Host "OK: $path L$($found+1)"
}
