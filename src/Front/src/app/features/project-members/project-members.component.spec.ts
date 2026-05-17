import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectMembersComponent } from './project-members.component';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';

describe('ProjectMembersComponent', () => {
  let fixture: ComponentFixture<ProjectMembersComponent>;
  let component: ProjectMembersComponent;

  const mockProjectService = jasmine.createSpyObj('ProjectService', ['getProject', 'getUsers', 'updateMemberRole', 'removeMember']);
  const mockAuthService = jasmine.createSpyObj('AuthenticationService', [], { getMsalAccount: null });
  const mockPageContext = jasmine.createSpyObj('PageContextService', ['setBreadcrumb', 'clear']);
  const mockSidebarContext = jasmine.createSpyObj('SidebarContextService', ['setProjectContext']);

  beforeEach(async () => {
    mockProjectService.getProject.and.returnValue(
      Promise.resolve({ id: 'p1', name: 'Test', members: [] })
    );
    mockProjectService.getUsers.and.returnValue(Promise.resolve([]));

    await TestBed.configureTestingModule({
      imports: [ProjectMembersComponent, TranslateModule.forRoot()],
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
        { provide: MatDialog, useValue: jasmine.createSpyObj('MatDialog', ['open']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectMembersComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
