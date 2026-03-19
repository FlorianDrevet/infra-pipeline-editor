# UI Library

La librairie UI contient tous les composants réutilisables du projet, organisés selon l'architecture Atomic Design.

## Structure

### Atoms (composants élémentaires)
Des composants très simples et autonomes, réutilisables dans toute l'application.

- **AvatarComponent** (`avatar/`): Affiche une initiale en circle avec couleurs configurable
  - Inputs: `initials`, `size` (xs|sm|md|lg|xl), `variant` (primary|secondary|accent|success|warning|error), `tooltip`
  
- **StatItemComponent** (`stat-item/`): Affiche une statistique avec icône, valeur et label
  - Inputs: `icon`, `value`, `label`, `color` (primary|accent|warn)
  
- **EmptyStateComponent** (`empty-state/`): État vide avec icône, titre, description et bouton optionnel
  - Inputs: `icon`, `title`, `description`, `actionLabel`, `fullHeight`
  
- **LoadingSpinnerComponent** (`loading-spinner/`): Spinner de chargement avec message
  - Inputs: `diameter`, `message`, `fullHeight`

### Molecules (composants composites)
Des composants composés d'atoms et d'autres éléments Material, réutilisables pour des UI complexes.

- **CardComponent** (`molecules/card/`): Conteneur card flexible avec variantes (default|outlined|elevated) et mode clickable
  - Inputs: `variant`, `clickable`
  - Output: `cardClick`
  
- **DataTableComponent** (`molecules/data-table/`): Table dynamique avec définition de colonnes et support nested properties
  - Inputs: `columns` (TableColumn[]), `dataSource`, `emptyMessage`, `emptyIcon`
  
- **TabbedViewComponent** (`molecules/tabbed-view/`): Onglets Material encapsulés
  - Inputs: `tabs` (TabItem[]), `backgroundColor`

## Utilisation

```typescript
import {
  AvatarComponent,
  CardComponent,
  DataTableComponent,
  EmptyStateComponent,
  LoadingSpinnerComponent,
  StatItemComponent,
  TabbedViewComponent,
  TableColumn,
} from './shared/ui-library';

// Puis ajouter dans les imports du composant
@Component({
  imports: [CardComponent, AvatarComponent, ...],
  ...
})
```

## Conventions

1. Tous les composants sont **standalone** et **OnPush**
2. HTML/SCSS séparés en fichiers distincts
3. Les propriétés d'input avec valeurs par défaut sont always définis en `@Input()`
4. Les outputs utilisent `EventEmitter` pour les interactions utilisateur
5. Les styles utilisent SCSS avec BEM pour le naming

## Patterns communs

### Utiliser AvatarComponent dans une liste
```typescript
<app-avatar 
  [initials]="user.name.charAt(0)" 
  size="md" 
  variant="primary" 
  [tooltip]="user.fullName">
</app-avatar>
```

### Utiliser DataTableComponent
```typescript
<app-data-table
  [columns]="columns"
  [dataSource]="items"
  emptyMessage="No items"
  emptyIcon="inbox">
</app-data-table>

// Dans le composant
columns: TableColumn[] = [
  { key: 'name', label: 'Name', width: '40%' },
  { key: 'email', label: 'Email', width: '60%' }
];
```

### Utiliser CardComponent
```typescript
<app-card [clickable]="true" variant="elevated" (cardClick)="onCardClicked()">
  <!-- Contenu de la card -->
</app-card>
```

## Roadmap future

- [ ] FormFieldComponent (texte, select, checkbox)
- [ ] DialogComponent wrapper
- [ ] PaginatorComponent
- [ ] BadgeComponent
- [ ] ToastNotificationComponent
- [ ] BreadcrumbComponent
