param(
    [string]$ContainerId,
    [string]$Database = "infraDb"
)

$ErrorActionPreference = "Stop"

if (-not $ContainerId) {
    $containers = docker ps --filter "ancestor=postgres:17.6" --format "{{.ID}} {{.Names}}"
    if (-not $containers) {
        throw "No running postgres:17.6 container was found. Start the Aspire AppHost first or pass -ContainerId explicitly."
    }

    $matches = @($containers)
    if ($matches.Count -gt 1) {
        throw "Multiple PostgreSQL containers were found. Pass -ContainerId explicitly. Found: $($matches -join ', ')"
    }

    $ContainerId = ($matches[0] -split ' ')[0]
}

$password = docker exec $ContainerId sh -lc "printenv POSTGRES_PASSWORD"
if ([string]::IsNullOrWhiteSpace($password)) {
    throw "Could not read POSTGRES_PASSWORD from container '$ContainerId'."
}

$sql = @'
BEGIN;

UPDATE "ProjectRepositories"
SET "ContentKinds" = CASE
    WHEN trim(both ',' from replace(replace(replace("ContentKinds", ',Pipelines', ''), 'Pipelines,', ''), 'Pipelines', '')) = ''
        THEN 'Infrastructure'
    ELSE trim(both ',' from replace(replace(replace("ContentKinds", ',Pipelines', ''), 'Pipelines,', ''), 'Pipelines', ''))
END
WHERE "ContentKinds" LIKE '%Pipelines%';

UPDATE "InfraConfigRepositories"
SET "ContentKinds" = CASE
    WHEN trim(both ',' from replace(replace(replace("ContentKinds", ',Pipelines', ''), 'Pipelines,', ''), 'Pipelines', '')) = ''
        THEN 'Infrastructure'
    ELSE trim(both ',' from replace(replace(replace("ContentKinds", ',Pipelines', ''), 'Pipelines,', ''), 'Pipelines', ''))
END
WHERE "ContentKinds" LIKE '%Pipelines%';

UPDATE "Projects" p
SET "LayoutPreset" = 'SplitInfraCode'
WHERE p."LayoutPreset" = 'AllInOne'
  AND (SELECT count(*) FROM "ProjectRepositories" pr WHERE pr."ProjectId" = p."Id") = 2
  AND EXISTS (
      SELECT 1
      FROM "ProjectRepositories" pr
      WHERE pr."ProjectId" = p."Id"
        AND pr."ContentKinds" LIKE '%Infrastructure%')
  AND EXISTS (
      SELECT 1
      FROM "ProjectRepositories" pr
      WHERE pr."ProjectId" = p."Id"
        AND pr."ContentKinds" LIKE '%ApplicationCode%');

UPDATE "ProjectRepositories" pr
SET "ContentKinds" = 'Infrastructure,ApplicationCode'
WHERE pr."ProjectId" IN (
    SELECT p."Id"
    FROM "Projects" p
    WHERE p."LayoutPreset" = 'AllInOne'
      AND (SELECT count(*) FROM "ProjectRepositories" pr2 WHERE pr2."ProjectId" = p."Id") = 1
)
AND pr."ContentKinds" IN ('Infrastructure', 'ApplicationCode');

COMMIT;

SELECT p."Name" AS "ProjectName", p."LayoutPreset", r."Alias", r."ContentKinds"
FROM "Projects" p
LEFT JOIN "ProjectRepositories" r ON r."ProjectId" = p."Id"
ORDER BY p."Name", r."Alias";
'@

$sql | docker exec -i -e "PGPASSWORD=$password" $ContainerId psql -v ON_ERROR_STOP=1 -h 127.0.0.1 -U postgres -d $Database