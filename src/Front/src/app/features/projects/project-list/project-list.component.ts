import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { LowerCasePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { AuthenticationService } from '../../../shared/services/authentication.service';
import { CreateProjectDialogComponent } from '../create-project-dialog/create-project-dialog.component';
import { PROJECT_ROLE_ICONS } from '../enums/project-role.enum';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [LowerCasePipe, TranslateModule, RouterLink, MatDialogModule, MatIconModule],
  templateUrl: './project-list.component.html',
  styleUrl: './project-list.component.scss',
})
export class ProjectListComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  protected readonly projects = signal<ProjectResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadErrorKey = signal('');
  protected readonly searchQuery = signal('');

  protected readonly filteredProjects = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const list = this.projects();
    if (!query) return list;
    return list.filter(
      (p) =>
        p.name.toLowerCase().includes(query) ||
        (p.description?.toLowerCase().includes(query) ?? false)
    );
  });

  public async ngOnInit(): Promise<void> {
    await this.loadProjects();
  }

  protected getRoleIcon(role: string): string {
    return PROJECT_ROLE_ICONS[role] ?? 'person';
  }

  protected getMyRole(project: ProjectResponse): string | null {
    const entraId = this.authService.getMsalAccount?.localAccountId;
    if (!entraId) return null;
    const member = project.members.find((m) => m.entraId === entraId);
    return member?.role ?? null;
  }

  protected openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateProjectDialogComponent, {
      width: '480px',
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.projects.update((list) => [result, ...list]);
      }
    });
  }

  protected onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
  }

  protected navigateToProject(id: string): void {
    this.router.navigate(['/projects', id]);
  }

  protected async refreshProjects(): Promise<void> {
    await this.loadProjects();
  }

  private async loadProjects(): Promise<void> {
    this.isLoading.set(true);
    this.loadErrorKey.set('');

    try {
      const result = await this.projectService.getAll();
      this.projects.set(result);
    } catch {
      this.loadErrorKey.set('PROJECTS.STATE.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }
}
