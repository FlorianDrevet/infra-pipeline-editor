$lines = @{
  'src/Front/src/app/features/project-detail/split-generation-switcher/split-generation-switcher.component.ts'=@(208,291);
  'src/Front/src/app/features/project-detail/project-detail.component.ts'=@(153,324);
  'src/Front/src/app/features/config-detail/add-resource-dialog/add-resource-dialog.component.ts'=@(1139);
  'src/Front/src/app/features/resource-edit/resource-edit.component.ts'=@(1419);
  'src/Front/src/app/features/config-detail/push-to-git-dialog/push-to-git-dialog.component.ts'=@(132);
  'src/Front/src/app/features/config-detail/config-detail.component.ts'=@(200,744,1644);
  'src/Front/src/app/features/resource-edit/add-app-config-key-dialog/add-app-config-key-dialog.component.ts'=@(158,441);
  'src/Front/src/app/features/resource-edit/add-app-setting-dialog/add-app-setting-dialog.component.ts'=@(171,447)
}
foreach ($f in $lines.Keys) {
  $c = Get-Content $f
  foreach ($l in $lines[$f]) { "[$f] L${l}: $($c[$l-1])" }
}
