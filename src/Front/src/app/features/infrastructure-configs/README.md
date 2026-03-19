# Infrastructure Configs Feature

La feature `infrastructure-configs` gère l'affichage et la gestion des configurations d'infrastructure Azure.

## Structure

```
infrastructure-configs/
├── components/
│   └── config-card/                   # Composant card réutilisable pour une configuration
│       ├── config-card.component.ts   
│       ├── config-card.component.html 
│       └── config-card.component.scss 
├── pages/
│   ├── configs-list/                  # Page liste des configurations
│   │   ├── configs-list.component.ts
│   │   ├── configs-list.component.html
│   │   └── configs-list.component.scss
│   └── config-details/                # Page détails d'une configuration
│       ├── config-details.component.ts
│       ├── config-details.component.html
│       └── config-details.component.scss
└── README.md
```

## Composants

### ConfigsListComponent
Page d'accueil affichant une grid de cards des configurations.
- Route: `/configs`
- Charge automatiquement les configurations au montage
- Utilise `LoadingSpinnerComponent` et `EmptyStateComponent` pour les états vides/chargement

### ConfigCardComponent
Card réutilisable affichant une configuration avec statistiques, membres et environnements.
- Utilise les atoms: `AvatarComponent`, `StatItemComponent`, `CardComponent`
- Affiche: nom, 3 stats (ressources, environnements, membres), équipe et environnements

### ConfigDetailsComponent
Page détails avec 3 sections principales.
- Route: `/configs/:id`
- **Resources tab**: Tableau des resource groups
- **Environments tab**: Cards détaillées des environnements avec tags
- **Members tab**: Tableau des membres du projet avec rôles

## Services utilisés

- `InfraConfigService`: Gestion des configurations (load, create, update, delete)
- `ResourceGroupService`: Gestion des resource groups

## Patterns appliqués

1. **Atomic Design**: Composition avec atoms et molecules
2. **Standalone Components**: Chaque composant est standalone avec ses imports
3. **Change Detection OnPush**: Meilleure performance et clarté du data flow
4. **Séparation des fichiers**: HTML, SCSS et TS dans des fichiers distincts
5. **Type-safe**: Tous les types sont définis et vérifiés par TypeScript

## Flux de données

```
ConfigsListComponent (page list)
  ├── InfraConfigService.loadConfigurations()
  ├── Signal: configurations() → affiche les cards
  └── ConfigCardComponent
      └── Affiche une configuration avec avatars, stats, etc.

ConfigDetailsComponent (page détails)
  ├── InfraConfigService.loadConfigDetails(id)
  ├── ResourceGroupService.loadResourceGroups(id)
  └── Affiche les 3 tabs: Resources, Environments, Members
      ├── Resources tab → DataTable de resource groups
      ├── Environments tab → Cards des environnements
      └── Members tab → DataTable des membres
```

## Améliorations futures

- [ ] Dialog pour créer nouvelle configuration
- [ ] Édition/suppression de configurations
- [ ] Ajout/suppression de membres
- [ ] Édition des environnements
- [ ] Filtrage/recherche des configurations
- [ ] Export des configurations
