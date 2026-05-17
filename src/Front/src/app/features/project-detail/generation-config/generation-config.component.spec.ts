import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';
import { GenerationConfigComponent } from './generation-config.component';
import { ProjectService } from '../../../shared/services/project.service';
import { SidebarContextService } from '../../../core/layouts/sidebar/sidebar-context.service';

describe('GenerationConfigComponent', () => {
  let fixture: ComponentFixture<GenerationConfigComponent>;
  let component: GenerationConfigComponent;

  const mockProjectService = jasmine.createSpyObj('ProjectService', ['getProject', 'getProjectConfigs']);
  const mockSidebarContextService = jasmine.createSpyObj('SidebarContextService', ['setProjectContext']);

  beforeEach(async () => {
    mockProjectService.getProject.and.returnValue(Promise.resolve({ id: 'p1', name: 'Test', repositories: [], layoutPreset: 'AllInOne' }));
    mockProjectService.getProjectConfigs.and.returnValue(Promise.resolve([]));

    await TestBed.configureTestingModule({
      imports: [GenerationConfigComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => 'p1' } } },
        },
        { provide: ProjectService, useValue: mockProjectService },
        { provide: SidebarContextService, useValue: mockSidebarContextService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GenerationConfigComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
