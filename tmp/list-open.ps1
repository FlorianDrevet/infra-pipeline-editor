$data = gh issue list --repo FlorianDrevet/infra-pipeline-editor --state open --limit 200 --json number,title,labels | ConvertFrom-Json
$rows = foreach ($i in $data) {
  $sev = ($i.labels.name | Where-Object { $_ -like 'severity:*' }) -replace 'severity:\s*',''
  $area = ($i.labels.name | Where-Object { $_ -like 'area:*' }) -replace 'area:\s*',''
  $type = ($i.labels.name | Where-Object { $_ -like 'type:*' }) -replace 'type:\s*',''
  $sevStr = if ($sev -is [array]) { $sev -join ',' } else { [string]$sev }
  $areaStr = if ($area -is [array]) { $area -join ',' } else { [string]$area }
  $typeStr = if ($type -is [array]) { $type -join ',' } else { [string]$type }
  "{0,4} | {1,-9} | {2,-15} | {3,-12} | {4}" -f $i.number, $sevStr, $areaStr, $typeStr, $i.title
}
$rows | Sort-Object | Out-File -Encoding utf8 tmp\open-now.txt
"$($rows.Count) issues"
