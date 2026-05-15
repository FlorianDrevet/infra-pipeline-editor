import { Component, computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { CONFIG_DETAIL_ROUTE_TABS } from '../../../shared/enums/detail-route-tabs';
import { FavoritesService } from '../../../shared/services/favorites.service';
import { RecentlyViewedItem, RecentlyViewedService } from '../../../shared/services/recently-viewed.service';
import { SidebarContextService } from './sidebar-context.service';
import { SidebarStateService } from './sidebar-state.service';
import { SidebarComponent } from './sidebar.component';

interface SidebarTestItem {
  readonly id: string;
  readonly icon: string;
  readonly labelKey: string;
  readonly routerLink: string;
  readonly exact?: boolean;
  readonly section?: string;
  readonly queryParams?: {
    readonly tab: string;
  };
}

interface SidebarTestState {
  readonly mode: 'project';
  readonly backLabel: string;
  readonly backLink: string;
  readonly contextTitle: string;
  readonly items: readonly SidebarTestItem[];
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

describe('SidebarComponent', () => {
  let fixture: ComponentFixture<SidebarComponent>;
  let router: Router;

  const contextState = signal<SidebarTestState>({
    mode: 'project',
    backLabel: 'Back',
    backLink: '/projects',
    contextTitle: 'Project 123',
    items: [
      { id: 'configs', icon: 'settings', labelKey: 'Configurations', routerLink: '/projects/project-123', exact: true, section: 'define' },
      { id: 'environments', icon: 'cloud_queue', labelKey: 'Environments', routerLink: '/projects/project-123', exact: false, section: 'define', queryParams: { tab: 'environments' } },
      { id: 'naming', icon: 'label', labelKey: 'Naming', routerLink: '/projects/project-123', exact: false, section: 'define', queryParams: { tab: 'naming' } },
      { id: 'members', icon: 'group', labelKey: 'Members', routerLink: '/projects/project-123', exact: false, section: 'manage', queryParams: { tab: 'members' } },
      { id: 'generation', icon: 'play_circle', labelKey: 'Generation', routerLink: '/projects/project-123/generate', exact: false, section: 'generate' },
    ],
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SidebarComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([
          { path: 'projects', component: DummyRouteComponent },
          { path: 'projects/:id', component: DummyRouteComponent },
          { path: 'projects/:id/generate', component: DummyRouteComponent },
        ]),
        {
          provide: SidebarStateService,
          useValue: {
            collapsed: signal(false).asReadonly(),
            width: signal('240px').asReadonly(),
            toggle: jasmine.createSpy('toggle'),
          } satisfies Pick<SidebarStateService, 'collapsed' | 'width' | 'toggle'>,
        },
        {
          provide: SidebarContextService,
          useValue: {
            contextState: contextState.asReadonly(),
            mode: computed(() => contextState().mode),
            favoriteIds: signal<string[]>([]).asReadonly(),
            recentItems: signal<RecentlyViewedItem[]>([]).asReadonly(),
          } satisfies Pick<SidebarContextService, 'contextState' | 'mode' | 'favoriteIds' | 'recentItems'>,
        },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(SidebarComponent);
  });

  it('Given_ContextualItemsWithTabTargets_When_Rendered_Then_LinksForwardQueryParamsIntoTheirHrefs', async () => {
    await router.navigateByUrl('/projects/project-123?tab=members');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(findLinkByLabel('Configurations').getAttribute('href')).toBe('/projects/project-123');
    expect(findLinkByLabel('Environments').getAttribute('href')).toBe('/projects/project-123?tab=environments');
    expect(findLinkByLabel('Naming').getAttribute('href')).toBe('/projects/project-123?tab=naming');
    expect(findLinkByLabel('Members').getAttribute('href')).toBe('/projects/project-123?tab=members');
    expect(findLinkByLabel('Generation').getAttribute('href')).toBe('/projects/project-123/generate');
  });

  it('Given_ProjectTabRoute_When_ActiveLinkClassesAreResolved_Then_OnlyTheMatchingContextualItemStaysActive', async () => {
    await router.navigateByUrl('/projects/project-123?tab=members');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const activeLinks = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('.sidebar__link.sidebar__link--active')
    );

    expect(activeLinks.length).toBe(1);
    expect(activeLinks[0].textContent).toContain('Members');
  });

  function findLinkByLabel(label: string): HTMLAnchorElement {
    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('.sidebar__link'));
    const link = links.find((candidate) => candidate.textContent?.includes(label));

    expect(link).withContext(`missing sidebar link ${label}`).toBeDefined();

    return link!;
  }
});

describe('SidebarComponent config mode', () => {
  let fixture: ComponentFixture<SidebarComponent>;
  let router: Router;
  let contextService: SidebarContextService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SidebarComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([
          { path: 'projects', component: DummyRouteComponent },
          { path: 'config/:id', component: DummyRouteComponent },
        ]),
        {
          provide: SidebarStateService,
          useValue: {
            collapsed: signal(false).asReadonly(),
            width: signal('240px').asReadonly(),
            toggle: jasmine.createSpy('toggle'),
          } satisfies Pick<SidebarStateService, 'collapsed' | 'width' | 'toggle'>,
        },
        {
          provide: FavoritesService,
          useValue: {
            favorites: signal<string[]>([]).asReadonly(),
          } satisfies Pick<FavoritesService, 'favorites'>,
        },
        {
          provide: RecentlyViewedService,
          useValue: {
            recentItems: signal<RecentlyViewedItem[]>([]).asReadonly(),
          } satisfies Pick<RecentlyViewedService, 'recentItems'>,
        },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    contextService = TestBed.inject(SidebarContextService);
    fixture = TestBed.createComponent(SidebarComponent);
  });

  it('Given_ConfigTabRoute_When_ActiveLinkClassesAreResolved_Then_OnlyTheMatchingContextualItemStaysActive', async () => {
    setConfigContext(contextService, 'config-456', 'Config 456', 'project-123', true);
    await router.navigateByUrl(`/config/config-456?tab=${CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs}`);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const activeLinks = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('.sidebar__link.sidebar__link--active')
    );

    expect(activeLinks.length).toBe(1);
    expect(activeLinks[0].getAttribute('href')).toBe(`/config/config-456?tab=${CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs}`);
  });
});

function setConfigContext(
  service: SidebarContextService,
  id: string,
  name: string,
  projectId: string,
  isProjectMultiRepo = false
): void {
  (service as ConfigContextCapableSidebarContextService).setConfigContext(id, name, projectId, isProjectMultiRepo);
}