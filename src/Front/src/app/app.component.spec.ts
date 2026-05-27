import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { AppComponent } from './app.component';
import { NavigationComponent } from './core/layouts/navigation/navigation.component';
import { FooterComponent } from './core/layouts/footer/footer.component';
import { SidebarComponent } from './core/layouts/sidebar/sidebar.component';
import { UserPreferencesService } from './shared/services/user-preferences.service';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let component: AppComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: Router,
          useValue: {
            events: of(new NavigationEnd(0, '/login', '/login')),
            url: '/login',
            navigate: jasmine.createSpy('navigate'),
          },
        },
        { provide: UserPreferencesService, useValue: {} },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    })
      .overrideComponent(AppComponent, {
        remove: { imports: [NavigationComponent, FooterComponent, SidebarComponent] },
      })
      .compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
