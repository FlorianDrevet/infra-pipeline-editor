$repo = 'FlorianDrevet/infra-pipeline-editor'
$obsolete = @(
  @{ Num=173; Reason="Verifie : src/Api/InfraFlowSculptor.BicepGeneration/BicepGenerationEngine.cs fait desormais 77 lignes (vs 950+ dans le finding). Refactoring pipeline + IR Bicep deja livre via PR #327 (Refacto bicep generation, mergee). Finding obsolete." },
  @{ Num=184; Reason="Verifie : src/Api/InfraFlowSculptor.Application/Common/Behaviors/ValidationBehavior.cs ne contient plus de cast dynamic. La methode privee 'BuildErrorResponse' utilise la reflection sur ErrorOr<T>.op_Implicit avec un commentaire explicite '(Audit APP-004 - 2026-04-23.)'. Finding obsolete." }
)
foreach ($o in $obsolete) {
  Write-Host "Closing #$($o.Num) (obsolete)"
  & gh issue close $o.Num --repo $repo --reason 'completed' --comment $o.Reason
}
"Done"
