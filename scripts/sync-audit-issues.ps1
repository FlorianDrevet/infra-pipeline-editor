[CmdletBinding()]
param(
    [string]$AuditFile,
    [string]$Repo,
    [string]$ConfigPath = '.github/audit/config.json',
    [switch]$EnsureLabels,
    [switch]$ListLabels,
    [switch]$WhatIf
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-GhJson {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $output = & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI command failed: gh $($Arguments -join ' ')"
    }

    if ([string]::IsNullOrWhiteSpace($output)) {
        return $null
    }

    return $output | ConvertFrom-Json -Depth 20
}

function Invoke-Gh {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    if ($WhatIf) {
        Write-Host ("[WhatIf] gh {0}" -f ($Arguments -join ' '))
        return
    }

    & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI command failed: gh $($Arguments -join ' ')"
    }
}

function Get-Config {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Config file not found: $Path"
    }

    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -Depth 20
}

function Get-AuditStamp {
    param([string]$Path)

    $leaf = Split-Path -Path $Path -Leaf
    $match = [regex]::Match($leaf, '(?<day>\d{2})-(?<month>\d{2})-(?<year>\d{4})')
    if ($match.Success) {
        $dateText = '{0}-{1}-{2}' -f $match.Groups['year'].Value, $match.Groups['month'].Value, $match.Groups['day'].Value
        $date = [datetime]::ParseExact(
            $dateText,
            'yyyy-MM-dd',
            [System.Globalization.CultureInfo]::InvariantCulture
        )
    }
    else {
        $date = Get-Date
    }

    [pscustomobject]@{
        Date = $date
        Label = 'audit: {0:yyyy-MM}' -f $date
        Display = '{0:yyyy-MM}' -f $date
        FileName = $leaf
        RelativeSource = ('audits/{0}' -f $leaf)
    }
}

function Convert-SeverityToLabel {
    param([string]$RawSeverity)

    $normalized = $RawSeverity.ToLowerInvariant()
    if ($normalized.Contains('crit')) { return 'severity: critical' }
    if ($normalized.Contains('haut') -or $normalized.Contains('high')) { return 'severity: high' }
    if ($normalized.Contains('moy')) { return 'severity: medium' }
    return 'severity: low'
}

function Get-SectionSeverityLabel {
    param(
        [int]$Index,
        [System.Text.RegularExpressions.MatchCollection]$SectionMatches
    )

    $sectionLabel = 'severity: low'
    foreach ($sectionMatch in $SectionMatches) {
        if ($sectionMatch.Index -gt $Index) {
            break
        }

        $bucket = $sectionMatch.Groups['bucket'].Value.ToLowerInvariant()
        switch ($bucket) {
            'critiques' { $sectionLabel = 'severity: critical' }
            'hauts' { $sectionLabel = 'severity: high' }
            'moyens' { $sectionLabel = 'severity: medium' }
            'bas' { $sectionLabel = 'severity: low' }
        }
    }

    return $sectionLabel
}

function Get-TypeLabel {
    param(
        [string]$Text,
        [object]$Config
    )

    $normalizedText = $Text.ToLowerInvariant()
    foreach ($heuristic in $Config.typeHeuristics) {
        foreach ($pattern in $heuristic.patterns) {
            if ($normalizedText -match $pattern) {
                return [string]$heuristic.label
            }
        }
    }

    return 'type: tech-debt'
}

function Get-PhaseMapping {
    param(
        [string]$Content,
        [object]$Config
    )

    $mapping = @{}
    $currentPhase = $null

    foreach ($line in ($Content -split "`r?`n")) {
        $phaseMatch = [regex]::Match($line, '^###\s+Phase\s+(?<number>\d+)')
        if ($phaseMatch.Success) {
            $phaseNumber = $phaseMatch.Groups['number'].Value
            if ($Config.phaseLabelsByNumber.PSObject.Properties.Name -contains $phaseNumber) {
                $currentPhase = [string]$Config.phaseLabelsByNumber.$phaseNumber
            }
            else {
                $currentPhase = $null
            }

            continue
        }

        if (-not $currentPhase) {
            continue
        }

        if ($line -notmatch '^\|') {
            continue
        }

        $ids = [regex]::Matches($line, '(?<id>[A-Z]+-\d{3})')
        foreach ($idMatch in $ids) {
            $mapping[$idMatch.Groups['id'].Value] = $currentPhase
        }
    }

    return $mapping
}

function Get-AuditFindings {
    param(
        [string]$Content,
        [object]$Config,
        [hashtable]$PhaseMapping,
        [object]$AuditStamp
    )

    $results = @()
    $sectionMatches = [regex]::Matches($Content, '(?mi)^##\s+.*SECTION\s+\d+\s+[—-]\s+FINDINGS\s+(?<bucket>CRITIQUES|HAUTS|MOYENS|BAS)')
    $matches = [regex]::Matches(
        $Content,
        '(?ms)^###\s+(?<id>[A-Z]+-\d{3})\s+[—-]\s+(?<title>.+?)\r?\n(?<body>.*?)(?=^###\s+[A-Z]+-\d{3}\s+[—-]|^##\s+|\z)'
    )

    foreach ($match in $matches) {
        $findingId = $match.Groups['id'].Value.Trim()
        $title = $match.Groups['title'].Value.Trim()
        $body = $match.Groups['body'].Value.Trim()

        $severityMatch = [regex]::Match($body, '\*\*S[ée]v[ée]rit[ée]\s*:\*\*\s*(?<value>.+)')
        $severityLabel = if ($severityMatch.Success) {
            Convert-SeverityToLabel -RawSeverity $severityMatch.Groups['value'].Value.Trim()
        }
        else {
            Get-SectionSeverityLabel -Index $match.Index -SectionMatches $sectionMatches
        }

        $severityRaw = if ($severityMatch.Success) { $severityMatch.Groups['value'].Value.Trim() } else { $severityLabel }

        $prefix = ($findingId -split '-')[0]
        $areaLabel = if ($Config.areaLabelsByPrefix.PSObject.Properties.Name -contains $prefix) {
            [string]$Config.areaLabelsByPrefix.$prefix
        }
        else {
            'area: application'
        }

        $typeLabel = Get-TypeLabel -Text ("{0}`n{1}" -f $title, $body) -Config $Config
        $phaseLabel = if ($PhaseMapping.ContainsKey($findingId)) { [string]$PhaseMapping[$findingId] } else { 'phase: 7-quality' }

        $constat = [regex]::Match($body, '\*\*Constat\s*:\*\*\s*(?<value>.+)')
        $risk = [regex]::Match($body, '\*\*Risque\s*:\*\*\s*(?<value>.+)')
        $recommendation = [regex]::Match($body, '\*\*Recommandation\s*:\*\*\s*(?<value>.*)', [System.Text.RegularExpressions.RegexOptions]::Singleline)

        $summaryText = if ($constat.Success) { $constat.Groups['value'].Value.Trim() } else { $body }
        $riskText = if ($risk.Success) { $risk.Groups['value'].Value.Trim() } else { 'Risk not explicitly documented in audit block.' }
        $recommendationText = if ($recommendation.Success) { $recommendation.Groups['value'].Value.Trim() } else { 'Review the full audit entry for remediation details.' }

        $results += [pscustomobject]@{
            Id = $findingId
            Title = $title
            Summary = $summaryText
            Risk = $riskText
            Recommendation = $recommendationText
            SeverityLabel = $severityLabel
            SeverityRaw = $severityRaw
            AreaLabel = $areaLabel
            TypeLabel = $typeLabel
            PhaseLabel = $phaseLabel
            AuditLabel = [string]$AuditStamp.Label
            AuditSource = [string]$AuditStamp.RelativeSource
            AuditFileName = [string]$AuditStamp.FileName
        }
    }

    return $results
}

function Get-ExistingLabels {
    param([string]$Repository)

    $response = Invoke-GhJson -Arguments @('label', 'list', '--repo', $Repository, '--limit', '200', '--json', 'name,color,description')
    $map = @{}
    foreach ($label in @($response)) {
        $map[[string]$label.name] = $label
    }

    return $map
}

function Ensure-Label {
    param(
        [string]$Repository,
        [string]$Name,
        [string]$Color,
        [string]$Description,
        [hashtable]$ExistingLabels
    )

    if ($ExistingLabels.ContainsKey($Name)) {
        return
    }

    Invoke-Gh -Arguments @('label', 'create', $Name, '--repo', $Repository, '--color', $Color, '--description', $Description)
    $ExistingLabels[$Name] = [pscustomobject]@{ name = $Name; color = $Color; description = $Description }
}

function Get-AuditIssues {
    param([string]$Repository)

    $issues = Invoke-GhJson -Arguments @('issue', 'list', '--repo', $Repository, '--state', 'all', '--limit', '500', '--json', 'number,title,body,state,labels,url')
    $auditIssues = @{}

    foreach ($issue in @($issues)) {
        $body = [string]($issue.body ?? '')
        $match = [regex]::Match($body, '<!--\s*audit-finding-id:\s*(?<id>[A-Z]+-\d{3})\s*-->')
        if (-not $match.Success) {
            continue
        }

        $auditIssues[$match.Groups['id'].Value] = $issue
    }

    return $auditIssues
}

function Get-CurrentIssueLabelNames {
    param([object]$Issue)

    $names = New-Object System.Collections.Generic.List[string]
    foreach ($label in @($Issue.labels)) {
        $names.Add([string]$label.name)
    }

    return $names
}

function New-IssueBody {
    param(
        [object]$Finding,
        [string]$FirstSeen,
        [string]$LastSeen
    )

    return @"
## Audit finding

- Finding ID: $($Finding.Id)
- Severity: $($Finding.SeverityLabel)
- Area: $($Finding.AreaLabel)
- Type: $($Finding.TypeLabel)
- Phase: $($Finding.PhaseLabel)
- First seen: $FirstSeen
- Last seen: $LastSeen
- Source audit: $($Finding.AuditSource)

## Summary

$($Finding.Summary)

## Risk

$($Finding.Risk)

## Recommendation

$($Finding.Recommendation)

<!-- audit-finding-id: $($Finding.Id) -->
<!-- audit-source: $($Finding.AuditSource) -->
<!-- audit-first-seen: $FirstSeen -->
<!-- audit-last-seen: $LastSeen -->
"@
}

function Get-FirstSeen {
    param(
        [string]$IssueBody,
        [string]$Fallback
    )

    $match = [regex]::Match($IssueBody, '<!--\s*audit-first-seen:\s*(?<value>[^>]+)-->')
    if ($match.Success) {
        return $match.Groups['value'].Value.Trim()
    }

    return $Fallback
}

function Write-BodyToTempFile {
    param([string]$Body)

    $path = [System.IO.Path]::GetTempFileName()
    Set-Content -LiteralPath $path -Value $Body -Encoding UTF8
    return $path
}

function Sync-FindingIssue {
    param(
        [object]$Finding,
        [hashtable]$ExistingIssues,
        [string]$Repository
    )

    $desiredLabels = [System.Collections.Generic.List[string]]::new()
    $desiredLabels.Add('audit')
    $desiredLabels.Add($Finding.AuditLabel)
    $desiredLabels.Add($Finding.SeverityLabel)
    $desiredLabels.Add($Finding.AreaLabel)
    $desiredLabels.Add($Finding.TypeLabel)
    $desiredLabels.Add($Finding.PhaseLabel)

    $title = '[AUDIT][{0}] {1}' -f $Finding.Id, $Finding.Title

    if (-not $ExistingIssues.ContainsKey($Finding.Id)) {
        $desiredLabels.Add('status: new')
        $body = New-IssueBody -Finding $Finding -FirstSeen $Finding.AuditFileName -LastSeen $Finding.AuditFileName
        $bodyFile = Write-BodyToTempFile -Body $body
        try {
            $arguments = @('issue', 'create', '--repo', $Repository, '--title', $title, '--body-file', $bodyFile)
            foreach ($label in $desiredLabels) {
                $arguments += @('--label', $label)
            }

            Invoke-Gh -Arguments $arguments
        }
        finally {
            Remove-Item -LiteralPath $bodyFile -ErrorAction SilentlyContinue
        }

        return
    }

    $issue = $ExistingIssues[$Finding.Id]
    $existingLabels = Get-CurrentIssueLabelNames -Issue $issue
    $firstSeen = Get-FirstSeen -IssueBody ([string]$issue.body) -Fallback $Finding.AuditFileName
    $body = New-IssueBody -Finding $Finding -FirstSeen $firstSeen -LastSeen $Finding.AuditFileName
    $bodyFile = Write-BodyToTempFile -Body $body
    try {
        if ([string]$issue.state -eq 'CLOSED') {
            Invoke-Gh -Arguments @('issue', 'reopen', ([string]$issue.number), '--repo', $Repository)
            if ($existingLabels -notcontains 'status: new') {
                $desiredLabels.Add('status: new')
            }
        }

        $removeLabels = [System.Collections.Generic.List[string]]::new()
        foreach ($label in $existingLabels) {
            if ($label -eq 'status: new') {
                continue
            }

            if ($label -match '^audit:\s+\d{4}-\d{2}$') {
                $removeLabels.Add($label)
                continue
            }
        }

        $arguments = @('issue', 'edit', ([string]$issue.number), '--repo', $Repository, '--title', $title, '--body-file', $bodyFile)
        foreach ($label in $desiredLabels) {
            $arguments += @('--add-label', $label)
        }

        if ($existingLabels -contains 'status: new' -and [string]$issue.state -ne 'CLOSED') {
            $arguments += @('--remove-label', 'status: new')
        }

        foreach ($label in $removeLabels) {
            $arguments += @('--remove-label', $label)
        }

        Invoke-Gh -Arguments $arguments
    }
    finally {
        Remove-Item -LiteralPath $bodyFile -ErrorAction SilentlyContinue
    }
}

function Close-ResolvedIssues {
    param(
        [hashtable]$ExistingIssues,
        [System.Collections.Generic.HashSet[string]]$CurrentFindingIds,
        [string]$Repository,
        [string]$AuditFileName
    )

    foreach ($entry in $ExistingIssues.GetEnumerator()) {
        $findingId = [string]$entry.Key
        $issue = $entry.Value
        if ([string]$issue.state -ne 'OPEN') {
            continue
        }

        if ($CurrentFindingIds.Contains($findingId)) {
            continue
        }

        Invoke-Gh -Arguments @('issue', 'comment', ([string]$issue.number), '--repo', $Repository, '--body', ("Closed automatically: finding {0} is no longer present in {1}." -f $findingId, $AuditFileName))
        Invoke-Gh -Arguments @('issue', 'close', ([string]$issue.number), '--repo', $Repository)
    }
}

$resolvedConfigPath = Resolve-Path -LiteralPath $ConfigPath
$config = Get-Config -Path $resolvedConfigPath

if (-not $Repo) {
    $Repo = [string]$config.defaultRepository
}

$existingLabels = Get-ExistingLabels -Repository $Repo

if ($ListLabels) {
    Write-Host ("Repository labels for {0}:" -f $Repo)
    foreach ($labelName in ($existingLabels.Keys | Sort-Object)) {
        Write-Host ("- {0}" -f $labelName)
    }

    Write-Host ''
    Write-Host 'Configured audit labels:'
    foreach ($label in @($config.requiredLabels)) {
        Write-Host ("- {0}" -f $label.name)
    }

    return
}

if (-not $AuditFile) {
    throw 'AuditFile is required unless -ListLabels is used.'
}

$resolvedAuditFile = Resolve-Path -LiteralPath $AuditFile
$auditStamp = Get-AuditStamp -Path $resolvedAuditFile
$auditContent = Get-Content -LiteralPath $resolvedAuditFile -Raw

if ($EnsureLabels) {
    foreach ($label in @($config.requiredLabels)) {
        Ensure-Label -Repository $Repo -Name ([string]$label.name) -Color ([string]$label.color) -Description ([string]$label.description) -ExistingLabels $existingLabels
    }

    $monthlyDescription = [string]::Format([string]$config.auditLabelDescriptionTemplate, $auditStamp.Display)
    Ensure-Label -Repository $Repo -Name $auditStamp.Label -Color ([string]$config.auditLabelColor) -Description $monthlyDescription -ExistingLabels $existingLabels
}

$phaseMapping = Get-PhaseMapping -Content $auditContent -Config $config
$findings = Get-AuditFindings -Content $auditContent -Config $config -PhaseMapping $phaseMapping -AuditStamp $auditStamp

if (-not $findings -or $findings.Count -eq 0) {
    throw "No findings were parsed from audit file: $resolvedAuditFile"
}

$existingIssues = Get-AuditIssues -Repository $Repo
$currentIds = [System.Collections.Generic.HashSet[string]]::new()

foreach ($finding in $findings) {
    $null = $currentIds.Add([string]$finding.Id)
    Sync-FindingIssue -Finding $finding -ExistingIssues $existingIssues -Repository $Repo
}

Close-ResolvedIssues -ExistingIssues $existingIssues -CurrentFindingIds $currentIds -Repository $Repo -AuditFileName $auditStamp.FileName

Write-Host ("Synchronized {0} audit findings against {1}." -f $findings.Count, $Repo)