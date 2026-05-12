## But principal

Étendre la PR #328 au-dela du lot securite/validation initial avec plusieurs remediations d'audit substantielles et validees: modernisation du rate limiting, sweep DB-001 en deux tranches, reclassement propre de DB-002 comme obsolete apres verification du modele relationnel, suppression du N+1 critique de APP-003 sur les references croisees de configuration, puis premiers splits read-only de DB-003 et correction du cascade-delete DB-006 sur `AppSetting`.

---

## Type de changement

- [x] fix - Correction de bug
- [x] test - Ajout ou modification de tests
- [x] docs - Documentation associee

---

## Changements par couche

### API (InfraFlowSculptor.Api)

- Implementation native ASP.NET Core du rate limiting avec options typees validees au startup, policy globale fixed-window, policy Expensive, et header Retry-After sur 429.
- Ordre du pipeline corrige pour executer UseAuthentication() avant UseRateLimiter().
- Partition authentifiee stabilisee (ClaimConstants.ObjectId, puis ClaimTypes.NameIdentifier, puis repli IP).
- Application coherente de la policy Expensive sur les endpoints lourds config/projet.
- Validation SafeRelativePath.TryNormalize(...) ajoutee sur GetBicepFileContent et GetPipelineFileContent pour rejeter les chemins relatifs dangereux avant tout acces MediatR/blob.

### Application (InfraFlowSculptor.Application)

- ValidationBehavior ignore desormais les queries et ne valide que les requetes implementant ICommandBase.
- CreateProjectCommandValidator est realigne sur la borne canonique 80 pour Project.Name.
- Ajout du validator manquant CreateInfrastructureConfigCommandValidator pour borner InfrastructureConfig.Name a 100 avant l'acces base.
- Ajout de CreateResourceGroupCommandValidator pour borner ResourceGroup.Name a 90 avant l'acces base.
- `ListCrossConfigReferencesQueryHandler` ne charge plus les configs cibles via `GetByIdAsync()` dans une boucle: il utilise desormais un chargement batch de resumes (`GetConfigSummariesByIdsAsync`) puis une jointure memoire avec les metadonnees de ressources deja batchees.
- `ProjectAccessService.VerifyReadAccessAsync(...)` utilise desormais un lookup projet dedie no-tracking (`GetByIdWithMembersReadOnlyAsync`) au lieu de reemployer le chargement tracke partage avec les flows write/owner.
- `GetResourceGroupQueryHandler` et `ListResourceGroupResourcesQueryHandler` utilisent desormais un lookup `ResourceGroup` dedie no-tracking (`GetByIdReadOnlyAsync`) au lieu du path tracke partage avec les handlers de commande.

### Contracts (InfraFlowSculptor.Contracts)

- EnumValidation accepte desormais les noms d'enum fournis comme string de facon case-insensitive.
- CreateProjectRequest explicite desormais Name <= 80 et Description <= 500.
- CreateInfrastructureConfigRequest explicite desormais Name <= 100.
- CreateResourceGroupRequest explicite desormais Name <= 90.

### Infrastructure / base de donnees (InfraFlowSculptor.Infrastructure)

- DB-001 tranche coeur: ajout de HasMaxLength(...) sur les colonnes persisted suivantes :
  - Projects.Name
  - Projects.DefaultNamingTemplate
  - ProjectEnvironments.Name, ShortName, Prefix, Suffix
  - InfrastructureConfigs.Name
  - InfrastructureConfigs.DefaultNamingTemplate
  - AzureResource.Name
  - ProjectResourceNamingTemplates.Template
  - ResourceNamingTemplates.Template
- DB-001 tranche ResourceGroup: ResourceGroup.Name est desormais contraint a varchar(90), aligne sur la regle Azure officielle Microsoft.Resources/resourcegroups (1-90).
- Generation des migrations EF Core AddCoreStringLengthConstraints puis AddResourceGroupNameLengthConstraint pour materialiser ces contraintes en base.
- Le tail restant de DB-001 se resserre maintenant sur ParameterDefinition, dont la matrice canonique Name / Type / DefaultValue n'est pas encore tranchee.
- Ajout de `IInfrastructureConfigRepository.GetConfigSummariesByIdsAsync(...)` pour supporter les queries read-only qui doivent resoudre plusieurs noms de configs sans charger les agregats complets un par un.
- Ajout de `IProjectRepository.GetByIdWithMembersReadOnlyAsync(...)` pour isoler un premier slice `DB-003` sans casser les flows projet qui doivent encore muter un aggregate tracke.
- Ajout de `IResourceGroupRepository.GetByIdReadOnlyAsync(...)` pour isoler un deuxieme slice `DB-003` cote queries `ResourceGroup`, sans toucher au `GetByIdAsync(...)` tracke encore partage par les commandes.
- DB-006 corrige : `AppSetting.SourceResourceId` passe de `Cascade` a `SetNull`, aligne sur la semantique deja retenue pour `KeyVaultResourceId`, afin de conserver l'app setting quand la ressource source referencee disparait.
- Generation de la migration EF Core `SetNullOnAppSettingSourceResource` pour materialiser ce changement de FK en base.

### Tests

- Projet tests/InfraFlowSculptor.Api.Tests utilise pour verrouiller le rate limiting et la validation des endpoints /{*filePath}.
- Ajout de tests cibles sur ValidationBehavior pour prouver que les commandes invalides sont bloquees et que les queries invalides passent au handler.
- Extension des tests EnumValidation pour les variantes de casse sur les noms d'enum.
- Ajout de tests de metadonnees EF (CoreStringLengthConfigurationTests) pour verrouiller la matrice de longueurs sur le noyau DB-001, y compris ResourceGroup.Name = 90.
- Ajout de IndexCoverageConfigurationTests pour verifier DB-002 contre le modele EF relationnel Npgsql et prouver que la couverture d'index critique existe deja sans migration supplementaire.
- Ajout de tests cibles pour l'alignement CreateProject, CreateInfrastructureConfig et CreateResourceGroup cote validators/contrats.
- Ajout de `ListCrossConfigReferencesQueryHandlerTests` pour verrouiller le chargement batch des configs cibles cote application.
- Ajout d'une couverture repository ciblee dans `InfrastructureConfigRepositoryTests` pour `GetConfigSummariesByIdsAsync(...)`.
- Ajout de `ProjectAccessServiceTests` pour verrouiller l'usage du nouveau lookup projet read-only dans `VerifyReadAccessAsync(...)`.
- Extension de `ProjectRepositoryTests` avec un test dedie qui verifie que `GetByIdWithMembersReadOnlyAsync(...)` renvoie un aggregate detache du change tracker.
- Ajout de `ListResourceGroupResourcesQueryHandlerTests` pour verrouiller l'usage du nouveau lookup `ResourceGroup` read-only sur le flow de lecture pur.
- Extension de `GetResourceGroupQueryHandlerTests` et `ResourceGroupRepositoryTests` pour verrouiller `GetByIdReadOnlyAsync(...)` et son caractere detache.
- Ajout de `DeleteBehaviorConfigurationTests` pour verifier sur le modele relationnel EF que `AppSetting.SourceResourceId` et `KeyVaultResourceId` utilisent bien `SetNull`.

### Documentation / audit

- Mise a jour de audits/triage-2026-05-12.md : #164, #165, #201 et #207 restent en FIXED-IN-PR, et #166 documente desormais que ResourceGroup est couvert et que le reliquat porte sur ParameterDefinition.
- Mise a jour de audits/triage-2026-05-12.md : #167 est reclasse CLOSED-OBSOLETE apres verification ciblee du modele relationnel (indexes explicites, indexes FK/conventions, et composite unique deja presents).
- Mise a jour de audits/triage-2026-05-12.md : #170 passe en FIXED-IN-PR apres suppression du N+1 de `ListCrossConfigReferencesQueryHandler`.
- Mise a jour de audits/triage-2026-05-12.md : #179 est reclasse CLOSED-OBSOLETE, car `GetConfiguredEnvironmentsByResourceGroupAsync(...)` interroge deja `vw_ResourceEnvironmentEntries` en une seule requete au lieu des 16 acces sequentiels cites par l'audit.
- Mise a jour de audits/triage-2026-05-12.md : #180 est reclasse CLOSED-OBSOLETE, car l'index unique composite `RoleAssignment(SourceResourceId, TargetResourceId, UserAssignedIdentityId, RoleDefinitionId)` existe deja dans le modele relationnel courant et reste verrouille par `IndexCoverageConfigurationTests`.
- Mise a jour de audits/triage-2026-05-12.md : #178 reste ouvert mais documente maintenant deux remediations sures no-tracking (`ProjectAccessService.VerifyReadAccessAsync(...)` et les queries `ResourceGroup`) sans casser les flows de commande restes trackes par design.
- Mise a jour de audits/triage-2026-05-12.md : #181 passe en FIXED-IN-PR apres bascule de `AppSetting.SourceResourceId` vers `SetNull`, couverture metadata relationnelle et migration dediee.
- Memoire projet mise a jour avec la matrice canonique de longueurs, la borne officielle ResourceGroup.Name = 90, le pattern CQRS de chargement batch par resumes, les splits read-only projet/resource-group, et les migrations associees.

---

## Base de donnees

- [x] Cette PR inclut des migrations EF Core
- [ ] Aucune migration necessaire

Migrations :
- 20260512091902_AddCoreStringLengthConstraints
- 20260512095600_AddResourceGroupNameLengthConstraint
- 20260512121558_SetNullOnAppSettingSourceResource

---

## Validation

- [x] dotnet build .\InfraFlowSculptor.slnx
- [x] dotnet test .\InfraFlowSculptor.slnx (1985 total, 0 failed, 9 skipped)
- [x] Tests cibles verts pour GeneratedFilePathValidationTests, ValidationBehaviorTests, EnumValidationTests, CoreStringLengthConfigurationTests, IndexCoverageConfigurationTests, DeleteBehaviorConfigurationTests, CreateProjectCommandValidatorTests, CreateProjectRequestTests, CreateInfrastructureConfigCommandValidatorTests, CreateInfrastructureConfigRequestTests, CreateResourceGroupCommandValidatorTests, CreateResourceGroupRequestTests, ListCrossConfigReferencesQueryHandlerTests, `ProjectAccessServiceTests`, `ProjectRepositoryTests`, `GetResourceGroupQueryHandlerTests`, `ListResourceGroupResourcesQueryHandlerTests`, `ResourceGroupRepositoryTests`, et le test repository cible de `GetConfigSummariesByIdsAsync(...)`

Note : les warnings NU1904 sur Microsoft.AspNetCore.DataProtection restent inchanges sur la branche et relevent d'une mise a niveau de dependance dediee.

---

## Issues / tickets lies

Closes #164
Closes #165
Closes #167
Closes #170
Closes #179
Closes #180
Closes #181
Closes #201
Closes #207
Partial progress on #166
Partial progress on #178
