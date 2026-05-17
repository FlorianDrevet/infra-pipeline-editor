import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { provideRouter } from '@angular/router';
import { HomeComponent } from './home.component';
import { ProjectService } from '../../shared/services/project.service';
import { FavoritesService } from '../../shared/services/favorites.service';
import { RecentlyViewedService } from '../../shared/services/recently-viewed.service';

describe('HomeComponent', () => {
  let fixture: ComponentFixture<HomeComponent>;
  let component: HomeComponent;

  beforeEach(async () => {
    const projectServiceSpy = jasmine.createSpyObj('ProjectService', ['getMyProjects']);
    projectServiceSpy.getMyProjects.and.returnValue(Promise.resolve([]));

    const favoritesServiceSpy = jasmine.createSpyObj('FavoritesService', ['isFavorite', 'toggle']);
    favoritesServiceSpy.isFavorite.and.returnValue(false);

    const recentlyViewedSpy = jasmine.createSpyObj('RecentlyViewedService', ['validateAndRefresh'], {
      recentItems: signal([]),
    });
    recentlyViewedSpy.validateAndRefresh.and.returnValue(Promise.resolve());

    await TestBed.configureTestingModule({
      imports: [HomeComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        { provide: ProjectService, useValue: projectServiceSpy },
        { provide: FavoritesService, useValue: favoritesServiceSpy },
        { provide: RecentlyViewedService, useValue: recentlyViewedSpy },
        { provide: MatDialog, useValue: jasmine.createSpyObj('MatDialog', ['open']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
