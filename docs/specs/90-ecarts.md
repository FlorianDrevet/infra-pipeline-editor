# 90 — Écarts et dette fonctionnelle (consolidation)

> Statut : v0 (2026-10-03). Consolide les écarts `EC-*` des documents 02 à 19. Le détail et le
> contexte de chaque écart sont dans le document cité. Les identifiants `D*` / `F*` renvoient à
> `docs/stabilization/feature-map.md`.

## 1. Synthèse : ce qui compte le plus

Les écarts se rangent en cinq familles. Pour une reconstruction, ce sont autant de règles à poser
dès la spec cible.

### 1.1 L'écran promet, la génération ne suit pas (données saisies perdues ou fausses)

| Écart | Effet pour l'utilisateur |
|---|---|
| EC-APP-01 / EC-GEN-04 (D07) | Les clés App Configuration saisies ne sont jamais déployées. |
| EC-NOM-01 / EC-GEN-02 (D05) | Le nom affiché et « validé » n'est pas celui du Bicep ; il peut être invalide pour Azure. |
| EC-NET-06 (D08) | « Valider le DNS » ne vérifie rien ; la liaison générée peut échouer au déploiement. |
| EC-NET-01 (D11) | Une ressource privatisée sans Private Endpoint devient injoignable. |
| EC-NET-03 | Le mode DNS du Private Endpoint est ignoré : pas de zone DNS privée. |
| EC-CFG-01 / EC-GEN-05 (F15) | La plupart des références cross-config sont ignorées sans message. |
| EC-CAT-03 / EC-GEN-03 (D06) | Les réglages par environnement de DocumentIntelligence sont perdus. |
| EC-CAT-01 | La politique d'éviction Redis n'est pas générée. |
| EC-APP-07 | Le mode `Combined` ne combine rien. |
| EC-UI-01 | Les paramètres de projet « à venir » ne stockent rien. |

### 1.2 Accès et isolation

| Écart | Effet |
|---|---|
| EC-ACC-01, EC-APP-05 | Plusieurs lectures ne vérifient pas que l'utilisateur appartient au projet. |
| EC-RBA-01 | La cible d'une attribution de rôle peut appartenir à un autre projet. |
| EC-ACC-02 | Tout utilisateur peut lister tous les utilisateurs. |
| EC-ACC-03 | Le dernier Owner peut disparaître. |
| EC-DEP-04 | Droits incohérents entre création d'une configuration et de ses dépôts. |

### 1.3 Livraison Azure DevOps incomplète

| Écart | Effet |
|---|---|
| EC-PIP-01 | Les approbations d'environnement ne sont pas créées. |
| EC-PIP-02 | Les service connections doivent être créées à la main. |
| EC-PIP-03 | Les variables secrètes sont créées vides. |
| EC-PIP-04 | Le bootstrap ne met jamais à jour l'existant. |
| EC-GIT-01 (D12) | Le bouton de push de l'en-tête ne pousse que le Bicep. |
| EC-GIT-02 | Pas de pull request après le push. |
| EC-DEP-03 / EC-PIP-05 / EC-GIT-04 | Le sous-chemin de dépôt n'est jamais honoré. |
| EC-DEP-06 / EC-PIP-06 / EC-GIT-05 | MultiRepo jamais validé contre un vrai fournisseur. |

### 1.4 Contrôles absents ou trompeurs

| Écart | Effet |
|---|---|
| EC-NOM-02 / EC-DIA-04 | La disponibilité d'un nom est testée par DNS public, pas par l'API Azure. |
| EC-CAT-02 | Aucun contrôle des CIDR (chevauchement, inclusion). |
| EC-RES-01 | Noms logiques en double acceptés. |
| EC-RES-05 | Supprimer une ressource référencée ne prévient de rien. |
| EC-DIA-02 / EC-DIA-03 | Contrôles éclatés entre écran et serveur, plusieurs contrôles manquants. |
| EC-PRJ-02 (F01) | Le jeton git n'est pas testé à la création par l'assistant. |

### 1.5 Code mort ou à moitié construit

| Écart | Objet |
|---|---|
| EC-NET-07 (D04) | Profil réseau V2 (décision : `cut`). |
| EC-RES-02, EC-RES-04, EC-CFG-05 | Dépendances explicites, liens entrée/sortie, paramètres de configuration : modélisés, jamais utilisés. |
| EC-NOM-04 | Nom personnalisé de ressource jamais utilisé. |
| EC-MCP-01 (D03) | 4 groupes d'outils MCP non enregistrés. |
| EC-DEP-01 / EC-GIT-06 (F20) | Écran de renouvellement de jeton jamais ouvert. |
| EC-APP-09 | Options de pipeline générables mais pas saisissables. |

## 2. Correspondance feature-map

| Feature-map | Écarts |
|---|---|
| D02 | EC-NET-02 |
| D03 | EC-MCP-01 |
| D04 | EC-NET-07 |
| D05 | EC-NOM-01, EC-NOM-06, EC-GEN-02 |
| D06 | EC-CAT-03, EC-GEN-03 |
| D07 | EC-APP-01, EC-GEN-04 |
| D08 | EC-NET-06 |
| D09, D10 | EC-DEP-06, EC-PIP-06, EC-GIT-05 (validation réelle restante) |
| D11 | EC-NET-01 |
| D12 | EC-GIT-01 |
| F01 | EC-PRJ-02 |
| F06 | EC-DEP-07, EC-APP-14 |
| F08 | EC-RES-03 |
| F11b | EC-GEN-01 |
| F15 | EC-CFG-01, EC-GEN-05 |
| F20 | EC-DEP-01, EC-GIT-06 |

Écarts **nouveaux** (absents de la feature-map, relevés pendant la rédaction des specs) : notamment
EC-NET-03, EC-NET-04, EC-PIP-01 à 04, EC-GIT-02, EC-RBA-01, EC-ACC-01 à 06, EC-CAT-01, EC-APP-02,
EC-APP-09, EC-UI-01. À reporter dans la feature-map lors du prochain tri.

## 3. Liste complète

| Écart | Feature-map | Intitulé | Document |
|---|---|---|---|
| EC-ACC-01 |  | Lectures sans contrôle d'appartenance | [02](02-acces-et-droits.md) |
| EC-ACC-02 |  | Liste de tous les utilisateurs | [02](02-acces-et-droits.md) |
| EC-ACC-03 |  | Dernier Owner | [02](02-acces-et-droits.md) |
| EC-ACC-04 |  | Doublon de membre | [02](02-acces-et-droits.md) |
| EC-ACC-05 |  | Portées inconnues | [02](02-acces-et-droits.md) |
| EC-ACC-06 |  | Invitation impossible | [02](02-acces-et-droits.md) |
| EC-PRJ-01 |  | Deux créations, deux règles | [03](03-projets.md) |
| EC-PRJ-02 | F01 | Jeton non testé à la création | [03](03-projets.md) |
| EC-PRJ-03 |  | Renommer un environnement casse des liens | [03](03-projets.md) |
| EC-PRJ-04 |  | Supprimer un environnement laisse des données | [03](03-projets.md) |
| EC-PRJ-05 |  | Jetons orphelins | [03](03-projets.md) |
| EC-PRJ-06 |  | Défauts de nommage dupliqués | [03](03-projets.md) |
| EC-PRJ-07 |  | Région par défaut | [03](03-projets.md) |
| EC-DEP-01 | F20 | Remplacement de jeton isolé sans écran | [04](04-topologie-et-depots.md) |
| EC-DEP-02 |  | Secret fantôme | [04](04-topologie-et-depots.md) |
| EC-DEP-03 |  | Sous-chemin de dépôt jamais honoré | [04](04-topologie-et-depots.md) |
| EC-DEP-04 |  | Droits par niveau incohérents | [04](04-topologie-et-depots.md) |
| EC-DEP-05 |  | Fournisseurs annoncés | [04](04-topologie-et-depots.md) |
| EC-DEP-06 |  | Validation réelle MultiRepo manquante | [04](04-topologie-et-depots.md) |
| EC-DEP-07 |  | Navigation de dépôt partielle en MultiRepo (F06) | [04](04-topologie-et-depots.md) |
| EC-DEP-08 |  | Changement de topologie destructif sans avertissement serveur | [04](04-topologie-et-depots.md) |
| EC-CFG-01 | F15 | Références partiellement câblées | [05](05-configurations.md) |
| EC-CFG-02 |  | Région libre | [05](05-configurations.md) |
| EC-CFG-03 |  | Renommer ou déplacer un resource group | [05](05-configurations.md) |
| EC-CFG-04 |  | Mode des pipelines applicatifs inatteignable | [05](05-configurations.md) |
| EC-CFG-05 |  | Paramètres de configuration morts | [05](05-configurations.md) |
| EC-CFG-06 |  | Pas d'alias ni d'usage déclaré | [05](05-configurations.md) |
| EC-CFG-07 |  | Route mal rangée | [05](05-configurations.md) |
| EC-NOM-01 | D05 | Deux calculs différents | [06](06-nommage.md) |
| EC-NOM-02 |  | Vérification DNS au lieu de l'API Azure | [06](06-nommage.md) |
| EC-NOM-03 |  | `{location}` différent entre écran et Bicep | [06](06-nommage.md) |
| EC-NOM-04 |  | Nom personnalisé mort | [06](06-nommage.md) |
| EC-NOM-05 |  | Contrôles inégaux | [06](06-nommage.md) |
| EC-NOM-06 |  | Le gabarit recommandé ne protège que l'écran | [06](06-nommage.md) |
| EC-NOM-07 |  | Pas de contrôle à la génération | [06](06-nommage.md) |
| EC-NOM-08 |  | VirtualNetwork sans abréviation | [06](06-nommage.md) |
| EC-RES-01 |  | Noms en double | [07](07-modele-commun-ressources.md) |
| EC-RES-02 |  | Dépendances explicites mortes | [07](07-modele-commun-ressources.md) |
| EC-RES-03 | F08 | Event Hub sans réglages à l'écran | [07](07-modele-commun-ressources.md) |
| EC-RES-04 |  | Liens d'entrée/sortie morts | [07](07-modele-commun-ressources.md) |
| EC-RES-05 |  | Dépendants incomplets | [07](07-modele-commun-ressources.md) |
| EC-RES-06 |  | Modifications ignorées sans message | [07](07-modele-commun-ressources.md) |
| EC-RES-07 |  | Trois régions par défaut | [07](07-modele-commun-ressources.md) |
| EC-CAT-02 |  | Pas de contrôle d'adressage | [08](08-catalogue-ressources.md) |
| EC-CAT-04 |  | Textes libres | [08](08-catalogue-ressources.md) |
| EC-CAT-05 |  | Valeurs serveur sans écran | [08](08-catalogue-ressources.md) |
| EC-CAT-06 |  | Pas de sortie pour trois types | [08](08-catalogue-ressources.md) |
| EC-CAT-07 |  | Versions figées | [08](08-catalogue-ressources.md) |
| EC-RBA-01 |  | Cible hors projet | [09](09-identites-et-rbac.md) |
| EC-RBA-02 |  | Source sans identité utile | [09](09-identites-et-rbac.md) |
| EC-RBA-03 |  | Diagnostic Key Vault strict | [09](09-identites-et-rbac.md) |
| EC-RBA-04 |  | Retrait de l'identité silencieux | [09](09-identites-et-rbac.md) |
| EC-RBA-05 |  | Portée unique | [09](09-identites-et-rbac.md) |
| EC-APP-01 | D07 | Clés App Configuration jamais générées | [10](10-configuration-applicative.md) |
| EC-APP-02 |  | Clé unique sans label | [10](10-configuration-applicative.md) |
| EC-APP-03 |  | Doublon de nom d'app setting en erreur technique | [10](10-configuration-applicative.md) |
| EC-APP-04 |  | Modification limitée | [10](10-configuration-applicative.md) |
| EC-APP-05 |  | Lectures sans contrôle | [10](10-configuration-applicative.md) |
| EC-APP-06 |  | Pas d'output cross-config explicite | [10](10-configuration-applicative.md) |
| EC-NET-01 | D11 | Privatisée sans Private Endpoint | [11](11-reseau-et-exposition.md) |
| EC-NET-02 | D02 | L'onglet Réseau ne charge pas | [11](11-reseau-et-exposition.md) |
| EC-NET-03 |  | Mode DNS ignoré | [11](11-reseau-et-exposition.md) |
| EC-NET-04 |  | Plusieurs sous-ressources dans un seul Private Endpoint | [11](11-reseau-et-exposition.md) |
| EC-NET-05 |  | Branchement des modules non vérifié | [11](11-reseau-et-exposition.md) |
| EC-NET-06 | D08 | Validation DNS fictive | [11](11-reseau-et-exposition.md) |
| EC-NET-07 | D04 | Profil réseau V2 mort | [11](11-reseau-et-exposition.md) |
| EC-NET-08 |  | Aucun outil MCP | [11](11-reseau-et-exposition.md) |
| EC-NET-09 |  | Pas de NSG ni de pare-feu IP | [11](11-reseau-et-exposition.md) |
| EC-APP-07 |  | `Combined` ne combine rien | [12](12-pipeline-applicatif.md) |
| EC-APP-08 |  | Deux énumérations du mode | [12](12-pipeline-applicatif.md) |
| EC-APP-09 |  | Options générées mais pas saisissables | [12](12-pipeline-applicatif.md) |
| EC-APP-10 |  | Scans de sécurité forcés | [12](12-pipeline-applicatif.md) |
| EC-APP-11 |  | Détection partielle | [12](12-pipeline-applicatif.md) |
| EC-APP-12 |  | « Image validée » non vérifiée | [12](12-pipeline-applicatif.md) |
| EC-APP-13 |  | Premier environnement implicite | [12](12-pipeline-applicatif.md) |
| EC-APP-14 |  | Navigation de dépôt au niveau projet seulement | [12](12-pipeline-applicatif.md) |
| EC-GEN-01 | F11b | Génération par configuration joignable par l'API mais sans entrée à l'écran hors MultiRepo | [13](13-generation-bicep.md) |
| EC-GEN-02 | D05 | Le nom généré peut différer du nom affiché et validé à l'écran | [13](13-generation-bicep.md) |
| EC-GEN-03 | D06 | Réglages par environnement perdus pour DocumentIntelligence (nom de module de base inconnu) | [13](13-generation-bicep.md) |
| EC-GEN-04 | D07 | Clés App Configuration jamais générées | [13](13-generation-bicep.md) |
| EC-GEN-05 | F15 | Seules 6 propriétés de référence cross-config sont branchées ; les autres sont ignorées sans message | [13](13-generation-bicep.md) |
| EC-GEN-06 |  | Pas d'historique ni de rétention des générations : seule la dernière est retrouvable | [13](13-generation-bicep.md) |
| EC-GEN-07 |  | Pas de validation du Bicep produit (`bicep build`, what-if) avant livraison | [13](13-generation-bicep.md) |
| EC-PIP-01 |  | Approbations non créées | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-02 |  | Service connections non créées | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-03 |  | Valeurs des variables vides | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-04 |  | Pas de mise à jour | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-05 |  | Chemins de base ignorés | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-06 |  | Validation MultiRepo réelle | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-07 |  | Azure DevOps uniquement | [14](14-generation-pipelines-bootstrap.md) |
| EC-PIP-08 |  | Pas de what-if | [14](14-generation-pipelines-bootstrap.md) |
| EC-GIT-01 | D12 | Le bouton « Push to Git » de l'en-tête de configuration ne pousse que le Bicep | [15](15-livraison-git.md) |
| EC-GIT-02 |  | Pas de pull request | [15](15-livraison-git.md) |
| EC-GIT-03 |  | Fichiers à la racine non nettoyés | [15](15-livraison-git.md) |
| EC-GIT-04 |  | Sous-chemin de dépôt ignoré | [15](15-livraison-git.md) |
| EC-GIT-05 |  | Validation réelle MultiRepo | [15](15-livraison-git.md) |
| EC-GIT-06 | F20 | Écran mort de renouvellement du jeton | [15](15-livraison-git.md) |
| EC-GIT-07 |  | Génération et push non liés | [15](15-livraison-git.md) |
| EC-DIA-01 |  | Règle Key Vault stricte | [16](16-diagnostics.md) |
| EC-DIA-02 |  | Diagnostics éclatés | [16](16-diagnostics.md) |
| EC-DIA-03 |  | Contrôles absents | [16](16-diagnostics.md) |
| EC-DIA-04 |  | Disponibilité du nom par DNS public | [16](16-diagnostics.md) |
| EC-DIA-05 |  | Avant génération projet seulement | [16](16-diagnostics.md) |
| EC-IMP-01 |  | Pas d'écran | [17](17-import-arm.md) |
| EC-IMP-02 |  | Nouveau projet uniquement | [17](17-import-arm.md) |
| EC-IMP-03 |  | Un seul resource group | [17](17-import-arm.md) |
| EC-IMP-04 |  | ARM JSON seulement | [17](17-import-arm.md) |
| EC-IMP-05 |  | Réglages par environnement perdus | [17](17-import-arm.md) |
| EC-IMP-06 |  | Abonnement vide | [17](17-import-arm.md) |
| EC-MCP-01 | D03 | 4 groupes d'outils non enregistrés | [18](18-serveur-mcp.md) |
| EC-MCP-02 |  | Couverture partielle | [18](18-serveur-mcp.md) |
| EC-MCP-03 |  | Génération Bicep projet seulement | [18](18-serveur-mcp.md) |
| EC-MCP-04 |  | Brouillons non persistés | [18](18-serveur-mcp.md) |
| EC-MCP-05 |  | Lecture limitée | [18](18-serveur-mcp.md) |
| EC-UI-01 |  | Paramètres de projet fictifs | [19](19-interface-et-preferences.md) |
| EC-UI-02 |  | Préférences non synchronisées | [19](19-interface-et-preferences.md) |
| EC-UI-03 |  | Pas de vue graphe | [19](19-interface-et-preferences.md) |
| EC-UI-04 |  | Pas d'écran d'import ARM | [19](19-interface-et-preferences.md) |
| EC-UI-05 |  | Pas de recherche globale | [19](19-interface-et-preferences.md) |
