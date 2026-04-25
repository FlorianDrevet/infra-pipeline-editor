import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { DsButtonComponent, DsPageHeaderComponent } from '../../shared/components/ds';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { ProjectService } from '../../shared/services/project.service';
import { FavoritesService } from '../../shared/services/favorites.service';
import { CreateProjectWizardDialogComponent } from './create-project-wizard/create-project-wizard-dialog.component';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [TranslateModule, RouterLink, MatDialogModule, MatIconModule, DsButtonComponent, DsPageHeaderComponent],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss',
})
export class ProjectsComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly favoritesService = inject(FavoritesService);
  private readonly dialog = inject(MatDialog);

  protected readonly projects = signal<ProjectResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly searchQuery = signal('');
  protected readonly sortBy = signal<'name' | 'members' | 'favorites'>('name');
  protected readonly filterFavoritesOnly = signal(false);

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

  protected onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
  }

  protected onSortChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sortBy.set(select.value as 'name' | 'members' | 'favorites');
  }

  protected toggleFavoritesFilter(): void {
    this.filterFavoritesOnly.update((v) => !v);
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
