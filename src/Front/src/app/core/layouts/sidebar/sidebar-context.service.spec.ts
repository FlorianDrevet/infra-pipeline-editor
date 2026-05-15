import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';

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

  it('Given_ProjectMode_When_ContextStateIsBuilt_Then_ContextualItemsExposeDistinctTabTargets', async () => {
    service.setProjectContext('project-123', 'Project 123');
    await router.navigateByUrl('/projects/project-123?tab=members');

    const items = readItems(service);
    const configsItem = findItem(items, 'configs');
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
    expect(findItem(items, 'members')).toEqual(jasmine.objectContaining({
      routerLink: '/projects/project-123',
      queryParams: { tab: 'members' },
    }));
    expect(findItem(items, 'git')).toEqual(jasmine.objectContaining({
      routerLink: '/projects/project-123',
      queryParams: { tab: 'repositories' },
    }));
    expect(findItem(items, 'project-settings')).toEqual(jasmine.objectContaining({
      routerLink: '/projects/project-123',
      queryParams: { tab: 'settings' },
    }));
    expect(generationItem.routerLink).toBe('/projects/project-123/generate');
    expect(generationItem.queryParams).toBeUndefined();
  });

  it('Given_ConfigMode_When_ContextStateIsBuilt_Then_ContextualItemsExposeDistinctTabTargets', async () => {
    service.setConfigContext('config-456', 'Config 456', 'project-123');
    await router.navigateByUrl('/config/config-456?tab=tags');

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
    expect(generationItem.routerLink).toBe('/config/config-456/generate');
    expect(generationItem.queryParams).toBeUndefined();
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

function findItem(items: readonly SidebarContextTargetItem[], id: string): SidebarContextTargetItem {
  const item = items.find((candidate) => candidate.id === id);

  if (!item) {
    throw new Error(`missing sidebar item ${id}`);
  }

  return item;
}