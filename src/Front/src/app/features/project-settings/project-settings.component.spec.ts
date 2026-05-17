import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectSettingsComponent } from './project-settings.component';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';

describe('ProjectSettingsComponent', () => {
  let fixture: ComponentFixture<ProjectSettingsComponent>;
  let component: ProjectSettingsComponent;

  const mockProjectService = jasmine.createSpyObj('ProjectService', ['getProject', 'setAgentPool']);
  const mockAuthService = jasmine.createSpyObj('AuthenticationService', [], { getMsalAccount: null });
  const mockPageContext = jasmine.createSpyObj('PageContextService', ['setBreadcrumb', 'clear']);
  const mockSidebarContext = jasmine.createSpyObj('SidebarContextService', ['setProjectContext']);

  beforeEach(async () => {
    mockProjectService.getProject.and.returnValue(
      Promise.resolve({ id: 'p1', name: 'Test', members: [], agentPoolName: null })
    );

    await TestBed.configureTestingModule({
      imports: [ProjectSettingsComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'p1' } } },
        },
        { provide: ProjectService, useValue: mockProjectService },
        { provide: AuthenticationService, useValue: mockAuthService },
        { provide: PageContextService, useValue: mockPageContext },
        { provide: SidebarContextService, useValue: mockSidebarContext },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj('MatSnackBar', ['open']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectSettingsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
