import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  DsButtonComponent,
  DsChipComponent,
  DsIconButtonComponent,
  DsPageHeaderComponent,
  DsSelectComponent,
  DsSelectOption,
  DsTextFieldComponent,
} from '../../shared/components/ds';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { ProjectService } from '../../shared/services/project.service';
import { FavoritesService } from '../../shared/services/favorites.service';
import { CreateProjectWizardDialogComponent } from './create-project-wizard/create-project-wizard-dialog.component';

const PROJECT_SORT_VALUES = {
  name: 'name',
  members: 'members',
  favorites: 'favorites',
} as const;

type ProjectSortKey = (typeof PROJECT_SORT_VALUES)[keyof typeof PROJECT_SORT_VALUES];

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatIconModule,
    DsButtonComponent,
    DsChipComponent,
    DsIconButtonComponent,
    DsPageHeaderComponent,
    DsSelectComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss',
})
export class ProjectsComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly favoritesService = inject(FavoritesService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  protected readonly projects = signal<ProjectResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly searchQuery = signal('');
  protected readonly sortBy = signal<ProjectSortKey>(PROJECT_SORT_VALUES.name);
  protected readonly filterFavoritesOnly = signal(false);
  protected readonly sortOptions: DsSelectOption[] = [
    {
      value: PROJECT_SORT_VALUES.name,
      label: this.translate.instant('PROJECTS.SORT.BY_NAME'),
    },
    {
      value: PROJECT_SORT_VALUES.members,
      label: this.translate.instant('PROJECTS.SORT.BY_MEMBERS'),
    },
    {
      value: PROJECT_SORT_VALUES.favorites,
      label: this.translate.instant('PROJECTS.SORT.BY_FAVORITES'),
    },
  ];

  protected readonly filteredProjects = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const sort = this.sortBy();
    const favOnly = this.filterFavoritesOnly();
    let filtered = this.projects();

    if (favOnly) {
      filtered = filtered.filter((p) => this.favoritesService.isFavorite(p.id));
    }

    if (query) {
      filtered = filtered.filter(
        (p) =>
          p.name.toLowerCase().includes(query) ||
          (p.description?.toLowerCase().includes(query) ?? false)
      );
    }

    return [...filtered].sort((a, b) => {
      switch (sort) {
        case 'members':
          return b.members.length - a.members.length;
        case 'favorites': {
          const aFav = this.favoritesService.isFavorite(a.id) ? 0 : 1;
          const bFav = this.favoritesService.isFavorite(b.id) ? 0 : 1;
          return aFav - bFav || a.name.localeCompare(b.name);
        }
        default:
          return a.name.localeCompare(b.name);
      }
    });
  });

  protected readonly projectCount = computed(() => this.projects().length);

  public async ngOnInit(): Promise<void> {
    await this.loadProjects();
  }

  protected isFavorite(projectId: string): boolean {
    return this.favoritesService.isFavorite(projectId);
  }

  protected toggleFavorite(event: Event, projectId: string): void {
    event.preventDefault();
    event.stopPropagation();
    this.favoritesService.toggle(projectId);
  }

  protected favoriteButtonIcon(projectId: string): string {
    return this.isFavorite(projectId) ? 'star' : 'star_border';
  }

  protected favoriteButtonVariant(projectId: string): 'ghost' | 'subtle' {
    return this.isFavorite(projectId) ? 'subtle' : 'ghost';
  }

  protected onSearchQueryChange(value: string): void {
    this.searchQuery.set(value);
  }

  protected onSortByChange(value: string | number | null): void {
    if (isProjectSortKey(value)) {
      this.sortBy.set(value);
    }
  }

  protected toggleFavoritesFilter(): void {
    this.filterFavoritesOnly.update((v) => !v);
  }

  protected openProject(projectId: string): void {
    void this.router.navigate(['/projects', projectId]);
  }

  protected onProjectKeydown(event: KeyboardEvent, projectId: string): void {
    if (event.key !== 'Enter' && event.key !== ' ') {
      return;
    }

    event.preventDefault();
    this.openProject(projectId);
  }

  protected openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateProjectWizardDialogComponent, {
      width: '960px',
      maxWidth: '96vw',
      maxHeight: '90vh',
      panelClass: 'ifs-wizard-dialog',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.projects.update((projects) => [result, ...projects]);
      }
    });
  }

  protected async refreshProjects(): Promise<void> {
    await this.loadProjects();
  }

  private async loadProjects(): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const projects = await this.projectService.getMyProjects();
      this.projects.set(projects);
    } catch {
      this.loadError.set('PROJECTS.STATE.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }
}

function isProjectSortKey(value: string | number | null): value is ProjectSortKey {
  return typeof value === 'string' && Object.values(PROJECT_SORT_VALUES).includes(value as ProjectSortKey);
}
