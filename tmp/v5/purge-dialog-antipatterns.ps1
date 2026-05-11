# Vague 5 — purge anti-patterns from dialog SCSS files in src/Front
# Replaces legacy light/glass/brand-shadow patterns with V2 dark tokens.

$base  = 'C:\Users\flori\RiderProjects\infra-pipeline-editor\src\Front\src\app\features'
$files = Get-ChildItem -Path $base -Recurse -Filter '*-dialog*.scss'
$totalChanges = 0

foreach ($file in $files) {
    $original = Get-Content -Path $file.FullName -Raw
    $content  = $original

    # 1. backdrop-filter / glass mixin → remove
    $content = $content -replace '(?im)^\s*-webkit-backdrop-filter\s*:[^;]+;\s*\r?\n', ''
    $content = $content -replace '(?im)^\s*backdrop-filter\s*:[^;]+;\s*\r?\n', ''
    $content = $content -replace '(?im)^\s*@include\s+ifs-glass\([^)]*\);\s*\r?\n', ''

    # 2. translateY(-Npx) hover → keep transform but neutralise lift
    $content = $content -replace 'translateY\(-\d+(\.\d+)?px\)', 'translateY(0)'

    # 3. transition: all <time> [ease...] → token-based set on background-color, border-color, color
    $content = $content -replace '(?im)transition\s*:\s*all\s+[^;]+;', 'transition: background-color var(--ifs-duration-fast) var(--ifs-ease-standard), border-color var(--ifs-duration-fast) var(--ifs-ease-standard), color var(--ifs-duration-fast) var(--ifs-ease-standard);'

    # 4. White / light gradients → surface-1
    $content = $content -replace 'linear-gradient\(\s*180deg\s*,\s*#fff\s*,\s*#[a-fA-F0-9]{3,8}\s*\)', 'var(--ifs-surface-1)'
    $content = $content -replace 'linear-gradient\(\s*180deg\s*,\s*#[a-fA-F0-9]{3,8}\s*,\s*#[a-fA-F0-9]{3,8}\s*\)', 'var(--ifs-surface-1)'
    $content = $content -replace 'linear-gradient\(\s*180deg\s*,\s*rgba\(\s*255\s*,\s*255\s*,\s*255[^)]*\)\s*,\s*rgba\(\s*255\s*,\s*255\s*,\s*255[^)]*\)\s*\)', 'var(--ifs-surface-1)'
    $content = $content -replace 'linear-gradient\(\s*180deg\s*,\s*rgba\(\s*248\s*,\s*250\s*,\s*252[^)]*\)\s*,\s*rgba\(\s*255\s*,\s*255\s*,\s*255[^)]*\)\s*\)', 'var(--ifs-surface-1)'

    # 5. Brand-tinted shadows → token shadows
    $content = $content -replace 'box-shadow\s*:\s*0\s+\d+px\s+\d+px\s+rgba\(\s*21\s*,\s*101\s*,\s*192\s*,[^)]*\)\s*;', 'box-shadow: var(--ifs-shadow-md);'
    $content = $content -replace 'box-shadow\s*:\s*0\s+\d+px\s+\d+px\s+rgba\(\s*13\s*,\s*47\s*,\s*102\s*,[^)]*\)\s*;', 'box-shadow: var(--ifs-shadow-md);'

    # 6. Brand-blue tints — backgrounds (ascending refinement)
    #    Low alpha → surface-3, medium → info-bg, hover-strong → accent-aware via info-bg
    $content = $content -replace 'background\s*:\s*rgba\(\s*21\s*,\s*101\s*,\s*192\s*,\s*0?\.0[1-4]\)\s*;', 'background: var(--ifs-surface-3);'
    $content = $content -replace 'background\s*:\s*rgba\(\s*21\s*,\s*101\s*,\s*192\s*,\s*0?\.0[5-9]\)\s*;', 'background: var(--ifs-info-bg);'
    $content = $content -replace 'background\s*:\s*rgba\(\s*21\s*,\s*101\s*,\s*192\s*,\s*0?\.1\d?\)\s*;', 'background: var(--ifs-info-bg);'

    # 7. Brand-blue tints — border-color (any alpha) → accent-500
    $content = $content -replace 'border-color\s*:\s*rgba\(\s*21\s*,\s*101\s*,\s*192\s*,[^)]+\)\s*;', 'border-color: var(--ifs-accent-500);'

    # 8. Brand-blue tints — composite border definition
    $content = $content -replace 'border\s*:\s*1px\s+solid\s+rgba\(\s*21\s*,\s*101\s*,\s*192\s*,[^)]+\)\s*;', 'border: 1px solid var(--ifs-border-subtle);'

    # 9. White translucent text fallback
    $content = $content -replace 'rgba\(\s*255\s*,\s*255\s*,\s*255\s*,\s*0?\.[678]\d?\)', 'var(--ifs-text-secondary)'

    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -NoNewline -Encoding UTF8
        $totalChanges++
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host ""
Write-Host "Files updated: $totalChanges / $($files.Count)"
