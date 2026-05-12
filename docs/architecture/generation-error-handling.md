# GEN-006 — Normalisation de la surface d'erreur de generation

## Contexte

Le slice generation du depot expose encore une surface d'erreur heterogene :

- la couche Application retourne des `ErrorOr<>` sur les handlers de cas d'usage ;
- `BicepGenerationEngine`, `PipelineGenerationEngine` et `AppPipelineGenerationEngine` retournent des resultats bruts ;
- plusieurs erreurs attendues de generation restent exprimees par `ArgumentException`, `InvalidOperationException` ou `NotSupportedException` ;
- le handler global API convertit ensuite toute exception non attrapee en `500` generique avec le detail `An error occurred.`.

Autrement dit, un echec de generation attendu n'est pas encore distingue proprement d'un vrai bug d'implementation au niveau de la frontiere Application/API.

## Constat sur la branche courante

Le constat est confirme par le code actuellement en place :

- `GenerateBicepCommandHandler` et `GeneratePipelineCommandHandler` exposent `ErrorOr<>`, mais appellent des engines qui peuvent encore lever des exceptions ;
- `AppPipelineGenerationEngine` valide le mode de deploiement via `ArgumentException` et l'absence de generateur via `InvalidOperationException` ;
- `ModuleBuildStage` leve `NotSupportedException` quand aucun generateur Bicep n'est enregistre pour une ressource ;
- les tests existants verrouillent encore ces contrats par exceptions sur le slice generation.

Le finding `GEN-006` reste donc valide, mais il ne releve pas d'un simple sweep mecanique. Il demande une decision claire sur la frontiere a normaliser.

## Decision recommandee

La normalisation doit se faire **a la frontiere des engines**, pas en poussant `ErrorOr<>` dans chaque stage, generateur ou helper interne.

### Regle cible

- les **erreurs attendues de generation** doivent etre transformees en erreurs typees a la sortie des engines ;
- les **violations d'invariants internes** et les **bugs de programmation** peuvent rester des exceptions.

### Pourquoi cette frontiere

- elle limite le blast radius sur le pipeline Bicep et les generateurs existants ;
- elle garde les stages simples et synchrones ;
- elle permet a la couche Application de retourner des erreurs explicites au lieu d'un `500` generique ;
- elle evite d'introduire `ErrorOr<>` dans tout le graphe interne de generation sans valeur immediate.

## Plus petit plan d'implementation credible

1. Definir une petite taxonomie d'erreurs de generation partagees pour les cas deja visibles : mode de deploiement invalide, generateur absent/non supporte, echec d'assemblage attendu.
2. Ajouter une frontiere de normalisation au niveau des engines de generation pour convertir les erreurs attendues en `ErrorOr<>` sans modifier d'abord chaque stage/generateur.
3. Basculer `GenerateBicepCommandHandler` et `GeneratePipelineCommandHandler` sur cette surface typee et propager ces erreurs jusqu'a l'API.
4. Mettre a jour les tests qui verrouillent aujourd'hui les exceptions attendues afin de verrouiller les erreurs typees a la place.
5. Ne traiter qu'ensuite les reliquats eventuels d'exceptions attendues encore laches hors frontiere engine.

## Hors scope de la premiere tranche

- rewriter chaque stage Bicep en `ErrorOr<>` ;
- changer tout le pipeline infra en mode resultat monadique ;
- revoir le handler global API pour les exceptions purement inattendues.

## Effet attendu

Une fois cette decision appliquee, les erreurs de generation attendues ne tomberont plus dans le `500` generique de l'API, tout en laissant les vraies erreurs internes continuer a remonter comme des fautes d'implementation.