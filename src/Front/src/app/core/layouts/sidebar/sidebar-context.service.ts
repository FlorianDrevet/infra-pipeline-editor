import { Injectable, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter, map } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { FavoritesService } from '../../../shared/services/favorites.service';
import { RecentlyViewedService } from '../../../shared/services/recently-viewed.service';
import { CONFIG_DETAIL_ROUTE_TABS, PROJECT_DETAIL_ROUTE_TABS } from '../../../shared/enums/detail-route-tabs';

const CONFIG_ROUTE_PATTERN = /^\/config\//;
const PROJECT_ROUTE_PATTERN = /^\/projects\/[^/]+/;

export type SidebarMode = 'global' | 'project' | 'config';

export interface SidebarContextItem {
  readonly id: string;
  readonly icon: string;
  readonly labelKey: string;
  readonly routerLink: string;
  readonly queryParams?: {
    readonly tab: string;
  };
  readonly exact?: boolean;
  readonly badge?: string;
  readonly section?: string;
}

export interface SidebarContextState {
  readonly mode: SidebarMode;
  readonly backLabel?: string;
  readonly backLink?: string;
  readonly contextTitle?: string;
  readonly contextSubtitle?: string;
  readonly items: readonly SidebarContextItem[];
}

@Injectable({ providedIn: 'root' })
export class SidebarContextService {
  private readonly router = inject(Router);
  private readonly favoritesService = inject(FavoritesService);
  private readonly recentlyViewedService = inject(RecentlyViewedService);

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map((e) => e.urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );

  private readonly _projectName = signal<string>('');
  private readonly _projectId = signal<string>('');
  private readonly _configName = signal<string>('');
  private readonly _configId = signal<string>('');
  private readonly _configProjectId = signal<string>('');
  private readonly _configProjectIsMultiRepo = signal(false);

  readonly mode = computed<SidebarMode>(() => {
    const url = this.currentUrl();
    if (CONFIG_ROUTE_PATTERN.exec(url)) return 'config';
    if (PROJECT_ROUTE_PATTERN.exec(url)) return 'project';
    return 'global';
  });

  readonly contextState = computed<SidebarContextState>(() => {
    const mode = this.mode();
    switch (mode) {
      case 'project':
        return this.buildProjectContext();
      case 'config':
        return this.buildConfigContext();
      default:
        return this.buildGlobalContext();
    }
  });

  readonly favoriteIds = this.favoritesService.favorites;
  readonly recentItems = this.recentlyViewedService.recentItems;

  setProjectContext(id: string, name: string): void {
    this._projectId.set(id);
    this._projectName.set(name);
  }

  setConfigContext(id: string, name: string, projectId: string, isProjectMultiRepo = false): void {
    this._configId.set(id);
    this._configName.set(name);
    this._configProjectId.set(projectId);
    this._configProjectIsMultiRepo.set(isProjectMultiRepo);
  }

  private buildGlobalContext(): SidebarContextState {
    return {
      mode: 'global',
      items: [
        { id: 'home', icon: 'home', labelKey: 'SIDEBAR.HOME', routerLink: '/', exact: true },
        { id: 'projects', icon: 'folder', labelKey: 'SIDEBAR.PROJECTS', routerLink: '/projects', exact: false },
        { id: 'settings', icon: 'settings', labelKey: 'SIDEBAR.SETTINGS', routerLink: '/settings', exact: false },
      ],
    };
  }

  private buildProjectContext(): SidebarContextState {
    const id = this._projectId();
    return {
      mode: 'project',
      backLabel: 'SIDEBAR.BACK_PROJECTS',
      backLink: '/projects',
      contextTitle: this._projectName() || 'Project',
      items: [
        { id: 'configs', icon: 'settings', labelKey: 'SIDEBAR.PROJECT.CONFIGURATIONS', routerLink: `/projects/${id}`, exact: true, section: 'define' },
        { id: 'environments', icon: 'cloud_queue', labelKey: 'SIDEBAR.PROJECT.ENVIRONMENTS', routerLink: `/projects/${id}`, queryParams: { tab: PROJECT_DETAIL_ROUTE_TABS.environments }, exact: false, section: 'define' },
        { id: 'naming', icon: 'label', labelKey: 'SIDEBAR.PROJECT.NAMING', routerLink: `/projects/${id}`, queryParams: { tab: PROJECT_DETAIL_ROUTE_TABS.naming }, exact: false, section: 'define' },
        { id: 'variables', icon: 'library_books', labelKey: 'SIDEBAR.PROJECT.VARIABLES', routerLink: `/projects/${id}`, queryParams: { tab: PROJECT_DETAIL_ROUTE_TABS.variables }, exact: false, section: 'define' },
        { id: 'generation', icon: 'play_circle', labelKey: 'SIDEBAR.PROJECT.GENERATION', routerLink: `/projects/${id}/generate`, exact: false, section: 'generate' },
        { id: 'members', icon: 'group', labelKey: 'SIDEBAR.PROJECT.MEMBERS', routerLink: `/projects/${id}`, queryParams: { tab: PROJECT_DETAIL_ROUTE_TABS.members }, exact: false, section: 'manage' },
        { id: 'project-settings', icon: 'tune', labelKey: 'SIDEBAR.PROJECT.SETTINGS', routerLink: `/projects/${id}`, queryParams: { tab: PROJECT_DETAIL_ROUTE_TABS.settings }, exact: false, section: 'manage' },
      ],
    };
  }

  private buildConfigContext(): SidebarContextState {
    const id = this._configId();
    const projectId = this._configProjectId();
    const items: SidebarContextItem[] = [
      { id: 'resources', icon: 'dns', labelKey: 'SIDEBAR.CONFIG.RESOURCES', routerLink: `/config/${id}`, exact: true, section: 'define' },
      { id: 'tags', icon: 'label_important', labelKey: 'SIDEBAR.CONFIG.TAGS', routerLink: `/config/${id}`, queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.tags }, exact: false, section: 'define' },
      { id: 'naming', icon: 'label', labelKey: 'SIDEBAR.CONFIG.NAMING', routerLink: `/config/${id}`, queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.naming }, exact: false, section: 'define' },
      { id: 'cross-config-refs', icon: 'link', labelKey: 'CONFIG_DETAIL.TABS.CROSS_CONFIG_REFS', routerLink: `/config/${id}`, queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs }, exact: false, section: 'manage' },
      { id: 'variables', icon: 'library_books', labelKey: 'CONFIG_DETAIL.TABS.PIPELINE_VARIABLES', routerLink: `/config/${id}`, queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.variables }, exact: false, section: 'manage' },
      { id: 'generation', icon: 'play_circle', labelKey: 'SIDEBAR.CONFIG.GENERATION', routerLink: `/config/${id}/generate`, exact: false, section: 'generate' },
    ];

    if (this._configProjectIsMultiRepo()) {
      items.push({
        id: 'git',
        icon: 'code',
        labelKey: 'CONFIG_DETAIL.TABS.GIT',
        routerLink: `/config/${id}`,
        queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.git },
        exact: false,
        section: 'manage',
      });
    }

    return {
      mode: 'config',
      backLabel: 'SIDEBAR.BACK_PROJECT',
      backLink: projectId ? `/projects/${projectId}` : '/projects',
      contextTitle: this._configName() || 'Configuration',
      items,
    };
  }
}
