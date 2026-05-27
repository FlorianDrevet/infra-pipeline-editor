import { Component, computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { CONFIG_DETAIL_ROUTE_TABS } from '../../../shared/enums/detail-route-tabs';
import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { FavoritesService } from '../../../shared/services/favorites.service';
import { ProjectService } from '../../../shared/services/project.service';
import { RecentlyViewedItem, RecentlyViewedService } from '../../../shared/services/recently-viewed.service';
import { SidebarContextService } from './sidebar-context.service';
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
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

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
      { id: 'generation-config', icon: 'settings', labelKey: 'Generation Config', routerLink: '/projects/project-123/generate/config', exact: true, section: 'generate' },
      { id: 'generation', icon: 'play_circle', labelKey: 'Generation', routerLink: '/projects/project-123/generate', exact: true, section: 'generate' },
    ],
  });

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getMyProjects']);
    projectServiceSpy.getMyProjects.and.resolveTo([]);

    await TestBed.configureTestingModule({
      imports: [SidebarComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([
          { path: 'projects', component: DummyRouteComponent },
          { path: 'projects/:id', component: DummyRouteComponent },
          { path: 'projects/:id/generate/config', component: DummyRouteComponent },
          { path: 'projects/:id/generate', component: DummyRouteComponent },
        ]),
        {
          provide: SidebarContextService,
          useValue: {
            contextState: contextState.asReadonly(),
            mode: computed(() => contextState().mode),
            favoriteIds: signal<string[]>([]).asReadonly(),
            recentItems: signal<RecentlyViewedItem[]>([]).asReadonly(),
          } satisfies Pick<SidebarContextService, 'contextState' | 'mode' | 'favoriteIds' | 'recentItems'>,
        },
        {
          provide: ProjectService,
          useValue: projectServiceSpy,
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
    expect(findLinkByLabel('Generation Config').getAttribute('href')).toBe('/projects/project-123/generate/config');
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

  it('Given_SidebarRendered_When_Rendered_Then_CollapseToggleIsNotExposed', () => {
    fixture.detectChanges();

    const collapseToggle = (fixture.nativeElement as HTMLElement).querySelector('.sidebar__toggle');

    expect(collapseToggle).toBeNull();
  });

  function findLinkByLabel(label: string): HTMLAnchorElement {
    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('.sidebar__link'));
    const link = links.find((candidate) => {
      const visibleLabel = candidate.querySelector('.sidebar__label')?.textContent?.trim();

      return visibleLabel === label;
    });

    expect(link).withContext(`missing sidebar link ${label}`).toBeDefined();

    return link!;
  }
});

describe('SidebarComponent config mode', () => {
  let fixture: ComponentFixture<SidebarComponent>;
  let router: Router;
  let contextService: SidebarContextService;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getMyProjects']);
    projectServiceSpy.getMyProjects.and.resolveTo([]);

    await TestBed.configureTestingModule({
      imports: [SidebarComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([
          { path: 'projects', component: DummyRouteComponent },
          { path: 'projects/:id/generate', component: DummyRouteComponent },
          { path: 'config/:id', component: DummyRouteComponent },
        ]),
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
        {
          provide: ProjectService,
          useValue: projectServiceSpy,
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

  it('Given_ConfigContext_When_Rendered_Then_GenerationLinkTargetsProjectGenerationPage', async () => {
    setConfigContext(contextService, 'config-456', 'Config 456', 'project-123');
    await router.navigateByUrl('/config/config-456');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const generationLink = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLAnchorElement>('.sidebar__link')
    ).find((candidate) => candidate.textContent?.includes('SIDEBAR.CONFIG.GENERATION'));

    expect(generationLink).toBeDefined();
    expect(generationLink?.getAttribute('href')).toBe('/projects/project-123/generate');
  });
});

describe('SidebarComponent global mode favorites', () => {
  let fixture: ComponentFixture<SidebarComponent>;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  const globalContextState = signal({
    mode: 'global' as const,
    items: [
      { id: 'home', icon: 'home', labelKey: 'SIDEBAR.HOME', routerLink: '/', exact: true },
      { id: 'projects', icon: 'folder', labelKey: 'SIDEBAR.PROJECTS', routerLink: '/projects', exact: false },
    ],
  });
  const favoriteIds = signal<string[]>(['project-1', 'project-2']);
  const recentItems = signal<RecentlyViewedItem[]>([
    { id: 'project-1', name: 'Alpha', type: 'project', timestamp: 1 },
    { id: 'config-1', name: 'Shared config', type: 'config', timestamp: 2 },
  ]);

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getMyProjects']);
    projectServiceSpy.getMyProjects.and.resolveTo([
      createProjectResponse('project-1', 'Alpha'),
      createProjectResponse('project-2', 'Beta'),
    ]);

    await TestBed.configureTestingModule({
      imports: [SidebarComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([
          { path: '', component: DummyRouteComponent },
          { path: 'projects', component: DummyRouteComponent },
          { path: 'projects/:id', component: DummyRouteComponent },
          { path: 'config/:id', component: DummyRouteComponent },
        ]),
        {
          provide: SidebarContextService,
          useValue: {
            contextState: globalContextState.asReadonly(),
            mode: computed(() => globalContextState().mode),
            favoriteIds: favoriteIds.asReadonly(),
            recentItems: recentItems.asReadonly(),
          } satisfies Pick<SidebarContextService, 'contextState' | 'mode' | 'favoriteIds' | 'recentItems'>,
        },
        {
          provide: ProjectService,
          useValue: projectServiceSpy,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SidebarComponent);
  });

  it('Given_FavoriteProjectsOutsideRecentItems_When_Rendered_Then_FavoritesSectionStillDisplaysAllFavoriteProjects', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const favoriteLinks = findSectionLinks(fixture, 'SIDEBAR.FAVORITES');

    expect(favoriteLinks.length).toBe(2);
    expect(favoriteLinks.map((link) => link.getAttribute('href'))).toEqual([
      '/projects/project-1',
      '/projects/project-2',
    ]);
    expect(favoriteLinks.map((link) => link.querySelector('.sidebar__label')?.textContent?.trim())).toEqual([
      'Alpha',
      'Beta',
    ]);
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

function findSectionLinks(
  fixture: ComponentFixture<SidebarComponent>,
  sectionLabel: string
): HTMLAnchorElement[] {
  const sections = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll<HTMLElement>('.sidebar__section'));
  const section = sections.find((candidate) => candidate.textContent?.includes(sectionLabel));

  expect(section).withContext(`missing sidebar section ${sectionLabel}`).toBeDefined();

  const list = section?.nextElementSibling;

  expect(list).withContext(`missing sidebar list for section ${sectionLabel}`).not.toBeNull();

  return Array.from(list!.querySelectorAll<HTMLAnchorElement>('.sidebar__link'));
}

function createProjectResponse(id: string, name: string): ProjectResponse {
  return {
    id,
    name,
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
  };
}