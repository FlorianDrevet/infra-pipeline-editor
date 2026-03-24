import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { ProjectService } from '../../shared/services/project.service';
import { FavoritesService } from '../../shared/services/favorites.service';
import { RecentlyViewedService, RecentlyViewedItem } from '../../shared/services/recently-viewed.service';
import { CreateProjectDialogComponent } from './create-project-dialog/create-project-dialog.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TranslateModule, RouterLink, MatDialogModule, MatIconModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly favoritesService = inject(FavoritesService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);
  private readonly dialog = inject(MatDialog);

  protected readonly projects = signal<ProjectResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');

  protected readonly recentItems = this.recentlyViewedService.recentItems;

  protected readonly totalMemberCount = computed(() =>
    this.projects().reduce((count, project) => count + project.members.length, 0)
  );

  protected readonly favoriteProjects = computed(() =>
    this.projects().filter((p) => this.favoritesService.isFavorite(p.id))
  );

  public async ngOnInit(): Promise<void> {
    await Promise.all([
      this.loadProjects(),
      this.recentlyViewedService.validateAndRefresh(),
    ]);
  }

  protected isFavorite(projectId: string): boolean {
    return this.favoritesService.isFavorite(projectId);
  }

  protected toggleFavorite(event: Event, projectId: string): void {
    event.preventDefault();
    event.stopPropagation();
    this.favoritesService.toggle(projectId);
  }

  protected openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateProjectDialogComponent, {
      width: '480px',
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.projects.update((projects) => [result, ...projects]);
      }
    });
  }

  protected getRecentItemRoute(item: RecentlyViewedItem): string[] {
    return item.type === 'project' ? ['/projects', item.id] : ['/config', item.id];
  }

  protected getRecentItemIcon(item: RecentlyViewedItem): string {
    return item.type === 'project' ? 'folder' : 'settings';
  }

  private async loadProjects(): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const projects = await this.projectService.getMyProjects();
      this.projects.set(projects);
    } catch {
      this.loadError.set('HOME.FEEDBACK.LOAD_ERROR_FALLBACK');
    } finally {
      this.isLoading.set(false);
    }
  }
}