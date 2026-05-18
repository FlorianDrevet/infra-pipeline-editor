import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { SidebarContextService } from './sidebar-context.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, TranslateModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent implements OnInit {
  private readonly context = inject(SidebarContextService);
  private readonly projectService = inject(ProjectService);

  protected readonly contextState = this.context.contextState;
  protected readonly mode = this.context.mode;
  protected readonly favoriteIds = this.context.favoriteIds;
  protected readonly recentItems = this.context.recentItems;
  protected readonly projects = signal<ProjectResponse[]>([]);
  protected readonly favoriteProjects = computed(() =>
    this.projects()
      .filter((project) => this.favoriteIds().includes(project.id))
      .slice(0, 4)
  );

  protected readonly defineItems = computed(() =>
    this.contextState().items.filter((i) => i.section === 'define')
  );
  protected readonly generateItems = computed(() =>
    this.contextState().items.filter((i) => i.section === 'generate')
  );
  protected readonly manageItems = computed(() =>
    this.contextState().items.filter((i) => i.section === 'manage')
  );
  protected readonly globalItems = computed(() =>
    this.contextState().items.filter((i) => !i.section)
  );

  public ngOnInit(): void {
    void this.loadProjects();
  }

  private async loadProjects(): Promise<void> {
    try {
      const projects = await this.projectService.getMyProjects();
      this.projects.set(projects);
    } catch {
      this.projects.set([]);
    }
  }
}
