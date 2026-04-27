$file = "src\Front\src\app\features\config-detail\add-resource-dialog\add-resource-dialog.component.ts"
$c = Get-Content $file -Raw

# In the onSubmit method, we need to add isExisting: common.isExisting ?? false to each service.create() call
# Pattern: find each create({ block and add isExisting before the closing });
# Strategy: replace each "environmentSettings: this.build...()," with same + isExisting
# More robust: replace the pattern at the end of each create call

# The create calls all follow the same pattern for resources WITH environmentSettings
$c = $c -replace "(await this\.\w+Service\.create\(\{[^}]+)(environmentSettings: this\.build\w+\(\),?)\s*(\}\);)", '$1$2
            isExisting: common.isExisting ?? false,
            $3'

# For UserAssignedIdentity (no environmentSettings): "location: common.location!,\n          });" 
$c = $c -replace "(case ResourceTypeEnum\.UserAssignedIdentity \{[^}]+name: common\.name!,\s+location: common\.location!,)\s*(\}\);)", '$1
            isExisting: common.isExisting ?? false,
            $2'

Set-Content $file $c -NoNewline
Write-Host "Done"
