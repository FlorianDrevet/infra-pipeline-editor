import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import {
  DsButtonComponent,
  DsChipComponent,
  DsIconButtonComponent,
  DsSelectComponent,
  DsTextFieldComponent,
} from '../../shared/components/ds';
import { ProjectResponse } from '../../shared/interfaces/project.interface';
import { FavoritesService } from '../../shared/services/favorites.service';
import { LanguageService } from '../../shared/services/language.service';
import { ProjectService } from '../../shared/services/project.service';
import { ProjectsComponent } from './projects.component';

describe('ProjectsComponent', () => {
  let fixture: ComponentFixture<ProjectsComponent>;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let favoritesServiceSpy: jasmine.SpyObj<FavoritesService>;

  beforeEach(async () => {
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['getMyProjects']);
    favoritesServiceSpy = jasmine.createSpyObj<FavoritesService>('FavoritesService', ['isFavorite', 'toggle']);

    projectServiceSpy.getMyProjects.and.resolveTo([
      createProject({
        id: 'project-alpha',
        name: 'Project Alpha',
        description: 'Primary platform workspace',
        memberCount: 1,
        environmentCount: 1,
      }),
      createProject({
        id: 'project-beta',
        name: 'Project Beta',
        memberCount: 3,
        environmentCount: 2,
      }),
    ]);

    favoritesServiceSpy.isFavorite.and.callFake((projectId: string) => projectId === 'project-alpha');

    await TestBed.configureTestingModule({
      imports: [ProjectsComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        {
          provide: ProjectService,
          useValue: projectServiceSpy,
        },
        {
          provide: FavoritesService,
          useValue: favoritesServiceSpy,
        },
        {
          provide: MatDialog,
          useValue: jasmine.createSpyObj<MatDialog>('MatDialog', ['open']),
        },
      ],
    }).compileComponents();

    const translateService = TestBed.inject(TranslateService);
    const languageService = TestBed.inject(LanguageService);

    translateService.setTranslation(
      'fr',
      {
        PROJECTS: {
          SORT: {
            BY_NAME: 'Nom',
            BY_MEMBERS: 'Membres',
            BY_FAVORITES: 'Favoris',
          },
        },
      },
      true
    );
    translateService.setTranslation(
      'en',
      {
        PROJECTS: {
          SORT: {
            BY_NAME: 'Name',
            BY_MEMBERS: 'Members',
            BY_FAVORITES: 'Favorites',
          },
        },
      },
      true
    );
    languageService.setLanguage('fr');

    fixture = TestBed.createComponent(ProjectsComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('renders toolbar and card controls with design-system components', () => {
    expect(fixture.debugElement.query(By.directive(DsTextFieldComponent))).not.toBeNull();
    expect(fixture.debugElement.query(By.directive(DsSelectComponent))).not.toBeNull();

    const dsButtons = fixture.debugElement.queryAll(By.directive(DsButtonComponent));
    const dsIconButtons = fixture.debugElement.queryAll(By.directive(DsIconButtonComponent));
    const dsChips = fixture.debugElement.queryAll(By.directive(DsChipComponent));

    expect(dsButtons.length).toBeGreaterThanOrEqual(2);
    expect(dsIconButtons.length).toBe(2);
    expect(dsChips.length).toBe(4);
  });

  it('sorts cards when the sort selection changes', () => {
    const component = fixture.componentInstance as unknown as {
      onSortByChange: (value: string | number | null) => void;
    };

    component.onSortByChange('members');
    fixture.detectChanges();

    expect(readRenderedProjectNames(fixture)).toEqual(['Project Beta', 'Project Alpha']);
  });

  it('updates sort option labels when the active language changes', () => {
    const component = fixture.componentInstance as unknown as {
      sortOptions: () => Array<{ label: string }>;
    };
    const languageService = TestBed.inject(LanguageService);

    expect(component.sortOptions().map((option) => option.label)).toEqual(['Nom', 'Membres', 'Favoris']);

    languageService.setLanguage('en');
    fixture.detectChanges();

    expect(component.sortOptions().map((option) => option.label)).toEqual([
      'Name',
      'Members',
      'Favorites',
    ]);
  });

  it('renders project cards with native links instead of link roles', () => {
    const root = fixture.nativeElement as HTMLElement;

    expect(root.querySelector('[role="link"]')).toBeNull();
    expect(root.querySelectorAll('.project-card__link').length).toBe(2);
  });

  it('renders metadata in a dedicated footer container even without a description', () => {
    const projectCards = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('.project-card')
    ) as HTMLElement[];
    const cardWithoutDescription = projectCards.find((card) =>
      card.querySelector('h3')?.textContent?.trim() === 'Project Beta'
    );

    expect(cardWithoutDescription).withContext('missing Project Beta card').toBeDefined();
    expect(cardWithoutDescription?.classList.contains('project-card--without-description')).toBeTrue();
    expect(cardWithoutDescription?.querySelector('.project-card__body')).not.toBeNull();
    expect(cardWithoutDescription?.querySelector('.project-card__meta')).not.toBeNull();
    expect(cardWithoutDescription?.lastElementChild?.classList.contains('project-card__meta')).toBeTrue();
  });
});

function readRenderedProjectNames(fixture: ComponentFixture<ProjectsComponent>): string[] {
  return Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('.project-card h3')).map(
    (title) => title.textContent?.trim() ?? ''
  );
}

interface ProjectStubOptions {
  id: string;
  name: string;
  memberCount: number;
  environmentCount: number;
  description?: string;
}

function createProject(options: ProjectStubOptions): ProjectResponse {
  return {
    id: options.id,
    name: options.name,
    description: options.description,
    members: Array.from({ length: options.memberCount }, (_, index) => ({
      id: `${options.id}-member-${index}`,
      userId: `user-${index}`,
      entraId: `entra-${index}`,
      role: 'Owner',
      firstName: 'User',
      lastName: `${index}`,
    })),
    environmentDefinitions: Array.from(
      { length: options.environmentCount },
      (_, index) => ({ id: `${options.id}-env-${index}` }) as ProjectResponse['environmentDefinitions'][number]
    ),
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    usedResourceTypes: [],
    repositories: [],
    layoutPreset: 'AllInOne',
  };
}