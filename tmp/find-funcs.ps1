$miss = @(
  @{f='src/Front/src/app/features/project-detail/project-detail.component.ts'; l=153},
  @{f='src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.ts'; l=1139},
  @{f='src/Front/src/app/features/config-detail/config-detail.component.ts'; l=200},
  @{f='src/Front/src/app/features/config-detail/config-detail.component.ts'; l=1644},
  @{f='src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.ts'; l=158},
  @{f='src/Front/src/app/features/resource-edit/add-app-setting-dialog/add-app-setting-dialog.component.ts'; l=171}
)
foreach ($t in $miss) {
  $c = Get-Content $t.f
  $start = [Math]::Max(0, $t.l - 60)
  "=== $($t.f) L$($t.l) ==="
  for ($i = $start; $i -lt $t.l; $i++) {
    $line = $c[$i]
    # Only show lines that look like function declarations or class members
    if ($line -match '\b(function|=>\s*[{(]|\)\s*[:{]|protected|private|public|static)\b' -and $line -notmatch '^\s*(//|\*)') {
      "  L$($i+1): $line"
    }
  }
}
