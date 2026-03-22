import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { ProjectService } from '../../shared/services/project.service';
import { LanguageService } from '../../shared/services/language.service';
import { CreateProjectDialogComponent } from './create-project-dialog/create-project-dialog.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TranslateModule, RouterLink, MatDialogModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly languageService = inject(LanguageService);
  private readonly dialog = inject(MatDialog);

  protected readonly projects = signal<ProjectResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly createdProjectName = signal('');
  protected readonly searchQuery = signal('');
  protected readonly sortBy = signal<'name' | 'members'>('name');

  protected readonly totalMemberCount = computed(() =>
    this.projects().reduce(
      (count, project) => count + project.members.length,
      0
    )
  );

  protected readonly filteredProjects = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const sort = this.sortBy();
    let filtered = this.projects();

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
        default:
          return a.name.localeCompare(b.name);
      }
    });
  });

  public async ngOnInit(): Promise<void> {
    await this.loadProjects();
  }

  protected async refreshProjects(): Promise<void> {
    await this.loadProjects();
  }

  protected openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateProjectDialogComponent, {
      width: '480px',
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.projects.update((projects) => [result, ...projects]);
        this.createdProjectName.set(result.name);
      }
    });
  }

  protected onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
  }

  protected onSortChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sortBy.set(select.value as 'name' | 'members');
  }

  private async loadProjects(): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const projects = await this.projectService.getMyProjects();
      this.projects.set(projects);
    } catch (error) {
      this.loadError.set(
        this.extractErrorMessage(
          error,
          'HOME.FEEDBACK.LOAD_ERROR_FALLBACK'
        )
      );
    } finally {
      this.isLoading.set(false);
    }
  }

  private extractErrorMessage(error: unknown, fallbackKey: string): string {
    if (typeof error !== 'object' || error === null) {
      return this.languageService.translate(fallbackKey);
    }

    const maybeResponse = error as {
      response?: {
        data?: {
          title?: string;
          detail?: string;
          errors?: Record<string, string[]>;
        };
      };
      message?: string;
    };

    const details = maybeResponse.response?.data;

    if (details?.errors) {
      const validationMessages = Object.values(details.errors).flat();
      if (validationMessages.length > 0) {
        return validationMessages[0] ?? this.languageService.translate(fallbackKey);
      }
    }

    return (
      details?.detail ||
      details?.title ||
      maybeResponse.message ||
      this.languageService.translate(fallbackKey)
    );
  }
}