import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

import { CONFIG_DETAIL_ROUTE_TABS, PROJECT_DETAIL_ROUTE_TABS } from '../../../shared/enums/detail-route-tabs';
import { RecentlyViewedItem, RecentlyViewedService } from '../../../shared/services/recently-viewed.service';
import { FavoritesService } from '../../../shared/services/favorites.service';
import { SidebarContextService } from './sidebar-context.service';

interface SidebarContextTargetItem {
  readonly id: string;
  readonly routerLink: string;
  readonly queryParams?: {
    readonly tab: string;
  };
}

type ConfigContextCapableSidebarContextService = SidebarContextService & {
  setConfigContext(id: string, name: string, projectId: string, isProjectMultiRepo: boolean): void;
};

@Component({
  standalone: true,
  template: '',
})
class DummyRouteComponent {
}

describe('SidebarContextService', () => {
  let router: Router;
  let service: SidebarContextService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'projects/:id', component: DummyRouteComponent },
          { path: 'projects/:id/generate/config', component: DummyRouteComponent },
          { path: 'projects/:id/generate', component: DummyRouteComponent },
          { path: 'config/:id', component: DummyRouteComponent },
          { path: 'config/:id/generate', component: DummyRouteComponent },
        ]),
        {
          provide: FavoritesService,
          useValue: createFavoritesServiceStub(),
        },
        {
          provide: RecentlyViewedService,
          useValue: createRecentlyViewedServiceStub(),
        },
      ],
    });

    router = TestBed.inject(Router);
    service = TestBed.inject(SidebarContextService);
  });

  it('Given_ProjectMode_When_ContextStateIsBuilt_Then_ContextualItemsExposeDistinctTabTargetsWithoutGitShortcut', async () => {
    service.setProjectContext('project-123', 'Project 123');
    await router.navigateByUrl('/projects/project-123?tab=members');

    const items = readItems(service);
    const configsItem = findItem(items, 'configs');
    const generationConfigItem = findItem(items, 'generation-config');
    const generationItem = findItem(items, 'generation');

    expect(configsItem.routerLink).toBe('/projects/project-123');
    expect(configsItem.queryParams).toBeUndefined();
    expect(findItem(items, 'environments')).toEqual(jasmine.objectContaining({
      routerLink: '/projects/project-123',
      queryParams: { tab: 'environments' },
    }));
    expect(findItem(items, 'naming')).toEqual(jasmine.objectContaining({
      routerLink: '/projects/project-123',
      queryParams: { tab: 'naming' },
    }));
    expect(findItem(items, 'variables')).toEqual(jasmine.objectContaining({
      routerLink: '/projects/project-123',
      queryParams: { tab: 'variables' },
    }));
    expect(findItem(items, 'members').routerLink).toBe('/projects/project-123/members');
    expect(findItem(items, 'members').queryParams).toBeUndefined();
    expect(items.some((item) => item.id === 'git')).toBeFalse();
    expect(findItem(items, 'project-settings').routerLink).toBe('/projects/project-123/settings');
    expect(findItem(items, 'project-settings').queryParams).toBeUndefined();
    expect(generationConfigItem.routerLink).toBe('/projects/project-123/generate/config');
    expect(generationConfigItem.queryParams).toBeUndefined();
    expect(generationItem.routerLink).toBe('/projects/project-123/generate');
    expect(generationItem.queryParams).toBeUndefined();
  });

  it('Given_ProjectMode_When_ContextStateIsBuilt_Then_GenerateSectionKeepsGenerationBeforeGenerationConfig', async () => {
    service.setProjectContext('project-123', 'Project 123');
    await router.navigateByUrl('/projects/project-123/generate');

    const generateItemIds = readItems(service)
      .filter((item) => item.id === 'generation' || item.id === 'generation-config')
      .map((item) => item.id);

    expect(generateItemIds).toEqual(['generation', 'generation-config']);
  });

  it('Given_ProjectDetailTabs_When_ProjectRouteTargetsAreEnumerated_Then_RepositoriesIsNotExposed', () => {
    expect('repositories' in PROJECT_DETAIL_ROUTE_TABS).toBeFalse();
  });

  it('Given_ConfigMode_When_ContextStateIsBuilt_Then_ContextualItemsExposeDistinctTabTargets', async () => {
    setConfigContext(service, 'config-456', 'Config 456', 'project-123');
    await router.navigateByUrl(`/config/config-456?tab=${CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs}`);

    const items = readItems(service);
    const resourcesItem = findItem(items, 'resources');
    const generationItem = findItem(items, 'generation');

    expect(resourcesItem.routerLink).toBe('/config/config-456');
    expect(resourcesItem.queryParams).toBeUndefined();
    expect(findItem(items, 'tags')).toEqual(jasmine.objectContaining({
      routerLink: '/config/config-456',
      queryParams: { tab: 'tags' },
    }));
    expect(findItem(items, 'naming')).toEqual(jasmine.objectContaining({
      routerLink: '/config/config-456',
      queryParams: { tab: 'naming' },
    }));
    expect(findItem(items, 'cross-config-refs')).toEqual(jasmine.objectContaining({
      routerLink: '/config/config-456',
      queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs },
    }));
    expect(findItem(items, 'variables')).toEqual(jasmine.objectContaining({
      routerLink: '/config/config-456',
      queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.variables },
    }));
    expect(items.some((item) => item.id === 'git')).toBeFalse();
    expect(generationItem.routerLink).toBe('/projects/project-123/generate');
    expect(generationItem.queryParams).toBeUndefined();
  });

  it('Given_ConfigModeWithoutProjectId_When_ContextStateIsBuilt_Then_GenerationFallsBackToConfigRoute', async () => {
    setConfigContext(service, 'config-456', 'Config 456', '');
    await router.navigateByUrl('/config/config-456');

    const items = readItems(service);

    expect(findItem(items, 'generation').routerLink).toBe('/config/config-456');
  });

  it('Given_MultiRepoConfigMode_When_ContextStateIsBuilt_Then_GitItemExposesDedicatedTabTarget', async () => {
    setConfigContext(service, 'config-456', 'Config 456', 'project-123', true);
    await router.navigateByUrl(`/config/config-456?tab=${CONFIG_DETAIL_ROUTE_TABS.git}`);

    const items = readItems(service);

    expect(findItem(items, 'git')).toEqual(jasmine.objectContaining({
      routerLink: '/config/config-456',
      queryParams: { tab: CONFIG_DETAIL_ROUTE_TABS.git },
    }));
  });
});

function createFavoritesServiceStub(): Pick<FavoritesService, 'favorites'> {
  return {
    favorites: signal<string[]>([]).asReadonly(),
  };
}

function createRecentlyViewedServiceStub(): Pick<RecentlyViewedService, 'recentItems'> {
  return {
    recentItems: signal<RecentlyViewedItem[]>([]).asReadonly(),
  };
}

function readItems(service: SidebarContextService): readonly SidebarContextTargetItem[] {
  return service.contextState().items;
}

function setConfigContext(
  service: SidebarContextService,
  id: string,
  name: string,
  projectId: string,
  isProjectMultiRepo = false
): void {
  (service as ConfigContextCapableSidebarContextService).setConfigContext(id, name, projectId, isProjectMultiRepo);
}

function findItem(items: readonly SidebarContextTargetItem[], id: string): SidebarContextTargetItem {
  const item = items.find((candidate) => candidate.id === id);

  if (!item) {
    throw new Error(`missing sidebar item ${id}`);
  }

  return item;
}