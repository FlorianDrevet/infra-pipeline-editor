# Skill : angular-patterns — Conventions Angular 19 du projet

> **Chargé automatiquement par l'agent `angular-front`.**
> Contient les patterns techniques Angular 19 spécifiques au projet InfraFlowSculptor.

---

## Règles Angular 19 — Fondamentaux

### 1. Standalone components obligatoires

**Tous** les composants sont `standalone: true`. Aucun NgModule n'est utilisé dans ce projet.

```typescript
@Component({
  selector: 'app-feature',
  standalone: true,
  imports: [CommonModule, MatButtonModule, RouterLink, /* ... */],
  templateUrl: './feature.component.html',
  styleUrl: './feature.component.scss',
})
export class FeatureComponent { }
```

### 2. Zoneless Change Detection

Le projet utilise `provideExperimentalZonelessChangeDetection()` dans `app.config.ts`. Cela signifie :
- Zone.js **ne détecte plus** les changements automatiquement.
- Utiliser **exclusivement des Signals** pour l'état local des composants.
- Utiliser `toSignal()` pour convertir les Observables RxJS en Signals.
- Ne jamais appeler `ChangeDetectorRef.detectChanges()` manuellement.
- Les Promises natives + Signals fonctionnent correctement.

### 3. Injection via `inject()` — jamais le constructeur

```typescript
// ✅ Correct
export class MyComponent {
  private readonly router = inject(Router);
  private readonly myService = inject(MyService);
}

// ❌ Interdit — ne pas utiliser le constructeur pour l'injection
export class MyComponent {
  constructor(private router: Router) {}
}
```

Exception : les classes non-composants (guards en fonction, etc.) utilisent aussi `inject()`.

### 4. Visibilité des membres dans les composants

Appliquer systématiquement les bons modificateurs de visibilité :

```typescript
export class MyComponent {
  // Injections — toujours private readonly
  private readonly router = inject(Router);
  private readonly service = inject(MyService);

  // État exposé au template — protected (pas public)
  protected items = signal<Item[]>([]);
  protected isLoading = signal(false);

  // Méthodes appelées uniquement depuis le template — protected
  protected onSubmit(): void { }

  // Méthodes internes — private
  private loadData(): void { }
}
```

---

## Signals API — Règles d'utilisation

### Signals de base

```typescript
import { signal, computed, effect, Signal } from '@angular/core';

// État mutable
protected count = signal(0);
protected items = signal<Item[]>([]);
protected isLoading = signal(false);
protected errorMessage = signal('');

// Valeur dérivée (lecture seule, recalculée automatiquement)
protected total = computed(() => this.items().length);
protected hasError = computed(() => this.errorMessage() !== '');

// Effets secondaires réactifs
private logEffect = effect(() => {
  console.log('items changed:', this.items());
});
```

### Input Signals (Angular 17.1+)

```typescript
import { input, InputSignal } from '@angular/core';

// Input obligatoire
name = input.required<string>();

// Input optionnel avec valeur par défaut
maxItems = input(10);

// Input avec alias
userId = input.required<string>({ alias: 'id' });

// Dans le template parent : <app-child [name]="value" />
```

### Output (Angular 17.3+)

```typescript
import { output, OutputEmitterRef } from '@angular/core';

// Output simple
itemSelected = output<Item>();
closed = output<void>();

// Émettre
this.itemSelected.emit(item);
this.closed.emit();
```

### Model Signals (two-way binding)

```typescript
import { model, ModelSignal } from '@angular/core';

// Pour un two-way binding [(value)]="..."
value = model<string>('');

// Dans le template : [(value)]="myValue"
```

### toSignal — conversion RxJS → Signal

```typescript
import { toSignal } from '@angular/core/rxjs-interop';

// Observable → Signal (lit immédiatement la valeur courante)
protected currentRoute = toSignal(
  this.router.events.pipe(
    filter(e => e instanceof NavigationEnd),
    map(e => (e as NavigationEnd).url)
  )
);

// Avec valeur initiale
protected isAuthenticated = toSignal(
  this.authService.isAuthenticated$,
  { initialValue: false }
);
```

### resource() — chargement asynchrone réactif (Angular 19)

```typescript
import { resource } from '@angular/core';

// Exemple: charger des données en fonction d'un signal
protected itemId = signal<string | null>(null);

protected itemResource = resource({
  request: () => this.itemId(),
  loader: async ({ request: id }) => {
    if (!id) return null;
    return this.myService.getById(id);
  }
});

// Dans le template :
// @if (itemResource.isLoading()) { <mat-spinner /> }
// @if (itemResource.value()) { ... }
// @if (itemResource.error()) { ... }
```

---

## Template — Nouvelles syntaxes obligatoires

Utiliser **exclusivement** la nouvelle syntaxe de flux de contrôle Angular 17+ :

```html
<!-- ✅ Nouvelle syntaxe — obligatoire -->
@if (isLoading()) {
  <mat-spinner />
} @else if (hasError()) {
  <p class="error">{{ errorMessage() }}</p>
} @else {
  <p>Contenu chargé</p>
}

@for (item of items(); track item.id) {
  <app-item [name]="item.name" />
} @empty {
  <p>Aucun élément.</p>
}

@switch (status()) {
  @case ('active') { <span class="badge-green">Actif</span> }
  @case ('inactive') { <span class="badge-red">Inactif</span> }
  @default { <span>Inconnu</span> }
}

<!-- ❌ Ancienne syntaxe — INTERDITE -->
<div *ngIf="isLoading">...</div>
<li *ngFor="let item of items">...</li>
```

**`track` est obligatoire** dans `@for`. Utiliser l'identifiant unique de l'objet (`.id`, `.uuid`, etc.), ou `$index` en dernier recours.

---

## Formulaires — Reactive Forms typés

```typescript
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

// Dans le composant
private readonly fb = inject(FormBuilder);

protected form = this.fb.group({
  name: ['', [Validators.required, Validators.minLength(3)]],
  email: ['', [Validators.required, Validators.email]],
  role: ['', Validators.required],
});

// Accès typé
protected get nameControl() {
  return this.form.controls.name;
}

// Soumission
protected async onSubmit(): Promise<void> {
  if (this.form.invalid) return;

  this.isLoading.set(true);
  try {
    const value = this.form.getRawValue();
    await this.myService.create(value);
    this.router.navigate(['/success']);
  } catch (error) {
    this.errorMessage.set('Une erreur est survenue.');
  } finally {
    this.isLoading.set(false);
  }
}
```

Template :
```html
<form [formGroup]="form" (ngSubmit)="onSubmit()">
  <mat-form-field>
    <mat-label>Nom</mat-label>
    <input matInput formControlName="name" />
    @if (nameControl.hasError('required') && nameControl.touched) {
      <mat-error>Champ requis</mat-error>
    }
  </mat-form-field>

  <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || isLoading()">
    @if (isLoading()) { Envoi en cours... } @else { Enregistrer }
  </button>
</form>
```

---

## Services — Pattern Axios

Ce projet utilise **Axios** (pas `HttpClient`). Toujours utiliser `AxiosService` via injection.

```typescript
import { inject, Injectable } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import { MyItemResponse, CreateMyItemRequest } from '../interfaces/my-item.interface';

@Injectable({
  providedIn: 'root',
})
export class MyItemService {
  private readonly axios = inject(AxiosService);

  getAll(): Promise<MyItemResponse[]> {
    return this.axios.request$<MyItemResponse[]>(MethodEnum.GET, '/my-items');
  }

  getById(id: string): Promise<MyItemResponse> {
    return this.axios.request$<MyItemResponse>(MethodEnum.GET, `/my-items/${id}`);
  }

  create(request: CreateMyItemRequest): Promise<MyItemResponse> {
    return this.axios.request$<MyItemResponse>(MethodEnum.POST, '/my-items', request);
  }

  update(id: string, request: Partial<CreateMyItemRequest>): Promise<MyItemResponse> {
    return this.axios.request$<MyItemResponse>(MethodEnum.PUT, `/my-items/${id}`, request);
  }

  delete(id: string): Promise<void> {
    return this.axios.request$<void>(MethodEnum.DELETE, `/my-items/${id}`);
  }
}
```

**Règles :**
- Retourner des `Promise<T>`, pas des `Observable<T>` pour les appels Axios.
- `private readonly axios` — toujours private readonly.
- Les URLs d'API ne doivent **jamais** contenir le `base_url` : AxiosService l'ajoute automatiquement.
- Toujours typer le générique `<T>` de `axios.request$`.

---

## Interfaces TypeScript — Alignement backend

Les interfaces frontend doivent correspondre exactement aux DTOs du backend (`InfraFlowSculptor.Contracts`).

```typescript
// src/Front/src/app/shared/interfaces/my-feature.interface.ts

// Correspond à MyFeatureResponse.cs
export interface MyFeatureResponse {
  id: string;          // Guid → string en JSON
  name: string;
  location: string;
  createdAt: string;   // DateTime → string ISO 8601
}

// Correspond à CreateMyFeatureRequest.cs
export interface CreateMyFeatureRequest {
  name: string;
  location: string;
}

// Correspond à UpdateMyFeatureRequest.cs
export interface UpdateMyFeatureRequest {
  name?: string;
  location?: string;
}
```

**Règles :**
- `Guid` backend → `string` frontend.
- `DateTime` backend → `string` frontend.
- Pas d'`any` ni `unknown` sans justification.
- Grouper toutes les interfaces d'une feature dans un seul fichier `feature.interface.ts`.

---

## Routing — Lazy Loading obligatoire

```typescript
// app-routing.ts
export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'infra-configs',
    canActivate: [AuthenticationGuard],
    loadComponent: () =>
      import('./features/infra-config/infra-config-list.component')
        .then(m => m.InfraConfigListComponent),
  },
];
```

**Règles :**
- **Toujours** `loadComponent` pour les routes features (jamais `component: MyComponent` directement).
- Utiliser `canActivate: [AuthenticationGuard]` pour toutes les routes protégées.
- Préférer des routes imbriquées pour les sections d'une même feature.

---

## Guards — Fonctions pures (pas classes)

```typescript
import { inject } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { MsalAuthService } from '../services/msal-auth.service';

export const MyGuard = async (): Promise<boolean | UrlTree> => {
  const service = inject(MsalAuthService);
  const router = inject(Router);

  const isAllowed = await service.checkPermission();
  return isAllowed ? true : router.createUrlTree(['/unauthorized']);
};
```

---

## Angular Material + Tailwind — Utilisation combinée

### Règle de base

- **Angular Material** : pour les composants interactifs (boutons, champs, dialogues, tableaux, etc.).
- **Tailwind** : pour le layout et l'espacement (flex, grid, p-, m-, gap-, w-, h-, etc.).
- **SCSS scopé** : pour les styles complexes ou les overrides Material.

### Imports Material dans le composant

```typescript
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
```

### Exemple de layout combiné

```html
<!-- layout Tailwind, composants Material -->
<div class="flex flex-col gap-6 p-6 max-w-4xl mx-auto">
  <mat-card>
    <mat-card-header>
      <mat-card-title>Titre</mat-card-title>
    </mat-card-header>
    <mat-card-content class="flex flex-col gap-4 mt-4">
      <!-- contenu -->
    </mat-card-content>
    <mat-card-actions class="flex justify-end gap-2">
      <button mat-stroked-button (click)="onCancel()">Annuler</button>
      <button mat-raised-button color="primary" (click)="onSave()">Enregistrer</button>
    </mat-card-actions>
  </mat-card>
</div>
```

### Theming SCSS

Ne pas dupliquer les couleurs ou tailles en dur dans le SCSS. Utiliser les variables CSS du thème Angular Material et les classes Tailwind. Si un style est spécifique à un composant, le définir dans le `.scss` scopé avec `:host {}`.

---

## Pattern de chargement asynchrone — Smart Component

```typescript
@Component({
  selector: 'app-items-list',
  standalone: true,
  imports: [MatTableModule, MatProgressSpinnerModule, MatButtonModule],
  templateUrl: './items-list.component.html',
  styleUrl: './items-list.component.scss',
})
export class ItemsListComponent implements OnInit {
  private readonly service = inject(ItemService);
  private readonly router = inject(Router);

  protected items = signal<ItemResponse[]>([]);
  protected isLoading = signal(false);
  protected errorMessage = signal('');

  protected displayedColumns = ['name', 'location', 'actions'];

  async ngOnInit(): Promise<void> {
    await this.loadItems();
  }

  private async loadItems(): Promise<void> {
    this.isLoading.set(true);
    this.errorMessage.set('');
    try {
      const result = await this.service.getAll();
      this.items.set(result);
    } catch {
      this.errorMessage.set('Erreur lors du chargement.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async onDelete(id: string): Promise<void> {
    this.isLoading.set(true);
    try {
      await this.service.delete(id);
      this.items.update(items => items.filter(i => i.id !== id));
    } catch {
      this.errorMessage.set('Erreur lors de la suppression.');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected navigateTo(id: string): void {
    this.router.navigate(['/items', id]);
  }
}
```

---

## Environnements — URLs d'API

**Jamais** hardcoder une URL d'API dans un service ou un composant.

```typescript
// Toujours via environment
import { environment } from '../../../environments/environment';

// Dans un service, si besoin de construire une URL complète :
const fullUrl = `${environment.api_url}/my-endpoint`;
```

En pratique, `AxiosService` lit automatiquement `environment.api_url`. Les services API n'ont pas besoin de l'importer directement — ils passent juste le chemin relatif.

| Config | Fichier | Usage |
|--------|---------|-------|
| `development` | `environment.development.ts` | `npm run start` |
| `aspire` | `environment.aspire.ts` | `npm run start:aspire` (via Aspire, proxy) |
| `production` | `environment.ts` | `npm run build` |

---

## Gestion d'erreur — Patterns recommandés

### Snackbar pour les erreurs utilisateur

```typescript
import { MatSnackBar } from '@angular/material/snack-bar';

private readonly snackBar = inject(MatSnackBar);

private showError(message: string): void {
  this.snackBar.open(message, 'Fermer', { duration: 5000, panelClass: 'error-snackbar' });
}
```

### Signal d'erreur visible dans le template

```typescript
protected errorMessage = signal('');

// Dans le template :
// @if (errorMessage()) {
//   <mat-error>{{ errorMessage() }}</mat-error>
// }
```

---

## Enums TypeScript — Règles strictes

### Placement et fichier dédié

**Les enums DOIVENT être dans un fichier séparé** — jamais définis dans le même fichier que le composant.

**Arborescence :**
- Si l'enum est utilisé par **plusieurs features** → `src/Front/src/app/shared/enums/{enum-name}.enum.ts`
- Si l'enum est utilisé par **une seule feature** → `src/Front/src/app/features/{feature}/enums/{enum-name}.enum.ts`

### Backend Enum → Frontend Dropdown

**Règle universelle** : Si un champ est un **enum au backend**, il DOIT avoir un **dropdown (`<mat-select>`)** au frontend, pas un input texte libre.

**Exemple :**
- Backend : `Location.LocationEnum` avec valeurs (EastUS, WestUS, etc.)
- Frontend : Créer `src/Front/src/app/features/config-detail/enums/location.enum.ts`
- UI Component : Utiliser `<mat-select>` avec `<mat-option>` pour chaque valeur enum

```typescript
// location.enum.ts
export enum LocationEnum {
  EastUS = 'EastUS',
  WestUS = 'WestUS',
  // ...
}

export const LOCATION_OPTIONS = Object.entries(LocationEnum).map(([key, value]) => ({
  label: key,
  value,
}));

// component.ts
import { LOCATION_OPTIONS } from '../enums/location.enum';

export class MyComponent {
  protected readonly locationOptions = LOCATION_OPTIONS;
}

// component.html
<mat-form-field appearance="outline">
  <mat-label>Location</mat-label>
  <mat-select formControlName="location">
    @for (option of locationOptions; track option.value) {
      <mat-option [value]="option.value">{{ option.label }}</mat-option>
    }
  </mat-select>
</mat-form-field>
```

---

## Conventions TypeScript

- Utiliser `readonly` pour toutes les propriétés qui ne changent pas après l'initialisation.
- Typer explicitement les retours de fonctions async : `async load(): Promise<void>`.
- Préférer `const` et `let` plutôt que `var`.
- Pas de `any` — utiliser des interfaces ou `unknown` + type guard si le type est inconnu.
- Utiliser `?.` (optional chaining) et `??` (nullish coalescing) plutôt que les vérifications manuelles.

---

## SCSS — Conventions

```scss
// feature.component.scss

:host {
  display: block;
  // Styles du composant hôte
}

// Classes locales — préfixées par le nom du composant pour éviter les collisions
.feature-card {
  // ...
}

// Media queries — mobile first
@media (max-width: 768px) {
  .feature-card {
    // responsive adjustments
  }
}

// Pas de couleurs en dur — utiliser les variables CSS Material ou les classes Tailwind
// ❌ color: #1976d2;
// ✅ color: var(--mat-primary-color);
// ✅ class="text-primary" (Tailwind)
```

---

## Internationalisation (i18n) — ngx-translate

Le projet utilise `@ngx-translate/core` + `@ngx-translate/http-loader` pour le **runtime FR/EN** (switch sans rebuild).

### Ajouter TranslateModule à un composant

Tout composant qui affiche du texte UI **doit** importer `TranslateModule` :

```typescript
import { TranslateModule } from '@ngx-translate/core';

@Component({
  standalone: true,
  imports: [CommonModule, MatButtonModule, TranslateModule /* ... */],
  templateUrl: './my.component.html',
})
```

### Pipe `| translate` dans les templates

```html
<!-- Texte simple -->
<h1>{{ 'HOME.TITLE' | translate }}</h1>

<!-- Avec paramètres dynamiques -->
<p>{{ 'HOME.FEEDBACK.CREATE_SUCCESS' | translate: { name: createdConfigName() } }}</p>

<!-- Attribut aria ou placeholder -->
<input [placeholder]="'HOME.FORM.PLACEHOLDER' | translate" />
```

### Convention des namespaces de traduction

Toutes les clés de traduction suivent une hiérarchie par écran/composant dans `src/Front/public/i18n/{fr,en}.json` :

| Namespace | Fichier / composant | Exemple de clé |
|-----------|--------------------|--------------------|
| `LANGUAGE` | `LanguageService`, navigation | `LANGUAGE.FRENCH_SHORT` |
| `NAV` | `navigation.component` | `NAV.HOME`, `NAV.LOGOUT` |
| `FOOTER` | `footer.component` | `FOOTER.RIGHTS` |
| `LOGIN` | `login.component` | `LOGIN.TITLE`, `LOGIN.ERROR.MSAL_FAILED` |
| `HOME` | `home.component` | `HOME.FORM.LABEL_NAME`, `HOME.LIST.EMPTY` |
| `<NEW_FEATURE>` | future composants | `<FEATURE>.TITLE`, `<FEATURE>.ERROR.*` |

**Règle :** Ne jamais mettre de texte UI en dur dans un template — toujours passer par une clé de traduction.

### Ajouter de nouvelles clés

1. Ajouter la clé dans `src/Front/public/i18n/fr.json` (valeur française).
2. Ajouter la même clé dans `src/Front/public/i18n/en.json` (valeur anglaise).
3. Les deux fichiers doivent avoir exactement les mêmes clés — pas de clé manquante dans une langue.

### Pattern pour les messages d'erreur

Stocker la **clé** de traduction dans le signal, pas le texte final :

```typescript
// ✅ Correct : signal stocke une clé i18n
protected errorMessageKey = signal('');

// Dans la méthode d'erreur
this.errorMessageKey.set('LOGIN.ERROR.MSAL_FAILED');

// Dans le template
@if (errorMessageKey()) {
  <mat-error>{{ errorMessageKey() | translate }}</mat-error>
}
```

```typescript
// ❌ Interdit : signal stocke du texte en dur
protected errorMessage = signal('Une erreur est survenue.');
```

### Utiliser LanguageService dans un composant

Injecter `LanguageService` uniquement si le composant a besoin de :
- Afficher le sélecteur de langue.
- Réagir dynamiquement au changement de langue en TS (rare).

```typescript
import { LanguageService } from '../../../shared/services/language.service';

protected readonly languageService = inject(LanguageService);

// Signal readonly de la langue courante
protected readonly currentLanguage = this.languageService.currentLanguage;

// Dans le template, itérer les langues disponibles
@for (lang of languageService.availableLanguages; track lang.code) {
  <button
    class="language-switch__button"
    [class.language-switch__button--active]="currentLanguage() === lang.code"
    (click)="languageService.setLanguage(lang.code)">
    {{ lang.labelKey | translate }}
  </button>
}
```

---

## Baseline visuelle — Login et Home

Tous les nouveaux écrans **authenticated** doivent s'aligner sur la baseline visuelle validée.

### Référence baseline

- Page de connexion : `src/Front/src/app/features/login/login.component.{html,scss}`
- Page d'accueil : `src/Front/src/app/features/home/home.component.{html,scss}`

### Palette et tokens visuels

```scss
// Dégradé premium bleu/cyan — identité visuelle du projet
background: linear-gradient(135deg, #1a237e 0%, #0288d1 50%, #00bcd4 100%);

// Surface secondaire (cartes, formulaires)
background: rgba(255, 255, 255, 0.08);
border: 1px solid rgba(255, 255, 255, 0.15);
border-radius: 16px;
backdrop-filter: blur(10px);

// Texte sur fond sombre
color: rgba(255, 255, 255, 0.9);      // texte principal
color: rgba(148, 203, 255, 0.85);     // texte secondaire / accent
color: rgba(255, 255, 255, 0.6);      // texte tertiaire / label

// Bouton CTA principal
background: linear-gradient(135deg, #0288d1, #00bcd4);
color: #fff;
border-radius: 12px;
```

### Convention typographique hero

```scss
h1 {
  font-size: clamp(1.5rem, 2.2vw, 2.2rem);
  line-height: 1.2;
  max-width: 26ch;   // Pas de max-width trop petit (< 15ch)
  font-weight: 700;
}
```

**Piège validé :** contraindre `max-width` à une valeur trop faible (ex : `12ch`) force le titre à se découper en 6+ lignes. Toujours garder `max-width ≥ 20ch` pour les titres hero.

### Piège validé — Timeline d'ordre (position visuelle vs order backend)

Pour les UIs de réordonnancement (flèches gauche/droite + timeline) :
- La timeline manipule une position 1-based (`1..N+1`)
- Le backend manipule une valeur `order` qui peut être non contiguë

Règle de conversion en mode édition :
- Déplacement vers la gauche: envoyer l'order de l'élément au slot cible (`others[position - 1].order`)
- Déplacement vers la droite: envoyer l'order de l'élément "sauté" (`others[position - 2].order`)

**Anti-pattern à éviter :** `payload.order = currentPosition` ou un mapping identique gauche/droite.

### Layout hero 2 colonnes

```scss
.hero-panel {
  display: grid;
  grid-template-columns: minmax(0, 1.2fr) minmax(16rem, 1fr);
  gap: 2rem;
  align-items: start;
}

.hero-panel__content {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;  // Pas space-between
  gap: 1.1rem;
}
```
